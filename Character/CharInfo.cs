using System.Collections.Generic;
using UnityEngine;
using Divine;

public enum TroopLeagueSituation
{
    Unlocked,
    Locked_CurLeague,
    Locked_FutureLeagues,
}

public class CharInfo
{
    //Statics
    private const string KEY__NUM_OF_UNSEEN_NEW_CHARS = "NumOfUnseenNewChars";
    private const string KEY__NEW_CHAR = "NewCh_";
    
    public delegate void Deleg_CooldownRemainingTimeChange(CharInfo charInfo, int remainingTime);
    public event Deleg_CooldownRemainingTimeChange Event_CooldownRemainingTimeChange;

    public delegate void Deleg_CurCardCountChanged(CharInfo charInfo, int newCardCount);
    public event Deleg_CurCardCountChanged Event_CurCardCountChanged;
    
    public delegate void Deleg_LevelChanged(CharInfo charInfo, int newLevel);
    public event Deleg_LevelChanged Event_LevelChanged;
    
    public delegate void Deleg_IntStatChanged(int oldVal, int newVal);
    public static Deleg_IntStatChanged Event_NumOfUnseenNewCharsChanged;
    
    public delegate void Deleg_CharIsNewStateChanged(Moniker moniker, bool isNew);
    public static Deleg_CharIsNewStateChanged Event_CharIsNewStateChanged;
    
    private static int _numOfUnseenNewChars = -1;
    
    public static int NumOfUnseenNewChars
    {
        get
        {
            TryInitNumOfUnseenNewChars();
            
            return _numOfUnseenNewChars;
        }
        set
        {
            TryInitNumOfUnseenNewChars();
            
            int old = _numOfUnseenNewChars;
            _numOfUnseenNewChars = value;

            if (_numOfUnseenNewChars < 0)
                _numOfUnseenNewChars = 0;

            SaveNumOfUnseenNewChars();

            if (Event_NumOfUnseenNewCharsChanged != null)
                Event_NumOfUnseenNewCharsChanged(old, _numOfUnseenNewChars);
        }
        
    }

    //Private
    private bool _timeSecEventAdded = false;
    private int _curCardCount;
    private int _level;
    private int _remainingCooldownTime;

    //Props            
    public bool enableInStart                  { get; private set; }
    public long uniqueID                       { get; private set; }
    public List<SpellInfo>  spells             { get; private set; }
    public Divine.Moniker   moniker            { get; private set; }
    public Divine.Moniker ownerHero            { get; private set; }
    public Dexterity turnPriority              { get; private set; }
    public WCharExistenceType existenceType    { get; set; }
    public WLeagueType unlockLeagueType        { get; set; }
    public int unlockLeagueStep                { get; set; }
    public int xp                              { get; set; }
    public NextUpgradeStats nextUpgradeStats   { get; set; }
    public BattleObjStats baseStats            { get; set; }
    public CharacterOtherStats otherStats      { get; set; }
    
    public int level
    {
        get { return _level; }
        set
        {
            if (_level != value)
            {
                _level = value;

                if (Event_LevelChanged != null)
                    Event_LevelChanged(this, _level);
            }
        } 
    }
    public int curCardCount
    {
        get { return _curCardCount; }
        set
        {
            if (_curCardCount != value)
            {
                _curCardCount = value;

                if (Event_CurCardCountChanged != null)
                    Event_CurCardCountChanged(this, _curCardCount);
            }
        } 
    }
    public int remainingCooldownTime
    {
        get { return _remainingCooldownTime; }
        private set
        {
            _remainingCooldownTime = value;

            if (_remainingCooldownTime < 0)
                _remainingCooldownTime = 0;
            
            if (Event_CooldownRemainingTimeChange != null)
                Event_CooldownRemainingTimeChange(this, remainingCooldownTime);
        }
    }
    
    //Static Methods
    public static bool IsCharUpgradable(CharInfo charInfo)
    {
        return (charInfo.level > 0
                && charInfo.nextUpgradeStats != null
                && charInfo.nextUpgradeStats.card_cost <= GameManager.instance.player.coin
                && charInfo.nextUpgradeStats.card_count <= charInfo.curCardCount
                && charInfo.nextUpgradeStats.card_cost > 0);
    }

    public static bool DoesAnyUnseenNewCharExists()
    {
        return NumOfUnseenNewChars > 0;
    }
    public static void TrySetCharAsNew(Moniker moniker)
    {
        string key = KEY__NEW_CHAR + moniker;
        
        if(DataAccessManager.HasKey(key))
            return;
        
        DataAccessManager.SaveStringToDisk(key, moniker.ToString());
        
        NumOfUnseenNewChars++;

        if (Event_CharIsNewStateChanged != null)
            Event_CharIsNewStateChanged(moniker, true);
    }
    public static void TryDeleteCharAsNew(Moniker moniker)
    {
        string key = KEY__NEW_CHAR + moniker;
        
        if(!DataAccessManager.HasKey(key))
            return;
        
        DataAccessManager.DeleteStringFromDisk(key);
        
        NumOfUnseenNewChars--;
        
        if (Event_CharIsNewStateChanged != null)
            Event_CharIsNewStateChanged(moniker, false);
    }
    public static bool IsCharNewAndUnseen(Moniker moniker)
    {
        if (!DoesAnyUnseenNewCharExists())
            return false;
        
        string key = KEY__NEW_CHAR + moniker;

        return DataAccessManager.HasKey(key);
    }
    
    private static void SaveNumOfUnseenNewChars()
    {
        DataAccessManager.SaveIntToDisk(KEY__NUM_OF_UNSEEN_NEW_CHARS, NumOfUnseenNewChars);
    }
    public static void TryInitNumOfUnseenNewChars()
    {
        if(_numOfUnseenNewChars < 0)
            _numOfUnseenNewChars =  DataAccessManager.LoadIntFromDisk(KEY__NUM_OF_UNSEEN_NEW_CHARS, 0);
    }
    
    //Base Methods
    public CharInfo(
        Divine.Moniker moniker, BattleObjStats initialStats, List<SpellInfo> spells,
        Dexterity priority, int xp, int level, long id,
        int coolDownRemainingTime, int cardCount, NextUpgradeStats nextUpgradeStats,
        Divine.Moniker owner = Divine.Moniker.Unknown, bool isActive = true)

    {
        this.xp = xp;
        this.level = level;

        uniqueID = id;
        this.spells = spells;
        this.moniker = moniker;
        turnPriority = priority;
        baseStats = initialStats;
        ownerHero = owner;

        curCardCount = cardCount;
        this.nextUpgradeStats = nextUpgradeStats;
        SetRemainingCooldownTime(coolDownRemainingTime);
        enableInStart = isActive;
    }

    public void SetBaseStats(int curHP, int maxHP, int damage, int curShield, int MaxShield)
    {
        baseStats = new BattleObjStats(curHP, maxHP, damage, curShield, MaxShield);
    }
    public void SetOtherStats(float critDamage, float critChance, float dodgeChance)
    {
        otherStats = new CharacterOtherStats(critDamage, critChance, dodgeChance);
    }
    public void SetUnlockData(WCharExistenceType existenceType, WLeagueType unlockLeagueType, int unlockLeagueStep)
    {
        this.existenceType = existenceType;
        this.unlockLeagueType = unlockLeagueType;
        this.unlockLeagueStep = unlockLeagueStep;
    }

    //Private Methods
    private bool TryDecreaseCooldown(int seconds)
    {
        remainingCooldownTime -= seconds;

        return remainingCooldownTime == 0;
    }
    
    //Public Methods
    public GameObject GetCharVisualPrefab()
    {
        return PrefabProvider_Battle.instance.GetPrefab_CharVisual(moniker.ToString());
    }
    public void SetRemainingCooldownTime(int seconds)
    {
        remainingCooldownTime = seconds;
 
        if (seconds == 0)
        {
            if (_timeSecEventAdded)
            {
                TimeManager.instance.Event_OneSecTick -= OnOneSecTick;
                _timeSecEventAdded = false;
            }
            
            return;
        }

        if (!_timeSecEventAdded)
        {
            TimeManager.instance.Event_OneSecTick += OnOneSecTick;
            _timeSecEventAdded = true;
        }
    }
    public bool IsUnlocked()
    {
        return level > 0;
    }

    //Event Handlers
    private void OnOneSecTick(float actualTimeInterval)
    {
        if (TryDecreaseCooldown((int)TimeManager.ONE_SECOND_TICK))
        {
            TimeManager.instance.Event_OneSecTick -= OnOneSecTick;
            _timeSecEventAdded = false;
        }
    }
}

public class TroopCoolDownData
{
    public int id { get; private set; }

    public int remainingSeconds { get; private set; }

    public TroopCoolDownData(int id, int remainingSeconds)
    {
        this.id = id;
        this.remainingSeconds = remainingSeconds;
    }
}