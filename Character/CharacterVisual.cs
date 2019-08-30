using UnityEngine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System;

public enum CharVisualActionType
{
    None,
    Move,
    Jump,
    JumpBack,
    Prefight,
    Idle,
    CastSpell,
    ReceiveSpell,
    Dying,
    Fading,
    AppearGrave,
    BattleEnding,
    SpecificAnimation,
    Revive,
}
public enum CharVisualIdleType
{
    Normal,
    DeadStepA,
    DeadStepB,
    Prefight,
}

public class CharVisualActionInfo
{
    //Props
    public int spellEffectIndex { get; internal set; }
    public bool isVictorious { get; set; }
    public bool isDied { get; set; }
    public float moveTime { get; set; }
    public Vector3 moveTarget { get; set; }
    public Vector3 moveScale { get; set; }
    public SpellInfo spellInfo { get; set; }
    public CharVisualIdleType idleType { get; set; }
    public CharVisualActionType actionType { get; set; }
    public CharacterSpecificAnimation specificAnimType { get; internal set; }
    public bool itsAlive { get; internal set; }
}

public class CharacterVisual : MonoBehaviour
{
    //Statics
    public delegate void Deleg_ActionsFinished();
    public event Deleg_ActionsFinished Event_ActionsFinished;

    public delegate void Deleg_SpellCast(SpellInfo curAction);
    public event Deleg_SpellCast Event_SpellCast;

    public delegate void Deleg_DraggingReached();
    public event Deleg_DraggingReached Event_DraggingReached;

    //Serialized
    [SerializeField]
    private Divine.Moniker _moniker;

    [SerializeField]
    private SkeletonAnimation _spineSkeleton;

    [SerializeField]
    [SpineEvent]
    private string _event_CastSpell;

    [SerializeField]
    private Transform _tr_graphicRoot;
    [SerializeField]
    private Transform _tr_HUD_HelperPos;
    [SerializeField]
    private Transform _tr_SelectionButton_HelperPos;
    [SerializeField]
    private Transform _tr_decal_HelperPos;
    [SerializeField]
    private Transform _tr_hitPoint;

    [SerializeField]
    private float _scaleFactor = 1.3f;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Move;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Idle;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Idle_Prefight;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Idle_DeadStep_A;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Idle_DeadStep_B;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Prefight;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Prepare;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_CriticalAttack;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Spell_Magic_A;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Spell_Magic_B;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Spell_Magic_C;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Spell_Magic_D;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Spell_Chakra;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Spell_SecretReveal;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_ReceiveDamage_Low;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_ReceiveDamage_High;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_LethalDamage;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Revive;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Appear;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_ReceiveBuff;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_ReceiveNerf;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Die;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Victory;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Lose;

    [SerializeField]
    [SpineAnimation]
    private string[] _anims_Taunt;

    //Private
    private List<CharVisualActionInfo> _actions;
    private CharVisualActionInfo _currentAction = null;
    private Spine.TrackEntry _curAnimTrackEntry;
    private int _curActionIndex = 0;    
    private float _fadingTime = .5f;
    private bool _isSpellHappened = false;
    private bool _isRunningActions = false;
    private CharUI _charUI;
    private Jump _jump;
    private Movement _movement;
    private GameObject _graveObj = null;
    private SpellEffect _charSpellEffect;
    private AudioManager _audioManager;
    private CharacterEffects _charEffects;
    private IExtraVisualLogic _extraVisualLogic;

    private float _dragToPosDefaultTime = 0.3f;

    //Props
    public bool             isFliped                       { get { return _spineSkeleton.skeleton.FlipX; } }
    public CharUI           charUI                         { get { return _charUI; } }
    public Vector3          hitPoint                       { get { return _tr_hitPoint.position; } }
    public Transform        Tr_HUD_HelperPos               { get { return _tr_HUD_HelperPos; } }
    public Transform        Tr_decal_HelperPos             { get { return _tr_decal_HelperPos; } }
    public Transform        Tr_SelectionButton_HelperPos   { get { return _tr_SelectionButton_HelperPos; } }
    public Divine.Moniker   moniker                        { get { return _moniker; } }
    public CharacterEffects charEffects                    { get { return _charEffects; } }

    public SkeletonAnimation spineSkeleton
    {
        get
        {
            return _spineSkeleton;
        }
    }

    //Base Methods
    private void Awake()
    {
        _charUI = Instantiate<CharUI>(PrefabProvider_Battle.instance.CharUIPrefab, transform, false);

        _charEffects = Instantiate<CharacterEffects>(PrefabProvider_Battle.instance.CharEffectsPrefab, transform, false);
        _charEffects.Init(moniker);

        _charSpellEffect = GetComponent<SpellEffect>();

        _movement = GetComponent<Movement>();

        _jump = GetComponent<Jump>();

        _audioManager = AudioManager.instance;

        _extraVisualLogic = GetComponent<IExtraVisualLogic>();
    }


    //Private Methods
    private void StartNextAction()
    {
        if (_curActionIndex >= _actions.Count - 1)
            return;

        _curActionIndex++;

        StartAction(_actions[_curActionIndex]);
    }
    private void StartAction(CharVisualActionInfo action)
    {
        _currentAction = action;

        if (_extraVisualLogic != null && !_extraVisualLogic.CanRunMainLogic(_currentAction))
            return;

        switch (_currentAction.actionType)
        {
            case CharVisualActionType.Idle:
                StartAct_Idle();
                break;

            case CharVisualActionType.Move:
                StartAct_Move();
                break;

            case CharVisualActionType.Jump:
                StartAct_Jump();
                break;

            case CharVisualActionType.JumpBack:
                StartAct_JumpBack();
                break;

            case CharVisualActionType.Prefight:
                StartAct_PreFight();
                break;

            case CharVisualActionType.CastSpell:
                StartAct_CastSpell();
                break;

            case CharVisualActionType.ReceiveSpell:
                StartAct_ReceiveSpell();
                break;

            case CharVisualActionType.SpecificAnimation:
                StartAct_RunSpecificAnim();
                break;

            case CharVisualActionType.Dying:
                StartAct_Dying();
                break;

            case CharVisualActionType.Fading:
                StartAct_Fading();
                break;

            case CharVisualActionType.AppearGrave:
                StartAct_AppearGrave();
                break;

            case CharVisualActionType.Revive:
                StartAct_Revive();
                break;

            case CharVisualActionType.BattleEnding:
                StartAct_BattleEndingAction();
                break;
        }

        if (_extraVisualLogic != null)
            _extraVisualLogic.RunExtraLogic(_currentAction);
    }
    private void SetCurActionFinished()
    {
        if (_curActionIndex < _actions.Count - 1)
            StartNextAction();
        else
            SetAllActionsFinished();
    }
    private void SetAllActionsFinished()
    {
        _isRunningActions = false;

        if (Event_ActionsFinished != null)
            Event_ActionsFinished();
    }

    private void StartAct_Idle()
    {
        string[] an = null;

        switch (_currentAction.idleType)
        {
            case CharVisualIdleType.Normal:
                an = _anims_Idle;
                break;

            case CharVisualIdleType.DeadStepA:
                an = _anims_Idle_DeadStep_A;
                break;

            case CharVisualIdleType.DeadStepB:
                an = _anims_Idle_DeadStep_B;
                break;

            case CharVisualIdleType.Prefight:
                an = _anims_Idle_Prefight;
                break;
        }

        PlayAnim(an, true);
    }
    private void StartAct_Move()
    {
        MoveInfo moveInfo = new MoveInfo(_currentAction.moveTarget, _currentAction.moveScale, _currentAction.moveTime);
        StartMoving(moveInfo);
    }
    private void FinishAct_Move()
    {
        SetCurActionFinished();
    }
    private void StartAct_Jump()
    {
        MoveInfo moveInfo = new MoveInfo(_currentAction.moveTarget, _currentAction.moveScale,
                                            _currentAction.moveTime);

        _jump.StartIt(moniker, moveInfo, _currentAction.spellInfo.charSpellsIndex, _currentAction.spellInfo.isCritical, FinishAct_Jump);
    }
    private void FinishAct_Jump()
    {
        SetCurActionFinished();
    }
    private void StartAct_JumpBack()
    {
        MoveInfo moveInfo = new MoveInfo(_currentAction.moveTarget, _currentAction.moveScale,
                                            _currentAction.moveTime);

        _jump.GoBack(moveInfo, FinishAct_JumpBack);
    }
    private void FinishAct_JumpBack()
    {
        SetCurActionFinished();
    }
    private void StartAct_PreFight()
    {
        PlayAnim(_anims_Prefight, false);
        _curAnimTrackEntry.Complete += OnPrefightAnimFinish;
    }
    private void FinishAct_Prefight()
    {
        SetCurActionFinished();
    }
    private void StartAct_CastSpell()
    {
        string[] an = null;
        CharacterAudioType audioType = CharacterAudioType.None;

        if (_currentAction.spellInfo.isCritical && _anims_CriticalAttack != null && _anims_CriticalAttack.Length > 0)        
            audioType = CharacterAudioType.spell_Crit;        
                
        switch (_currentAction.spellInfo.spellType)
        {
            case SpellType.Chakra:
                an = _anims_Spell_Chakra;
                audioType = CharacterAudioType.chakra;
                break;

            case SpellType.Magic:
                an = FindProperAnimForMagic();
                break;

            case SpellType.Secret:
                an = _anims_Spell_SecretReveal;
                audioType = CharacterAudioType.secret;
                break;
        }

        PlayAnim(an);

        if (audioType == CharacterAudioType.None)
            _audioManager.PlayCharSpell(moniker, _currentAction.spellInfo.charSpellsIndex);
        else
            _audioManager.PlayChar(moniker, audioType);

        _isSpellHappened = false;
        _curAnimTrackEntry.Complete += OnSpellCastAnimFinish;
        _curAnimTrackEntry.Event += OnSpineEventCapture;

        if (_charSpellEffect != null)
            _charSpellEffect.HandleSpellEvent(_curAnimTrackEntry, _currentAction.spellInfo);
    }

    private void FinishAct_CastSpell()
    {
        SetCurActionFinished();
    }
    private void StartAct_ReceiveSpell()
    {
        SpellEffectInfo effectOfSpell = _currentAction.spellInfo.spellEffectInfos[_currentAction.spellEffectIndex];
        DivineDebug.Log("ReceiveSpell: EffectOnCharacter: " + effectOfSpell.effectOnCharacter);

        string[] an = null;
        bool shouldPlaySFX = false;
        CharacterAudioType sfxType = CharacterAudioType.None;

        if (_currentAction.isDied)
        {
            an = _anims_LethalDamage;

            shouldPlaySFX = true;
            sfxType = CharacterAudioType.die;
        }
        else
            switch (effectOfSpell.effectOnCharacter)
            {
                case SpellEffectOnChar.NormalDamage:
                    if (_currentAction.spellInfo.damageType == DamageType.High)                    
                        an = _anims_ReceiveDamage_High;                    
                    else                    
                        an = _anims_ReceiveDamage_Low;                    
                    break;
                case SpellEffectOnChar.SeriousDamage:
                    an = _anims_ReceiveDamage_High;
                    break;
                case SpellEffectOnChar.Appear:
                    an = _anims_Appear;
                    break;

                case SpellEffectOnChar.Nerf:
                    an = _anims_ReceiveNerf;
                    shouldPlaySFX = true;
                    sfxType = CharacterAudioType.Nerf;
                    break;
                case SpellEffectOnChar.Buff:
                    an = _anims_ReceiveBuff;
                    shouldPlaySFX = true;
                    sfxType = CharacterAudioType.Buff;
                    break;
                case SpellEffectOnChar.Miss:
                    an = _anims_ReceiveBuff;
                    break;
                case SpellEffectOnChar.Dodge:
                    an = _anims_ReceiveBuff;
                    break;
                case SpellEffectOnChar.Burn:
                    an = _anims_ReceiveNerf;
                    break;
                case SpellEffectOnChar.Fear:
                    an = _anims_ReceiveNerf;
                    break;
                case SpellEffectOnChar.Taunt:
                    an = null;
                    break;

                case SpellEffectOnChar.Prepare:
                    an = _anims_Prepare;
                    break;
            }

        if (an != null)
        {
            PlayAnim(an);
            _curAnimTrackEntry.Complete += OnSpellReceiveAnimFinish;

            if (shouldPlaySFX)
                _audioManager.PlayChar(moniker, sfxType);
        }
        else
            OnSpellReceiveAnimFinish(null);

        SpellImpact();
    }
    private void FinishAct_ReceiveSpell()
    {
        SetCurActionFinished();
    }
    private void StartAct_RunSpecificAnim()
    {
        string[] an = null;

        switch (_currentAction.specificAnimType)
        {
            case CharacterSpecificAnimation.Taunt:
                an = _anims_Taunt;
                break;            
        }

        if (an == null || (an != null && an.Length == 0))
        {
            DivineDebug.Log("No Anim for this specific animation: " + _currentAction.specificAnimType.ToString());

            FinishAct_RunSpecificAnim();
            return;
        }

        PlayAnim(an);
        _curAnimTrackEntry.Complete += OnSpecificAnimFinish;        
    }
    private void FinishAct_RunSpecificAnim()
    {
        SetCurActionFinished();
    }
    private void StartAct_Dying()
    {
        PlayAnim(_anims_Die);
        _curAnimTrackEntry.Complete += OnDieAnimFinish;
    }
    private void FinishAct_Dying()
    {
        SetCurActionFinished();
    }
    protected virtual void StartAct_Fading()
    {
        if (!_currentAction.itsAlive)        
            charEffects.ShowEffect_Grave();        

        LeanTween.value(gameObject, _spineSkeleton.skeleton.a, 0, _fadingTime).
            setOnComplete(FinishAct_Fading).
            setOnUpdate((float val) =>
            {
                _spineSkeleton.skeleton.a = val;
            });
    }
    private void FinishAct_Fading()
    {
        SetCurActionFinished();
    }
    private void StartAct_AppearGrave()
    {
        _graveObj = Instantiate(PrefabProvider_Battle.instance.GetPrefab_Grave());
        _graveObj.transform.position = _currentAction.moveTarget + new Vector3(0f, 0f, -0.01f);
        
        _graveObj.GetComponent<Grave>().Appear(FinishAct_AppearGrave, _moniker.ToString(), _spineSkeleton.skeleton.flipX);
    }
    private void FinishAct_AppearGrave()
    {
        SetCurActionFinished();
    }
    private void StartAct_Revive()
    {
        _graveObj.GetComponent<Grave>().Revive(GraveReviveMoment);
    }
    private void FinishAct_Revive()
    {
        SetCurActionFinished();
    }
    private void StartAct_BattleEndingAction()
    {
        string[] an = null;

        if (_currentAction.isVictorious)
            an = _anims_Victory;
        else
            an = _anims_Lose;

        PlayAnim(an, true);
    }
    private void FinishAct_BattleEndingAction()
    {
        SetCurActionFinished();
    }

    private void StartMoving(MoveInfo moveInfo)
    {
        PlayAnim(_anims_Move, true);
        _movement.StartMovement(moveInfo, FinishAct_Move);
    }

    private void SetDragToPosFinished()
    {
        if (Event_DraggingReached != null)
            Event_DraggingReached();
    }
    private void PlayAnim(string[] anims, bool isLoop = false)
    {
        PlayAnim(Utilities.GetRandomArrayElement<string>(anims), isLoop);
    }
    private void PlayAnim(string anim, bool isLoop = false)
    {
        DivineDebug.Log("Playing Animation named: " + anim + " For this character: " + gameObject.name);
        _curAnimTrackEntry = _spineSkeleton.state.SetAnimation(Constants.SPINE_ROOT_LAYER_INDEX, anim, isLoop);
    }

    private void SpellHappened()
    {
        _isSpellHappened = true;

        if (Event_SpellCast != null)
            Event_SpellCast(_currentAction.spellInfo);
    }

    private void SpellImpact()
    {
        float shakeRatio = 0;
        float freezeTime = 0.025f;

        //In taunt it can be null
        if (_currentAction.spellInfo == null)
            return;

        SpellEffectInfo effectOfSpell = _currentAction.spellInfo.spellEffectInfos[_currentAction.spellEffectIndex];

        switch (_currentAction.spellInfo.spellImpact)
        {
            case SpellImpactType.Low:
                {
                    shakeRatio = .5f;
                    freezeTime = 0.05f;
                }
                break;
            case SpellImpactType.High:
                {
                    freezeTime = 0.07f;
                    shakeRatio = 1f;
                }
                break;
        }

        if (!_currentAction.spellInfo.needTargetToComeNear)
            freezeTime = 0;

        if (effectOfSpell.effectOnCharacter == SpellEffectOnChar.SeriousDamage)
            _currentAction.spellInfo.spellImpact = SpellImpactType.High;

        if (shakeRatio == .5f)
            MainCamera.instance.CameraShake(false);
        else if (shakeRatio == 1f)
            MainCamera.instance.CameraShake(true);
    }

    IEnumerator FreezeTime(object freezeTime)
    {
        // Target time scale amount
        Time.timeScale = 0.1f;

        // Target freeze time
        yield return new WaitForSecondsRealtime((float)freezeTime);

        Time.timeScale = 1;

        FreezeFinished();
    }

    private void GraveReviveMoment()
    {
        _spineSkeleton.state.SetEmptyAnimation(0, 0f);
        PlayAnim(_anims_Revive, false);
        _curAnimTrackEntry.Complete += OnReviveComplete;

        LeanTween.value(gameObject, _spineSkeleton.skeleton.a, 1, .3f).
            setOnUpdate((float val) =>
            {
                _spineSkeleton.skeleton.a = val;
            });
    }
    private void OnReviveComplete(Spine.TrackEntry trackEntry)
    {
        FinishAct_Revive();
    }

    private string[] FindProperAnimForMagic()
    {
        string[] an = null;

        if (_currentAction.spellInfo.isCritical && _anims_CriticalAttack != null && _anims_CriticalAttack.Length > 0)
            an = _anims_CriticalAttack;
        else
            switch (_currentAction.spellInfo.charSpellsIndex)
            {
                case 0:
                    an = _anims_Spell_Magic_A;
                    break;

                case 1:
                    an = _anims_Spell_Magic_B;
                    break;

                case 2:
                    an = _anims_Spell_Magic_C;
                    break;

                case 3:
                    an = _anims_Spell_Magic_D;
                    break;
            }

        return an;
    }

    protected virtual IEnumerator SetActivityWithDelay(bool isActive, bool changeUIActivity = true)
    {
        yield return new WaitForSeconds(.1f);

        _spineSkeleton.skeleton.a = isActive ? 1f : 0f;

        if (changeUIActivity)
            SetUIActivity(isActive);
    }    

    //Public Methods
    public void SetGraphicRootOrientation(Quaternion rotation)
    {
        if (rotation.eulerAngles.y == 180)
            _spineSkeleton.skeleton.FlipX = true;
    }

    public void StartActionList(List<CharVisualActionInfo> actions)
    {
        _actions = null;
        _actions = actions;

        _isRunningActions = true;
        _curActionIndex = -1;
        StartNextAction();
    }
    public void DragToPos(Vector3 position, Vector3 scale)
    {
        _movement.StartMovement(new MoveInfo(position, scale, _dragToPosDefaultTime), SetDragToPosFinished);
    }

    public void SetActivity(bool isActive, bool changeUIActivity = true)
    {
        StartCoroutine(SetActivityWithDelay(isActive, changeUIActivity));
    }

    public void SetUIActivity(bool isActive, float delay = 0)
    {
        _charUI.SetActivity(isActive, delay);
    }

    public SpellCastTransData GetSpellTransData()
    {
        SpellCastHelper spellCastHelper = GetComponent<SpellCastHelper>();
        SpellCastTransData stData = new SpellCastTransData();

        if (spellCastHelper == null)
        {
            stData.position = Vector3.zero;
            stData.rotationAmount = 0;
        }
        else
            stData = spellCastHelper.GetProperCastTransData();

        stData.scale = transform.localScale * _scaleFactor;

        return stData;
    }

    public void DestroyIt()
    {
        Event_SpellCast = null;
        Event_ActionsFinished = null;
        Event_DraggingReached = null;

        if (_graveObj != null)
            Destroy(_graveObj);

        Destroy(gameObject);
    }

    //Event Handlers
    private void OnPrefightAnimFinish(Spine.TrackEntry trackEntry)
    {
        FinishAct_Prefight();
    }
    private void OnSpellCastAnimFinish(Spine.TrackEntry trackEntry)
    {
        if (!_isSpellHappened)
            SpellHappened();

        FinishAct_CastSpell();
    }
    private void OnSpellCastAnimEvent(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        SpellHappened();
    }
    private void OnSpellReceiveAnimFinish(Spine.TrackEntry trackEntry)
    {
        FinishAct_ReceiveSpell();
    }
    private void OnDieAnimFinish(Spine.TrackEntry trackEntry)
    {
        FinishAct_Dying();
    }    
    private void OnSpecificAnimFinish(Spine.TrackEntry trackEntry)
    {
        FinishAct_RunSpecificAnim();
    }
    private void OnSpineEventCapture(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        if (e.data.name == _event_CastSpell)
            OnSpellCastAnimEvent(trackEntry, e);
    }
}
