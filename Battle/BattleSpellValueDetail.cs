using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


public enum BattleSpellColorType
{
    Red,
    Green,
    Yellow,
}

public enum BattleSpellIconType
{
    Attack,
    DamageSplash,
    Heal,
    HealSplash,
    Damage_inc,
    Damage_dec,
    Health_dec,
    ActionPoint,
    Taunt,
    Chakra,
    Protect,
}

public enum BattleSpellSign
{
    None,
    Percent,
    Plus,
    Comma,
    Dash,
}

public enum BattleSpellValueType
{
    Damage,
    Heal_SelfPercent,
    DecreaseHp_SelfPercent,
    IncreaseDmg_SelfPercent,
    DecreaseHp,
    IncreaseDmg,
    MultiDmg,
    Taunt,
    ActionPointGenerate,
    LostHpAddedToDmg,
    NotFixDmg,
    DamageToTwoUnit,
    ActionPointToDamage,
    HealAccordingToDamage,
    LifeSteal,
    TutorialDamage,
    Protect,
}

public class BattleSpellValueDetail : MonoBehaviour
{
    [SerializeField]
    private BattleSpellSign _signType;

    [SerializeField]
    private BattleSpellIconType _iconType;

    [SerializeField]
    private BattleSpellColorType _colorType;
    
    [SerializeField]
    private BattleSpellValueType _valueType;

    private List<int> _values = new List<int>();

    public List<int> values                 { get { return _values; } }
    public BattleSpellIconType iconType     { get { return _iconType; } }
    public BattleSpellColorType colorType   { get { return _colorType; } }
    public BattleSpellValueType valueType   { get { return _valueType; } }
    public BattleSpellSign signType
    {
        get
        {
            if (TutorialManager.instance.IsInBattleTutorial())
                return BattleSpellSign.None;
            else
                return _signType;
        }
    }


    public void Reset(CharInfo charInfo, int charSpellIndex, int currentPartyAP)
    {
        _values.Clear();

        int index = charSpellIndex;
        SpellInfo currentSpellInf = charInfo.spells[index];

        if(TutorialManager.instance.IsInBattleTutorial())
        {
            var par0 = JsonConvert.DeserializeObject<WSpellDataTutorialParams>(currentSpellInf.spellParams);
            _values.Add(par0.damage);

            return;
        }

        switch (valueType)
        {
            case BattleSpellValueType.Damage:
                _values.Add(charInfo.baseStats.damage);
                break;

            case BattleSpellValueType.TutorialDamage:
                var par0 = JsonConvert.DeserializeObject<WSpellDataTutorialParams>(currentSpellInf.spellParams);
                _values.Add(par0.damage);
                break;

            case BattleSpellValueType.Heal_SelfPercent:                
                var par1 = JsonConvert.DeserializeObject<WSpellDataHealerParams>(currentSpellInf.spellParams);
                _values.Add((int)(charInfo.baseStats.maxHp * par1.healer_percent));
                break;

            case BattleSpellValueType.DecreaseHp_SelfPercent:
                var par2 = JsonConvert.DeserializeObject<WSpellDataDecreaseHPIncDamage>(currentSpellInf.spellParams);
                _values.Add((int)(charInfo.baseStats.maxHp * par2.decrease_health));
                break;

            case BattleSpellValueType.IncreaseDmg_SelfPercent:
                var par3 = JsonConvert.DeserializeObject<WSpellDataDecreaseHPIncDamage>(currentSpellInf.spellParams);
                _values.Add((int)(charInfo.baseStats.damage * par3.increase_dmg));
                break;

            case BattleSpellValueType.DecreaseHp:
                var par4 = JsonConvert.DeserializeObject<WSpellDataDecreaseHPIncDamage>(currentSpellInf.spellParams);
                _values.Add((int)(par4.decrease_health * 100));
                break;

            case BattleSpellValueType.IncreaseDmg:
                var par5 = JsonConvert.DeserializeObject<WSpellDataDecreaseHPIncDamage>(currentSpellInf.spellParams);
                _values.Add((int)(par5.increase_dmg * 100));
                break;
                
            case BattleSpellValueType.MultiDmg:
                var par6 = JsonConvert.DeserializeObject<WSpellDataMultiDmg>(currentSpellInf.spellParams);
                for (int i = 0; i < par6.multi_damage.Length; i++)
                    _values.Add((int)(charInfo.baseStats.damage * par6.multi_damage[i]));
                break;

            case BattleSpellValueType.Taunt:
                var par7 = JsonConvert.DeserializeObject<WSpellDataTaunt>(currentSpellInf.spellParams);
                _values.Add(par7.taunt_Duration);
                break;

            case BattleSpellValueType.ActionPointGenerate:                
                _values.Add(currentSpellInf.generatedActionPoint);
                break;

            case BattleSpellValueType.LostHpAddedToDmg:
                var par8 = JsonConvert.DeserializeObject<WSpellDataLostHpToDmg>(currentSpellInf.spellParams);
                int diffHp = charInfo.baseStats.maxHp - charInfo.baseStats.hp;
                _values.Add(charInfo.baseStats.damage + (int)(par8.inc_dmg * diffHp));
                break;

            case BattleSpellValueType.NotFixDmg:
                var par9 = JsonConvert.DeserializeObject<WSpellDataNotFixDamage>(currentSpellInf.spellParams);
                _values.Add((int)(charInfo.baseStats.damage * par9.damage_multiplier.min));
                _values.Add((int)(charInfo.baseStats.damage * par9.damage_multiplier.max));
                break;

            case BattleSpellValueType.DamageToTwoUnit:
                var par10 = JsonConvert.DeserializeObject<WSpellDataDamageToTwoUnit>(currentSpellInf.spellParams);
                _values.Add(charInfo.baseStats.damage);
                _values.Add((int)(charInfo.baseStats.damage * par10.second_attack_power));
                break;

            case BattleSpellValueType.ActionPointToDamage:
                var par11 = JsonConvert.DeserializeObject<WSpellDataAPToDamage>(currentSpellInf.spellParams);
                _values.Add(charInfo.baseStats.damage + (currentPartyAP * par11.action_point_dmg));
                break;

            case BattleSpellValueType.HealAccordingToDamage:
                var par12 = JsonConvert.DeserializeObject<WSpellDataHealAccordToDmg>(currentSpellInf.spellParams);
                _values.Add((int)(charInfo.baseStats.damage * par12.heal_percent));
                break;

            case BattleSpellValueType.LifeSteal:
                var par13 = JsonConvert.DeserializeObject<WSpellDataLifeSteal>(currentSpellInf.spellParams);
                _values.Add((int)(charInfo.baseStats.damage * par13.life_steal_percent / 100));
                break;

            case BattleSpellValueType.Protect:
                var par14 = JsonConvert.DeserializeObject<WSpellDataProtect>(currentSpellInf.spellParams);
                _values.Add(par14.protect_duration);
                break;

            default:
                DivineDebug.Log("No Battle Spell Value Type found for it: " + valueType.ToString());
                break;
        }
    }    
}

public class WSpellDataTutorialParams
{
    public int damage;
}

public class WSpellDataHealerParams
{
    public bool is_critical;
    public float healer_percent;
}

public class WSpellDataDecreaseHPIncDamage
{
    public bool is_critical;
    public float increase_dmg;
    public float decrease_health;
}

public class WSpellDataMultiDmg
{
    public bool is_critical;
    public float[] multi_damage;
}

public class WSpellDataTaunt
{
    public int taunt_Duration;
    public bool is_critical;
}

public class WSpellDataLostHpToDmg
{
    public bool is_critical;
    public float inc_dmg;
}

public class WSpellDataNotFixDamage
{
    public bool is_critical;
    public WDamageMultiplierRange damage_multiplier;
}

public class WSpellDataDamageToTwoUnit
{
    public bool is_critical;
    public float second_attack_power;
}

public class WSpellDataAPToDamage
{
    public int action_point_dmg;
    public bool is_critical;
}

public class WSpellDataHealAccordToDmg
{
    public float heal_percent;
}

public class WSpellDataLifeSteal
{
    public bool is_critical;
    public float life_steal_chance;
    public float life_steal_percent;
}

public class WDamageMultiplierRange
{
    public float min;
    public float max;
}

public class WSpellDataProtect
{
    public int protect_duration;
    public bool is_critical;
}