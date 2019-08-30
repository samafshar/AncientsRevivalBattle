using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpellSingleStatChangeType
{
    None,
    curHPValChange,
    curShieldValChange,
    curDamageValChange,
    curFlagValChange,
}

public class SpellSingleStatChangeInfo
{
    //Private
    private SpellSingleStatChangeType _charStatChangeType;
    private int _intVal;

    //Props
    public SpellSingleStatChangeType charStatChangeType { get { return _charStatChangeType; } }
    public int intVal { get { return _intVal; } }

    //Base Methods
    public SpellSingleStatChangeInfo(SpellSingleStatChangeType type, int intValue)
    {
        _charStatChangeType = type;
        _intVal = intValue;
    }

    //Public Methods
    public bool GetBoolVal()
    {
        return intVal != 0;
    }
}

public enum SpellEffectOnChar
{
    None,
    Appear,
    NormalDamage,
    SeriousDamage,
    Nerf,
    Buff,
    Miss,
    Dodge,
    Burn,
    Fear,
    Taunt,
    Revive,
    Prepare,
    Protect,
}

public class SpellEffectInfo
{
    //Private
    private bool _isMultiPart = false;
    private List<SpellSingleStatChangeInfo> _singleStatChanges;
    private long _targetCharacterID;
    private Character _targetCharacter;
    private SpellEffectOnChar _effectOnCharacter;
    private BattleObjStats _finalCharacterStats;

    //Props
    public bool isMultiPart
    {
        get
        {
            return _isMultiPart;
        }
        set
        {
            _isMultiPart = value;
        }
    }
    public List<SpellSingleStatChangeInfo> singleStatChanges
    {
        get
        {
            return _singleStatChanges;
        }
    }
    public Character targetCharacter
    {
        get
        {
            return _targetCharacter;
        }
    }
    public SpellEffectOnChar effectOnCharacter
    {
        get
        {
            return _effectOnCharacter;
        }
    }
    public BattleObjStats finalCharacterStats
    {
        get
        {
            return _finalCharacterStats;
        }
    }    
    public long targetCharacterID { get { return _targetCharacterID; } }

    //Base Methods
    public SpellEffectInfo(
        List<SpellSingleStatChangeInfo> statChanges, long targetCharacterID,
        SpellEffectOnChar effectOnChar, BattleObjStats finalCharStats)
    {
        _singleStatChanges = statChanges;
        _targetCharacterID = targetCharacterID;
        _effectOnCharacter = effectOnChar;
        _finalCharacterStats = finalCharStats;
    }

    //public Methods
    public void SetTarget(Character target)
    {
        _targetCharacter = target;
    }
}

public enum SpellType
{
    Magic,
    Secret,
    Chakra,
    DamageReturn,
}

public enum SpellImpactType
{
    None,
    Low,
    High
}

public enum SpellTargetType
{
    Ally,
    Enemy,
    All,
    Self,
    Hero
}

public enum DamageType
{
    Low,
    High,
}

public class SpellInfo
{
    //Statics
    private const string NAME_NOTVISUAL_SPELL = "NotVisual";

    //Private
    private int                     _charSpellsIndex = 0;
    private int                     _cost;
    private bool                    _dontShowToUser;
    private bool                    _isInstant;
    private bool                    _needTargetToComeNear = true;
    private string                  _spellName = "";
    private string                  _prefabName = "";
    private string                  _spellParams;
    private SpellType               _spellType = SpellType.Magic;
    private DamageType              _damageType = DamageType.Low;
    private SpellImpactType         _spellImpact;
    private List<SpellEffectInfo>   _spellEffectInfos;
    
    //Props
    public DamageType damageType { get { return _damageType; } set { _damageType = value; } }
    public SpellType spellType { get { return _spellType; } set { _spellType = value; } }
    public SpellImpactType spellImpact { get { return _spellImpact; } set { _spellImpact = value; } }
    public List<SpellEffectInfo> spellEffectInfos
    {
        get
        {
            return _spellEffectInfos;
        }

        set
        {
            _spellEffectInfos = value;
        }
    }
    public int charSpellsIndex
    {
        get
        {
            return _charSpellsIndex;
        }

        set
        {
            _charSpellsIndex = value;
        }
    }
    public bool isInstant
    {
        get
        {
            return _isInstant;
        }
    }    
    public int generatedActionPoint { get; internal set; }
    public bool needTargetToComeNear { get { return _needTargetToComeNear; } }
    public string spellParams { get { return _spellParams; } }
    public string spellName { get { return _spellName; } }
    public Character owner { get; set; }
    public int cost { get { return _cost; } }
    public bool dontShowToUser { get { return _dontShowToUser; } }
    public bool isCritical { get; internal set; }
    
    //Base Methods
    public SpellInfo(
        int index, bool isInstant, bool dontShowToUser, bool needTargetToComeNear, string name, int cost,
        string prefab, SpellType type,DamageType dmgType, SpellImpactType spellImpact, List<SpellEffectInfo> effectInfo)
    {
        _charSpellsIndex = index;
        _isInstant = isInstant;
        _needTargetToComeNear = needTargetToComeNear;
        _spellName = name;
        _prefabName = prefab;
        _damageType = dmgType;
        _spellType = type;
        _spellImpact = spellImpact;
        _spellEffectInfos = effectInfo;
        _cost = cost;
        _dontShowToUser = dontShowToUser;
    }

    //Public Methods
    public GameObject GetSpellPrefab()
    {
        return PrefabProvider_Battle.instance.GetPrefab_Spell(GetPrefabName());
    }

    public void SetSpellParams(string spParams)
    {
        _spellParams = spParams;
    }

    //Private Methods
    private string GetPrefabName()
    {
        if (!isInstant)
            return _prefabName;
        else
            return NAME_NOTVISUAL_SPELL;
    }

}
