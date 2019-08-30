using System;
using System.Collections.Generic;
using UnityEngine;

public class BtlCtrlCharacter
{
    public struct ChangeStatInfo
    {
        public int hpChangeAmount { get; private set; }
        public int shieldChangeAmount { get; private set; }

        public void SetValues(int hpChange, int shieldChange)
        {
            hpChangeAmount = hpChange;
            shieldChangeAmount = shieldChange;
        }

        public void ChangeShield(int amount)
        {
            shieldChangeAmount += amount;
        }

        public void ChangeHP(int amount)
        {
            hpChangeAmount += amount;
        }
    }

    public enum Type
    {
        Hero,
        Troop,
        Chakra,
    }
    
    public delegate void Deleg_SpellFinished(BtlCtrlSpell spell);
    public delegate void Deleg_ActionGenerated(ActionData actionData);
    
    public event Deleg_SpellFinished Event_SpellFinished;
    public event Deleg_ActionGenerated Event_ActionGenerated;

    private int     _id;
    private int     _level;
    private Type    _type;
    private bool    _isActive;
    private BtlCtrlParty _relatedParty;
    private Divine.Moniker _moniker;
    private BattleObjStats _stats;
    private ChangeStatInfo _lastChangeStatInfo;
    private List<BtlCtrlSpell> _spells;

    public int id           { get { return _id; } set { _id = value; } }
    public int hp           { get { return _stats.hp; } }
    public int level        { get { return _level; } set { _level = value; } }
    public int maxHp        { get { return _stats.maxHp; } }
    public int shield       { get { return _stats.shield; } }
    public int maxShield    { get { return _stats.maxShield; } }
    public Type type        { get { return _type; } }    
    public bool isDead      { get { return _stats.hp == 0; } }
    public bool isActive    { get { return _isActive; } set { _isActive = value; } }
    public BtlCtrlParty relatedParty { get { return _relatedParty; } }
    public Divine.Moniker moniker    { get { return _moniker; } }
    public List<BtlCtrlSpell> spells { get { return _spells; } }
    
    //Base Methods
    public BtlCtrlCharacter(Divine.Moniker moniker, int maxHP, int MaxShield,int level, Type type)
    {
        Init(moniker, maxHP, MaxShield, maxHP, MaxShield,level, type);
    }
    public BtlCtrlCharacter(Divine.Moniker moniker, int hp, int shield, int maxHP, int MaxShield,int level, Type type)
    {
        Init(moniker, hp, shield, maxHP, MaxShield,level, type);
    }
    
    //Public Methods
    public void SetSpells(List<BtlCtrlSpell> spells)
    {
        for (int i = 0; i < spells.Count; i++)
        {
            spells[i].SetOwnerAndIndex(this, i);

            spells[i].Event_SpellFinished += OnSpellFinished;
            spells[i].Event_ActionGenerated += OnActionGenerated;
        }

        _spells = spells;
    }

    public ChangeStatInfo ApplyDamage(int amount)
    {
        DivineDebug.Log("Character " + moniker + " Hp-Shield: " + _stats.hp + "  " + _stats.shield, DivineLogType.Warn);

        if (amount > 0)
        {
            DivineDebug.Log("Positive amount for damage is wrong", DivineLogType.Error);

            _lastChangeStatInfo.SetValues(0, 0);

            return _lastChangeStatInfo;
        }

        int beforeDmgShield = 0;
        int amountDecreaseFromHP = 0;
        int amountDecreaseFromShield = 0;

        if (_stats.shield > 0)
        {
            beforeDmgShield = _stats.shield;
            _stats.SetShield(_stats.shield + amount); // amount is negative here

            amountDecreaseFromShield = _stats.shield > 0 ? amount : beforeDmgShield;

            amount += beforeDmgShield;
            amount = Mathf.Min(amount, 0);
        }

        if (amount < 0) //still has damage to apply
        {
            int beforeDmghp = _stats.hp;
            _stats.SetHP(_stats.hp + amount);

            amountDecreaseFromHP = _stats.hp > 0 ? amount : beforeDmghp;
        }

        _lastChangeStatInfo.SetValues(amountDecreaseFromHP, amountDecreaseFromShield);

        if (_isActive)
            relatedParty.ChangeAP(1);

        if (isDead && type == Type.Troop && _isActive)
            relatedParty.ChangeAP(2);

        DivineDebug.Log("Character " + moniker + " Hp-Shield: " + _stats.hp + "  " + _stats.shield, DivineLogType.Warn);

        if (beforeDmgShield > 0 && shield <= 0 && type == Type.Hero)
            CastSpell(2, new List<BtlCtrlCharacter> { relatedParty.chakra }, null, false);

        return _lastChangeStatInfo;
    }

    public ChangeStatInfo Heal(int healAmount)
    {
        if(healAmount<0)
        {
            DivineDebug.Log("Negative amount for heal is wrong", DivineLogType.Error);

            _lastChangeStatInfo.SetValues(0, 0);

            return _lastChangeStatInfo;
        }

        _lastChangeStatInfo.SetValues(healAmount, 0);

        return _lastChangeStatInfo;        
    }

    public void SetEnable(bool isEnable)
    {
        _isActive = isEnable;
    }

    public List<int> GetEligibleSpells()
    {
        List<int> elgSps = new List<int>();

        for (int i = 0; i < spells.Count; i++)
        {
            if (spells[i].actionPointAmount <= _relatedParty.ap)
                elgSps.Add(i);
        }

        return elgSps;
    }
    public void SetRelatedParty(BtlCtrlParty btlCtrlParty)
    {
        _relatedParty = btlCtrlParty;
    }

    public void DestoryCharacter()
    {
        for (int i = 0; i < spells.Count; i++)
        {
            spells[i].Event_SpellFinished -= OnSpellFinished;
            spells[i].Event_ActionGenerated -= OnActionGenerated;
        }

        _relatedParty = null;
        _spells.Clear();
        _spells = null;
        _stats = null;
    }

    //Private Methods
    private void Init(Divine.Moniker moniker, int hp, int shield, int maxHP, int maxShield, int level, Type type)
    {
        _moniker = moniker;

        _level = level;

        _type = type;

        _stats = new BattleObjStats(hp, maxHP, 0, shield, maxShield);

        _lastChangeStatInfo = new ChangeStatInfo();
    }

    internal void CastSpell(int selectedSpellIndex, List<BtlCtrlCharacter> targets, Action<BtlCtrlSpell> onCastFinished,bool shouldChangeAP = true)
    {
        if (shouldChangeAP)
            _relatedParty.ChangeAP(-spells[selectedSpellIndex].actionPointAmount);

        _spells[selectedSpellIndex].StartSpell(targets, onCastFinished);
    }

    //Handlers
    private void OnSpellFinished(BtlCtrlSpell spell)
    {
        if (Event_SpellFinished != null)
            Event_SpellFinished(spell);
    }

    private void OnActionGenerated(ActionData actionData)
    {
        if (Event_ActionGenerated != null)
            Event_ActionGenerated(actionData);
    }
}