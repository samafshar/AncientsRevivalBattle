using UnityEngine;

public class BattleInfo
{
    //Private
    public int              turnTime                { get; private set; }
    public long[]           charactersTurnOrder_id  { get; private set; }
    public PartyInfo[]      partyInfoes             { get; private set; }
    public BattleSceneType  battleSceneType         { get; private set; }

    public bool isBot { get; private set; }
    public float botPower { get; private set; }

    //Public Methods
    public void Init(int turnTime, long[] charOrder, PartyInfo[] partyInf,
                        BattleSceneType battleSceneType , bool bot = false,
                        float bot_ai = 1f)
    {
        DivineDebug.Log("Initing battle info.");

        isBot                   = bot;
        botPower                = bot_ai;
        partyInfoes             = partyInf;
        this.turnTime           = turnTime;
        this.battleSceneType    = battleSceneType;
        charactersTurnOrder_id  = charOrder;
    }
}
