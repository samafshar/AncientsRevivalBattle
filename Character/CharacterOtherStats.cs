using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextUpgradeStats
{
    public int attack;
    public int health;
    public int shield;
    public int card_cost;
    public int card_count;    
    public float miss_chance;
    public float dodge_chance;
    public float critical_ratio;
    public float critical_chance;
    
    public NextUpgradeStats(){}

    public NextUpgradeStats(int attack, int health, int shield, int cardCost, int cardCount, float missChance, float dodgeChance, float criticalRatio, float criticalChance)
    {
        this.attack = attack;
        this.health = health;
        this.shield = shield;
        this.card_cost = cardCost;
        this.card_count = cardCount;
        this.miss_chance = missChance;
        this.dodge_chance = dodgeChance;
        this.critical_ratio = criticalRatio;
        this.critical_chance = criticalChance;
    }
}

public class CharacterOtherStats
{
    //Statics
    public static bool operator ==(CharacterOtherStats a, CharacterOtherStats b)
    {
        return a.Equals(b);
    }
    public static bool operator !=(CharacterOtherStats a, CharacterOtherStats b)
    {
        return !a.Equals(b);
    }
    public static CharacterOtherStats operator +(CharacterOtherStats a, CharacterOtherStats b)
    {
        CharacterOtherStats c = new CharacterOtherStats(a.critDamage + b.critDamage, a.critChance + b.critChance, a.dodgeChance + b.dodgeChance);
        return c;
    }
    public static CharacterOtherStats operator -(CharacterOtherStats a, CharacterOtherStats b)
    {
        CharacterOtherStats c = new CharacterOtherStats(a.critDamage - b.critDamage, a.critChance - b.critChance, a.dodgeChance - b.dodgeChance);
        return c;
    }

    public float critDamage { get; set; }
    public float critChance { get; set; }
    public float dodgeChance { get; set; }

    public CharacterOtherStats (float critDamage, float critChance, float dodgeChance)
    {
        this.critDamage = critDamage;
        this.critChance = critChance;
        this.dodgeChance = dodgeChance;
    }

    public override bool Equals(object obj)
    {
        CharacterOtherStats statsObj = obj as CharacterOtherStats;

        if (statsObj == null)
            return false;

        return this.critChance == statsObj.critChance
            && this.critDamage == statsObj.critDamage
            && this.dodgeChance == statsObj.dodgeChance;
    }

    public void CopyStatsTo(CharacterOtherStats statsObj)
    {
        statsObj.critDamage = this.critDamage;
        statsObj.critChance = this.critChance;
        statsObj.dodgeChance = this.dodgeChance;
    }
}
