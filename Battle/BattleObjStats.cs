using System;

public class BattleObjStats : ICloneable
{
    //Statics
    public static bool operator ==(BattleObjStats a, BattleObjStats b)
    {
        if (Object.Equals(a, null) && Object.Equals(b, null))
            return true;
        
        if (Object.Equals(a, null) && !Object.Equals(b, null))
            return false;

        if (!Object.Equals(a, null) && Object.Equals(b, null))
            return false;

        return a.Equals(b);
    }
    public static bool operator !=(BattleObjStats a, BattleObjStats b)
    {
        return !(a == b);
    }
    public static BattleObjStats operator +(BattleObjStats a, BattleObjStats b)
    {
        BattleObjStats c = new BattleObjStats(0, a.maxHp + b.maxHp, a.damage + b.damage, 0, a.maxShield + b.maxShield);
        c.SetHP(a.hp + b.hp);
        c.SetShield(a.shield + b.shield);
        return c;
    }
    public static BattleObjStats operator -(BattleObjStats a, BattleObjStats b)
    {
        BattleObjStats c = new BattleObjStats(0, a.maxHp - b.maxHp, a.damage - b.damage, 0, a.maxShield - b.maxShield);
        c.SetHP(a.hp - b.hp);
        c.SetShield(a.shield - b.shield);
        return c;
    }

    //Props
    public BattleFlags flags { get; set; }

    public int hp { get; private set; }
    
    public int maxHp { get; set; }

    public int damage { get; set; }

    public int shield { get; private set; }

    public int maxShield { get; set; }

    //Base Methods
    public BattleObjStats(int hp, int maxHp, int damage, int shield, int maxShield)
    {
        this.hp         = hp;
        this.maxHp      = maxHp;
        this.damage     = damage;
        this.shield     = shield;
        this.maxShield  = maxShield;
    }

    public BattleObjStats(int hp, int maxHp, int damage, int shield, int maxShield, BattleFlags flags)
    {
        this.flags = flags;
        this.hp = hp;
        this.maxHp = maxHp;
        this.damage = damage;
        this.shield = shield;
        this.maxShield = maxShield;
    }
    
    public void SetHP(int value)
    {
        hp = UnityEngine.Mathf.Clamp(value, 0, maxHp);
    }

    public void SetShield(int value)
    {
        shield = UnityEngine.Mathf.Clamp(value, 0, maxShield);
    }

    public object Clone()
    {
        return new BattleObjStats(hp, maxHp, damage, shield, maxShield, flags);
    }

    public void CopyStatsTo(BattleObjStats statsObj)
    {
        statsObj.maxHp          =   this.maxHp          ;
        statsObj.SetHP          (   this.hp         )   ;
        statsObj.damage         =   this.damage         ;
        statsObj.SetShield      (   this.shield     )   ;
        statsObj.maxShield      =   this.maxShield      ;
    }
    
    public override bool Equals(object obj)
    {
        BattleObjStats statsObj = obj as BattleObjStats;

        if (statsObj == null)
            return false;

        return this.hp == statsObj.hp
            && this.maxHp == statsObj.maxHp
            && this.damage == statsObj.damage
            && this.shield == statsObj.shield
            && this.maxShield == statsObj.maxShield;
    }
}
