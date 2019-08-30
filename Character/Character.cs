using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterState
{
    None,
    Incoming,
    Idle,
    Prefight,
    CastSpell,
    ReceiveSpell,
    Dying,
    Dead,
    BattleEndAction,
    JumpBack,
    SpecificAnim,
}

public enum CharacterSpecificAnimation
{
    Taunt,
}

public class Character
{
    //Statics
    public delegate void Deleg_CharacterStateChange(Character character, CharacterState newState);
    public event Deleg_CharacterStateChange Event_StateChange;

    public delegate void Deleg_ChakraAppeared();
    public event Deleg_ChakraAppeared Event_ChakraApeared;

    public delegate void Deleg_SpellCasted(Spell spell);
    public event Deleg_SpellCasted Event_SpellCast;

    public delegate void Deleg_SpellReceived(SpellInfo spellInfo,int effectIndex, Character reciever);
    public event Deleg_SpellReceived Event_SpellReceived;

    public delegate void Deleg_SelectAsTarget(Character character);
    public event Deleg_SelectAsTarget Event_SelectAsTarget;

    public delegate void Deleg_CharacterClicked(Character character);
    public event Deleg_CharacterClicked Event_OnClick;

    private const int percentOfHpToGoToDeadStepA = 50;
    private const int percentOfHpToGoToDeadStepB = 20;
    private const float enemyPosNearest = 2.8f;
    private const float enemyPosFarthest = 5f;
    private const float heroUIHideDelay = 0.17f;
    private const float chakraUIShowDelay = 0.00f;

    //Private
    private int                     _waitingSpellEffectIndex = -1;
    private bool                    _jumped;
    private bool                    _becomingChakra;
    private bool                    _isChakra;
    private bool                    _isActive;
    private bool                    _isSelected;
    private bool                    _dieWithHit;
    private bool                    _goesToChakraMode = false;
    private bool                    _isEligibleTarget;
    private bool                    _isTargetable = true; //Means if it can be target at all or not (like Ent)
    private Party                   _relatedParty = null;
    private Spell                   _currentSpell;
    private Vector3                 _startingPoint;
    private Vector3                 _startingScale;
    private CharInfo                _charInfo;
    private SpellInfo               _waitingSpellInfo = null;
    private List<int>               _currentEligibleSpells;
    private BattleFlags             _previousFlag = BattleFlags.None;
    private BattleObjStats          _currentStats;
    private CharacterState          _curState = CharacterState.None;
    private CharacterVisual         _charVisual = null;
    private ExtraSpellHandler       _extraSpellHandler;
    private CharacterStartingPoint  _formationPoint;

    protected bool                  _isHero = false;

    //Props  
    public long                     id                  { get { return _charInfo.uniqueID; } }
    public bool                     isDead              { get { return _currentStats.hp <= 0; } }
    public bool                     isIdle              { get { return _curState == CharacterState.Idle; } }
    public bool                     isTaunt             { get { return _currentStats.flags == BattleFlags.Taunt; } }
    public bool                     isFliped            { get { return _charVisual.isFliped; } }
    public bool                     isChakra            { get { return _isChakra; } }
    public bool                     isHero              { get { return _isHero; } }
    public bool                     isActive            { get { return _isActive; } }
    public bool                     isTargetable        { get { return _isTargetable; } }
    public PartySide                side                { get { return _relatedParty.side; } }
    public CharInfo                 charInfo            { get { return _charInfo; } }
    public Divine.Moniker           moniker             { get { return _charInfo.moniker; } }
    public CharacterState           state               { get { return _curState; } }

    public Vector3                  scale               { get { return _charVisual.transform.localScale; } }
    public Vector3                  position_forHit     { get { return _charVisual.hitPoint; } }
    public Vector3                  position_pivot      { get { return _charVisual.transform.position; } }
    public CharacterStartingPoint   charFormationPoint  { get { return _formationPoint; } }

    

    //Private Methods
    private void SetOwnerParty(Party relatedParty)
    {
        _relatedParty = relatedParty;
    }

    private void SetState_Incoming(Vector3 targetPos, Vector3 targetScale)
    {
        _curState = CharacterState.Incoming;
        
        List<CharVisualActionInfo> acts = new List<CharVisualActionInfo>();

        CharVisualActionInfo act = new CharVisualActionInfo();
        act.actionType = CharVisualActionType.Move;
        act.moveTarget = targetPos;
        act.moveScale = targetScale;
        act.moveTime = .5f;

        acts.Add(act);

        _charVisual.StartActionList(acts);        

        if (Event_StateChange != null)
            Event_StateChange(this, _curState);
    }

    private void SetState_Idle(bool shouldBeNormal)
    {
        if (_curState == CharacterState.Dead)
            return;

        CharacterState lastState = _curState;
        _curState = CharacterState.Idle;

        List<CharVisualActionInfo> acts = new List<CharVisualActionInfo>();

        CharVisualActionInfo act = new CharVisualActionInfo();
        act.actionType = CharVisualActionType.Idle;

        if (shouldBeNormal)
            act.idleType = SetIdleTypeInNormalMode();
        else
            act.idleType = lastState == CharacterState.Prefight ? CharVisualIdleType.Prefight : SetIdleTypeInNormalMode();

        acts.Add(act);

        _charVisual.StartActionList(acts);

        if (Event_StateChange != null)
            Event_StateChange(this, _curState);
    }
    private void SetState_Prefight()
    {
        _curState = CharacterState.Prefight;

        List<CharVisualActionInfo> acts = new List<CharVisualActionInfo>();

        CharVisualActionInfo act = new CharVisualActionInfo();
        act.actionType = CharVisualActionType.Prefight;
        acts.Add(act);

        _charVisual.StartActionList(acts);

        if (Event_StateChange != null)
            Event_StateChange(this, _curState);
    }
    private void Setstate_JumpBack()
    {
        _curState = CharacterState.JumpBack;

        List<CharVisualActionInfo> acts = new List<CharVisualActionInfo>();

        CharVisualActionInfo act = new CharVisualActionInfo();
        act.actionType = CharVisualActionType.JumpBack;
        act.moveTarget = _startingPoint;
        act.moveScale = _startingScale;
        acts.Add(act);

        _charVisual.StartActionList(acts);

        if (Event_StateChange != null)
            Event_StateChange(this, _curState);
    }
    private void SetState_Dying(bool shownAnimBefore)
    {
        _curState = CharacterState.Dying;

        List<CharVisualActionInfo> acts = new List<CharVisualActionInfo>();

        _charVisual.charUI.SetActivity(false);

        _extraSpellHandler.Die();

        CharVisualActionInfo act = null;

        if (!shownAnimBefore)
        {
            act = new CharVisualActionInfo();
            act.actionType = CharVisualActionType.Dying;
            acts.Add(act);
        }

        HeroInfo hInfo = _charInfo as HeroInfo;
        bool isHero = hInfo != null;

        if (!isChakra && !isHero)
        {
            act = new CharVisualActionInfo();
            act.actionType = CharVisualActionType.Fading;
            acts.Add(act);

            act = new CharVisualActionInfo();
            act.actionType = CharVisualActionType.AppearGrave;
            act.moveTarget = _startingPoint;
            acts.Add(act);
        }
        else
            _curState = CharacterState.Dead;

        _charVisual.StartActionList(acts);

        if (Event_StateChange != null)
            Event_StateChange(this, _curState);
    }
    private void SetState_Dead()
    {
        _curState = CharacterState.Dead;

        _extraSpellHandler.Die();

        if (Event_StateChange != null)
            Event_StateChange(this, _curState);
    }
    private void SetState_CastSpell(SpellInfo spellInfo, int consumedAP)
    {
        _curState = CharacterState.CastSpell;

        DivineDebug.Log("CastSpell: Casted character " + moniker + " SpellType: " + spellInfo.spellType);

        if (spellInfo.spellType == SpellType.DamageReturn)
        {            
            _extraSpellHandler.CastSpell(spellInfo, OnCharVisualSpellCast, OnCharVisualActionsFinish);
        }
        else
        {
            List<CharVisualActionInfo> acts = new List<CharVisualActionInfo>();

            CharVisualActionInfo act = new CharVisualActionInfo();

            if (spellInfo.needTargetToComeNear)
            {
                act.actionType = CharVisualActionType.Jump;
                act.moveScale = _charVisual.transform.localScale + ScaleRatio(
                    spellInfo.spellEffectInfos[0].targetCharacter.charFormationPoint.formationIndex,
                    charFormationPoint.formationIndex);
                act.moveTarget = TargetPos(act.moveScale.x, spellInfo.spellEffectInfos[0].targetCharacter.position_pivot) + new Vector3(0f, 0f, -0.05f);
                act.spellInfo = spellInfo;

                acts.Add(act);

                _jumped = true;

                _charVisual.charUI.SetActivity(false);
            }

            act = new CharVisualActionInfo();
            act.actionType = CharVisualActionType.CastSpell;
            act.spellInfo = spellInfo;
            acts.Add(act);

            if (spellInfo.spellType == SpellType.Chakra)
            {
                act = new CharVisualActionInfo();
                act.actionType = CharVisualActionType.Fading;
                act.itsAlive = true;
                acts.Add(act);

                _goesToChakraMode = true;
                
                _charVisual.SetUIActivity(false, heroUIHideDelay);

                _extraSpellHandler.Die();
            }

            _charVisual.StartActionList(acts);

            _relatedParty.SetActionPoint(_relatedParty.actionPoint - consumedAP, false);
        }

        if (Event_StateChange != null)
            Event_StateChange(this, _curState);
    }
    private void SetState_SpecificAnim(CharacterSpecificAnimation animType)
    {
        if (_curState != CharacterState.Idle)
            return;

        if (_isSelected)
            return;

        _curState = CharacterState.SpecificAnim;

        DivineDebug.Log("Run Specific Anim " + moniker + " AnimType: " + animType.ToString());

        List<CharVisualActionInfo> acts = new List<CharVisualActionInfo>();

        CharVisualActionInfo act = new CharVisualActionInfo();
        
        act = new CharVisualActionInfo();
        act.actionType = CharVisualActionType.SpecificAnimation;
        act.specificAnimType = animType;
        acts.Add(act);
        
        _charVisual.StartActionList(acts);
        
        if (Event_StateChange != null)
            Event_StateChange(this, _curState);
    }
    private void SetState_ReceiveSpell(SpellInfo spellInfo, int effectIndex, bool shouldWaitForLastStateDone)
    {
        if (shouldWaitForLastStateDone)
        {
            _waitingSpellInfo = spellInfo;
            _waitingSpellEffectIndex = effectIndex;
            return;
        }

        RemoveWaitingSpells();

        _curState = CharacterState.ReceiveSpell;

        if (Event_SpellReceived != null)
            Event_SpellReceived(spellInfo, effectIndex , this);

        if (spellInfo.spellEffectInfos[effectIndex].effectOnCharacter == SpellEffectOnChar.Revive)
        {
            Revive();
            return;
        }

        //Here comes Chakra! AAAA...
        if (spellInfo.spellEffectInfos[effectIndex].effectOnCharacter == SpellEffectOnChar.Appear)
        {
            _becomingChakra = true;
            
            SetActive(true, false);

            if (Event_ChakraApeared != null && isChakra)
                Event_ChakraApeared();
        }

        _extraSpellHandler.SpellReceieved(spellInfo.spellEffectInfos[effectIndex].effectOnCharacter);      

        List<CharVisualActionInfo> acts = new List<CharVisualActionInfo>();

        DivineDebug.Log("ReceiveSpell: " + spellInfo.spellEffectInfos[effectIndex].effectOnCharacter + " Reciever: " + moniker);

        CharVisualActionInfo act = new CharVisualActionInfo();
        act.actionType = CharVisualActionType.ReceiveSpell;
        act.spellInfo = spellInfo;
        act.spellEffectIndex = effectIndex;

        if (spellInfo.spellEffectInfos[effectIndex].finalCharacterStats.hp <= 0)
        {
            act.isDied = true;
            _dieWithHit = true;
        }
        else
        {
            act.isDied = false;
            _dieWithHit = false;
        }

        acts.Add(act);

        _charVisual.StartActionList(acts);

        if (Event_StateChange != null)
            Event_StateChange(this, _curState);
    }
    private void SetState_BattleEndingAction(bool isVictorious)
    {
        _curState = CharacterState.BattleEndAction;

        List<CharVisualActionInfo> acts = new List<CharVisualActionInfo>();

        CharVisualActionInfo act = new CharVisualActionInfo();
        act.actionType = CharVisualActionType.BattleEnding;
        act.isVictorious = isVictorious;

        acts.Add(act);

        _charVisual.StartActionList(acts);

        if (Event_StateChange != null)
            Event_StateChange(this, _curState);
    }
    
    private Vector3 TargetPos(float xScale, Vector3 targetPos)
    {
        float enemyScaledPos = enemyPosNearest +
                                    ((xScale - 1) * (enemyPosFarthest - enemyPosNearest) / .7f); //between 2.8 and 5
        float addedVal = targetPos.x > 0 ? -enemyScaledPos : enemyScaledPos;
        Vector3 pos = new Vector3(targetPos.x + addedVal, targetPos.y, targetPos.z);
        return pos;
    }
    private Vector3 ScaleRatio(int targetFormationIndex, int selfFormationIndex)
    {
        int scaleLevel = targetFormationIndex - selfFormationIndex;

        if (scaleLevel == 0) return Vector3.zero;

        return Vector3.one * (scaleLevel * .1f);
    }

    private void SetActive(bool isActive, bool changeUIActivity = true)
    {
        _isActive = isActive;
        _charVisual.SetActivity(isActive, changeUIActivity);
    }

    private void RemoveWaitingSpells()
    {
        _waitingSpellInfo = null;
        _waitingSpellEffectIndex = -1;
    }

    private void Revive()
    {
        List<CharVisualActionInfo> acts = new List<CharVisualActionInfo>();

        DivineDebug.Log("Revive Happened for this character: " + moniker);

        _charVisual.charUI.SetActivity(true);

        CharVisualActionInfo act = new CharVisualActionInfo();
        act.actionType = CharVisualActionType.Revive;

        acts.Add(act);

        _charVisual.StartActionList(acts);

        if (Event_StateChange != null)
            Event_StateChange(this, _curState);
    }

    private CharVisualIdleType SetIdleTypeInNormalMode()
    {
        if (_currentStats.hp < (_currentStats.maxHp * percentOfHpToGoToDeadStepB / 100))
            return CharVisualIdleType.DeadStepB;

        if (_currentStats.hp < (_currentStats.maxHp * percentOfHpToGoToDeadStepA / 100))
            return CharVisualIdleType.DeadStepA;

        return CharVisualIdleType.Normal;
    }


    //UI Methods
    private void ResetCharacterUIAndEffect()
    {
        ShowUI_StatChange_HpOrShield(false);

        Show_EffectForFlagChange(_currentStats.flags, false);
    }
    private void ShowUpdatingStatsVisual(SpellEffectInfo spellEffectInfo)
    {
        switch (spellEffectInfo.effectOnCharacter)
        {
            case SpellEffectOnChar.Miss:
                _charVisual.charUI.newCharHUD.AddBuzz(BuzzType.Miss);
                return;
            case SpellEffectOnChar.Dodge:
                _charVisual.charUI.newCharHUD.AddBuzz(BuzzType.Dodge);
                return;

            case SpellEffectOnChar.SeriousDamage:
                _charVisual.charUI.newCharHUD.AddBuzz(BuzzType.Critical);
                break;
        }

        for (int i = 0; i < spellEffectInfo.singleStatChanges.Count; i++)
        {
            SpellSingleStatChangeInfo singleStatChange = spellEffectInfo.singleStatChanges[i];

            DivineDebug.Log("On Receive Damage" + singleStatChange.charStatChangeType.ToString() + "  " + singleStatChange.intVal);

            switch (singleStatChange.charStatChangeType)
            {
                case SpellSingleStatChangeType.curDamageValChange:
                    ShowUI_StatChange_DMG(singleStatChange.intVal, spellEffectInfo.effectOnCharacter);
                    break;
            }
        }
                
        ShowUI_StatChange_HpOrShield(spellEffectInfo.isMultiPart);
        Show_EffectForFlagChange(_currentStats.flags, spellEffectInfo.effectOnCharacter == SpellEffectOnChar.None);
    }

    private void ShowUI_StatChange_DMG(int val, SpellEffectOnChar effectType)
    {
        switch (effectType)
        {
            case SpellEffectOnChar.Nerf:
                _charVisual.charUI.newCharHUD.AddBuzz(BuzzType.AttackDec);
                break;
            case SpellEffectOnChar.Buff:
                _charVisual.charUI.newCharHUD.AddBuzz(BuzzType.AttackInc);
                break;
        }
    }

    private void ShowUI_StatChange_HpOrShield(bool isMultipart)
    {
        _charVisual.charUI.newCharHUD.ResetHPAndShield(_currentStats.hp, _currentStats.shield, isMultipart);
    }

    private void Show_EffectForFlagChange(BattleFlags newFlag, bool isFromTurnStat)
    {
        DivineDebug.Log("Show_EffectForFlagChange New: " + newFlag.ToString() + " Prev: " + _previousFlag.ToString());

        _charVisual.charUI.newCharHUD.ResetActiveEffects(newFlag);

        _extraSpellHandler.SetExtraSpells(_previousFlag, newFlag, position_pivot, scale, _charVisual, isFromTurnStat);

        _charVisual.charEffects.HandleChangingFlag(newFlag, _previousFlag, scale);
    }

    private bool ShouldWait(SpellInfo spellInfo, int effectIndex)
    {
        bool result = false;

        //bool shouldWait = spellInfo.owner == spellInfo.spellEffectInfos[effectIndex].targetCharacter;

        if (_curState == CharacterState.CastSpell)
            result = true;

        SpellEffectOnChar type = spellInfo.spellEffectInfos[effectIndex].effectOnCharacter;
        if (_curState == CharacterState.ReceiveSpell &&
                            type != SpellEffectOnChar.NormalDamage && type != SpellEffectOnChar.SeriousDamage)
            result = true;

        return result;
    }

    //Public Methods
    public void Init(Party owner, CharInfo charInfo, CharacterVisual charVisual)
    {
        _extraSpellHandler = new ExtraSpellHandler();

        _charVisual = charVisual;
        _charVisual.Event_ActionsFinished += OnCharVisualActionsFinish;
        _charVisual.Event_SpellCast += OnCharVisualSpellCast;

        _charInfo = charInfo;
        
        _currentStats = _charInfo.baseStats;
        DivineDebug.Log(moniker + " Created with hp: " + _currentStats.hp + " Shield: " + _currentStats.shield);

        SetOwnerParty(owner);
        
        _charVisual.charUI.Init(
            _charVisual.Tr_HUD_HelperPos,
            _charVisual.Tr_SelectionButton_HelperPos,
            _charVisual.Tr_decal_HelperPos);
                
        _charVisual.charUI.SubscribeForSelectTarget(SelectedAsTarget);

        _charVisual.charUI.newCharHUD.Init(_currentStats.maxHp, _currentStats.hp, _currentStats.maxShield, _currentStats.shield, _charInfo.level, side == PartySide.Player, isHero);

        ResetCharacterUIAndEffect();

        _isActive = charInfo.enableInStart;
    }

    public void SelectedAsTarget()
    {
        if (Event_OnClick != null)
            Event_OnClick(this);

        DivineDebug.Log("Selected Char:" + this.moniker + " Is eligible: " + _isEligibleTarget);

        if (!_isEligibleTarget)
        {
            _relatedParty.NonEligibleTargetSelected(this);
            return;
        }

        _charVisual.charUI.SetSelected();

        if (Event_SelectAsTarget != null)
            Event_SelectAsTarget(this);
    }
    public void SetTargetStatus(bool isTarget)
    {
        _isEligibleTarget = isTarget;

        _charVisual.charUI.SetTargetStatus(_isEligibleTarget);
    }
    public void SetIsTargetable(bool isTargetable)
    {
        _isTargetable = isTargetable;
    }
    public void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;

        if (isSelected && _relatedParty.side == PartySide.Player)
            _charVisual.charUI.charDecalManager.SetSelect();
        else
            _charVisual.charUI.charDecalManager.HideAll();

        if (_isSelected)
            SetState_Prefight();
        else
            SetState_Idle(true);
    }
    public void CastSpell(SpellInfo spellInfo, int consumedAP)
    {
        SetState_CastSpell(spellInfo, consumedAP);
    }
    public void ReceiveSpell(SpellInfo spellInfo, int effectIndex)
    {
        if (isDead && spellInfo.spellEffectInfos[effectIndex].effectOnCharacter != SpellEffectOnChar.Revive)
        {
            RemoveWaitingSpells();
            return;
        }

        SetState_ReceiveSpell(spellInfo, effectIndex, ShouldWait(spellInfo, effectIndex));

        UpdateStats(spellInfo.spellEffectInfos[effectIndex], (effectIndex == spellInfo.spellEffectInfos.Count - 1));
    }
    public void RunSpecificAnimation(CharacterSpecificAnimation animType)
    {
        SetState_SpecificAnim(animType);
    }
    public void BattleEndingAction(bool isVictorious)
    {
        SetState_BattleEndingAction(isVictorious);
    }
    public void ComeToStartPoint(Vector3 startingPoint, Vector3 startingScale, CharacterStartingPoint formationPoint)
    {
        _startingPoint = startingPoint;
        _startingScale = startingScale;
        _formationPoint = formationPoint;
        
        SetActive(_isActive);

        SetState_Incoming(startingPoint, startingScale);
    }
    public void UpdateStats(SpellEffectInfo effectInfo, bool isFinal)
    {
        DivineDebug.Log("Update Stats for: ");
        DivineDebug.Log("CurrentHP: " + _currentStats.hp + " FinalHP: " + effectInfo.finalCharacterStats.hp + " IsFinal: " + isFinal);

        if ((effectInfo.isMultiPart && isFinal) || !effectInfo.isMultiPart)
        {
            _currentStats = effectInfo.finalCharacterStats;

            DivineDebug.Log("moniker " + moniker + 
                " hp: " + _currentStats.hp +
                " shield: " + _currentStats.shield +
                " damage: " + _currentStats.damage);

            charInfo.SetBaseStats(_currentStats.hp, _currentStats.maxHp, _currentStats.damage,
                                    _currentStats.shield, _currentStats.maxShield);
        }
        else
        {
            for (int i = 0; i < effectInfo.singleStatChanges.Count; i++)
            {
                switch (effectInfo.singleStatChanges[i].charStatChangeType)
                {
                    case SpellSingleStatChangeType.None:
                        break;
                    case SpellSingleStatChangeType.curHPValChange:
                        _currentStats.SetHP(_currentStats.hp + effectInfo.singleStatChanges[i].intVal);
                        break;
                    case SpellSingleStatChangeType.curShieldValChange:
                        _currentStats.SetShield(_currentStats.shield + effectInfo.singleStatChanges[i].intVal);
                        break;
                    case SpellSingleStatChangeType.curDamageValChange:
                        _currentStats.damage += effectInfo.singleStatChanges[i].intVal;
                        break;
                    case SpellSingleStatChangeType.curFlagValChange:
                        _currentStats.flags = (BattleFlags)effectInfo.singleStatChanges[i].intVal;
                        break;
                    default:
                        break;
                }
            }
        }       

        DivineDebug.Log("After Update CurrentHP: " + _currentStats.hp +
                            " Shield: " + _currentStats.shield + " Flag: " + _currentStats.flags.ToString());

        ShowUpdatingStatsVisual(effectInfo);

        if (_curState == CharacterState.Idle && _currentStats.hp <= 0)
            SetState_Dying(false);

        _previousFlag = _currentStats.flags;
    }
    public void AddActionPoint(int actionPoint)
    {
        _relatedParty.SetActionPoint(_relatedParty.actionPoint + actionPoint, false);
    }

    public void SpellSelectionDone()
    {
        _charVisual.charUI.charDecalManager.HideAll();
    }

    public void SetIsChakra(bool isChakra)
    {
        _isChakra = isChakra;
        _isActive = false;
    }

    public void DestroyIt()
    {
        DivineDebug.Log("Deleting Character named: + " + moniker);

        Event_SelectAsTarget = null;
        Event_SpellCast = null;
        Event_StateChange = null;

        _extraSpellHandler.Die();
        _extraSpellHandler = null;

        _charVisual.DestroyIt();
        _charVisual = null;

        _charInfo = null;
        _relatedParty = null;
    }
    public int GetHP()
    {
        return _currentStats.hp;
    }
    
    public void StartDialogue(string dialogueKey, Action onEndCallback)
    {
        _charVisual.charUI.newCharHUD.StartIngameDialogues(dialogueKey, onEndCallback);
    }
    public void StartDialogue(List<string> dialogueKeys, Action onEndCallback)
    {
        _charVisual.charUI.newCharHUD.StartIngameDialogues(dialogueKeys, onEndCallback);
    }
    public void EndDialogue()
    {
        _charVisual.charUI.newCharHUD.EndIngameDialogues();
    }

    ///EventHandlers
    private void OnCharVisualActionsFinish()
    {
        if (_curState == CharacterState.CastSpell)
        {
            if (_currentSpell != null)
                _currentSpell.HappenedAgain(true);
        }

        if (_waitingSpellEffectIndex > -1)
        {
            SetState_ReceiveSpell(_waitingSpellInfo, _waitingSpellEffectIndex, false);
            return;
        }

        if (_jumped)
        {
            _jumped = false;
            Setstate_JumpBack();
            return;
        }

        if (_becomingChakra)
        {
            _becomingChakra = false;
            _charVisual.SetUIActivity(true, chakraUIShowDelay);
        }

        if(_curState == CharacterState.JumpBack)
            _charVisual.charUI.SetActivity(true);

        if (isDead)
        {
            if (_curState != CharacterState.Dying && !isChakra && !isHero)
                SetState_Dying(_dieWithHit);
            else
                SetState_Dead();
        }
        else
            SetState_Idle(false);

        if (_goesToChakraMode)
            SetActive(false, false);
    }
    private void OnCharVisualSpellCast(SpellInfo spellInfo)
    {
        if (_currentSpell == null)
        {
            _currentSpell = SpellBuilder.Build(spellInfo, _charVisual.GetSpellTransData());

            _currentSpell.Event_SpellWillDestroy += OnCurrentSpellDestroy;

            if (Event_SpellCast != null)
                Event_SpellCast(_currentSpell);
        }
        else
            _currentSpell.HappenedAgain();
    }
    private void OnCurrentSpellDestroy(Spell spell)
    {
        _currentSpell.Event_SpellWillDestroy -= OnCurrentSpellDestroy;

        _currentSpell = null;
    }
}