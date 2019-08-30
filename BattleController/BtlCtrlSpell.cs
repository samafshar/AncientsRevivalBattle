using System;
using UnityEngine;
using System.Collections.Generic;

public class BtlCtrlSpell
{
    protected Deleg_SpellFinished _eventSpellFinished;

    public delegate void Deleg_ActionGenerated(ActionData actionData);
    public delegate void Deleg_SpellFinished(BtlCtrlSpell spell);

    public event Deleg_SpellFinished Event_SpellFinished
    {
        add { _eventSpellFinished += value; }
        remove { _eventSpellFinished -= value; }
    }
    public event Deleg_ActionGenerated Event_ActionGenerated;

    private int _index;
    private int _damage;
    private bool _isMultipleTarget;
    private string _spellParams;
    private BtlCtrlCharacter _owner;
    private SpellEffectOnChar _effect;
    private List<SpellEffectInfo> _spellEffectInfoes;

    protected int _actionPointAmount;
    protected bool _isSecret = false;
    protected Divine.Secret _secretType;
    protected Action<BtlCtrlSpell> _onSpellCastfinished;
    protected List<BtlCtrlSpellSubset> _spellSubsets;

    public int damage { get { return _damage; } }     
    public int actionPointAmount { get { return _actionPointAmount; } }
    public bool isMultipleTarget { get { return _isMultipleTarget; } }
    public string spellParams { get { return _spellParams; } }
    public BtlCtrlCharacter owner { get { return _owner; } }
    public SpellEffectOnChar effect { get { return _effect; } }


    //Public Methods
    public void SetOwnerAndIndex(BtlCtrlCharacter owner, int index)
    {
        _owner = owner;
        _index = index;
    }

    public void MakeAndAddSpellEffect(BtlCtrlCharacter target, SpellEffectOnChar effectOnChar,
                                               BtlCtrlCharacter.ChangeStatInfo changeStatInfo)
    {
        var statChanges = new List<SpellSingleStatChangeInfo>();

        if (Mathf.Abs(changeStatInfo.shieldChangeAmount) > 0)
            statChanges.Add(new SpellSingleStatChangeInfo(
                                SpellSingleStatChangeType.curShieldValChange,
                                changeStatInfo.shieldChangeAmount));

        if (Mathf.Abs(changeStatInfo.hpChangeAmount) > 0)
            statChanges.Add(new SpellSingleStatChangeInfo(
                                SpellSingleStatChangeType.curHPValChange,
                                changeStatInfo.hpChangeAmount));

        var stat = new BattleObjStats(target.hp,
                                      target.maxHp,
                                      damage,
                                      target.shield,
                                      target.maxShield);

        var effectInfo = new SpellEffectInfo(
                                statChanges, target.id,
                                effectOnChar,
                                stat);

        _spellEffectInfoes.Add(effectInfo);
    }

    public void MakeAction()
    {
        FightActionData fightActiondata = new FightActionData();

        AppearanceConfigData.MagicAppearanceData appearance;

        if (!_isSecret)
            appearance = AppearanceConfigData.instance.GetMagicWithIndex(owner.moniker, _index);
        else
            appearance = AppearanceConfigData.instance.GetSecretWithIndex(_secretType);

        SpellInfo spellInf = MakeSpellInfo(appearance, _spellEffectInfoes, _index);

        fightActiondata.ownerID     = owner.id;
        fightActiondata.spellInfo   = spellInf;
        fightActiondata.consumedActionPoint = _actionPointAmount;

        if (Event_ActionGenerated != null)
            Event_ActionGenerated(fightActiondata);
    }

    //Virtual Methods
    public virtual void Init(SpellEffectOnChar effect, int damage, int actionPoint, string spellParams,
                                                        bool isMultipleTarget = false)
    {
        _effect = effect;
        _damage = damage;
        _isMultipleTarget = isMultipleTarget;
        _actionPointAmount = actionPoint;

        _spellParams = spellParams;

        _spellSubsets = new List<BtlCtrlSpellSubset>();
        _spellEffectInfoes = new List<SpellEffectInfo>();
    }

    public virtual void StartSpell(List<BtlCtrlCharacter> targets, Action<BtlCtrlSpell> onSpellCastFinished)
    {
        _onSpellCastfinished = onSpellCastFinished;

        _spellEffectInfoes.Clear();

        if (targets == null || targets.Count == 0)
            targets = FindTarget();

        EditTargetIfNeeded(targets);
    }

    public virtual List<BtlCtrlCharacter> FindTarget()
    {
        List<BtlCtrlCharacter> targets = new List<BtlCtrlCharacter>();

        return targets;
    }

    public virtual void EditTargetIfNeeded(List<BtlCtrlCharacter> targets)
    {
        if (isMultipleTarget && targets.Count == 1)
        {
            List<BtlCtrlCharacter> chars = targets[0].relatedParty.GetCharacters();

            for (int i = 0; i < chars.Count; i++)
            {
                if (chars[i].isActive && !chars[i].isDead && !targets.Contains(chars[i]))
                    targets.Add(chars[i]);
            }
        }
    } 

    //Private Methods
    private SpellInfo MakeSpellInfo(AppearanceConfigData.MagicAppearanceData appearance,
                                        List<SpellEffectInfo> effectsInfo, int selectedSpell = 0)
    {
        return new SpellInfo(selectedSpell,
                appearance._isInstant, appearance._dontShowToUser, appearance._needTargetToComeNear,
                appearance._spellName, appearance._cost, appearance._prefabName,
                appearance._spellType, appearance._damageType, appearance._spellImpact, effectsInfo);
    }

    
}

public class BtlCtrlSpellSubset
{
    public enum TimeType
    {
        RunOnce,
        RunPeriodical,
    }

    public virtual void StartIt(BtlCtrlSpell main, List<BtlCtrlCharacter> targets){}
}
