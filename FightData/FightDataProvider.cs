using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public delegate void BattleStartedEventHandler(BattleInfo bi);

public delegate void BattleTurnChangedEventHandler(TurnData turnData);

public delegate void BattleClockTickedEventHandler(int elapsed);

public delegate void BattleStatesChangedEventHandler(List<Divine.BattleState> states);

public delegate void SpellInfoReceivedEventHandler(List<ActionData> actions);

public delegate void SecretInfoReceivedEventHandler(int enabledSecret);

public delegate void BattleFinishedEventHandler(bool victory, RewardData reward, List<TroopCoolDownData> troopCoolDowns);

public class FightDataProvider : MonoBehaviour
{
	public event BattleStartedEventHandler          Started;
	public event BattleFinishedEventHandler         Finished;
	public event BattleClockTickedEventHandler      Ticked;
	public event BattleTurnChangedEventHandler      TurnChanged;
	public event SpellInfoReceivedEventHandler      SpellInfoReceived;
    public event SecretInfoReceivedEventHandler     SecretInfoReceived;
    public event BattleStatesChangedEventHandler    StateChanged;

	private FightDataAnalyzer analyzer_;
    	
    //Base Methods
	void Awake ()
	{
		analyzer_ = new FightDataAnalyzer();

		NetworkManager.instance.BattleStarted           += OnBattleStarted;
		NetworkManager.instance.BattleTurnChanged       += OnBattleTurnChanged;
		NetworkManager.instance.BattleClockTicked       += OnBattleClockTicked;
        NetworkManager.instance.BattleStatesChanged     += OnBattleStatesChanged;
		NetworkManager.instance.BattleActionReceived    += OnBattleActionReceived;
        NetworkManager.instance.BattleSecretReceived    += OnBattleSecretReceived;
	}

    private void OnDestroy()
    {
        if (NetworkManager.instance == null) return;

        NetworkManager.instance.BattleStarted           -= OnBattleStarted;
        NetworkManager.instance.BattleTurnChanged       -= OnBattleTurnChanged;
        NetworkManager.instance.BattleClockTicked       -= OnBattleClockTicked;
        NetworkManager.instance.BattleStatesChanged     -= OnBattleStatesChanged;
        NetworkManager.instance.BattleActionReceived    -= OnBattleActionReceived;
        NetworkManager.instance.BattleSecretReceived    -= OnBattleSecretReceived;
    }

    //Event Handlers
	private void OnBattleStarted(Divine.BattleData battle)
	{
		analyzer_.Player    = new OpponentInfo(battle.Party[0]);
		analyzer_.Opponent  = new OpponentInfo(battle.Party[1]);
		analyzer_.TurnTime  = battle.TurnTime;
        
        Started(analyzer_.MakeBattleInfo());
	}

	private void OnBattleTurnChanged(Divine.BattleTurnData turn)
	{
        var updateStats = analyzer_.ChangeTurn(turn.StatUpdate);
        var turnData = new TurnData(
            turn.Turn, turn.EligibleSpell.ToList(), turn.EligibleSecret.ToList(),
            turn.CoolDownSpell.Select(ToCoolDownData).ToList(),
            turn.CoolDownSecret.Select(ToCoolDownData).ToList(),
            turn.GeneratedActionPoint[0], turn.GeneratedActionPoint[1],
            updateStats);
        if (TurnChanged != null)
            TurnChanged(turnData);
	}

    private CoolDownData ToCoolDownData(Divine.CoolDownSpellData coolDownSpellData)
    {
        return new CoolDownData(coolDownSpellData.Spell, coolDownSpellData.RemainingTurns);
    }

	private void OnBattleClockTicked(Divine.BattleTickData tick)
	{
        if (Ticked != null)
            Ticked(tick.Elapsed);
	}

    private void OnBattleStatesChanged(Divine.BattleStateData state)
    {
        if (StateChanged != null)
            StateChanged(state.State.ToList<Divine.BattleState>());
    }

    private void OnBattleActionReceived(Divine.BattleActionData action)
	{
        SpellInfoReceived(analyzer_.MakeFightActionDataList(action));
	}

    private void OnBattleSecretReceived(Divine.BattleSecretData secret)
    {
        SecretInfoReceived(secret.EnabledSecret);
    }
    

    //Instance
    private static FightDataProvider _instance;
    public static FightDataProvider instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<FightDataProvider>();

            return _instance;
        }
    }
}

public class TurnData
{
    public long turnId { get; private set; }
    public List<int> eligibleSpells { get; private set; }
    public List<int> eligibleSecrets { get; private set; }
    public List<CoolDownData> coolDownSpells { get; private set; }
    public List<CoolDownData> coolDownSecrets { get; private set; }
    public int allyReceivedActionPoints { get; private set; }
    public int enemyReceivedActionPoints { get; private set; }
    public List<StatsUpdateData> updatedStats { get; private set; }

    public TurnData(long turnId, List<int> eligibleSpells, List<int> eligibleSecrets,
        List<CoolDownData> coolDownSpells, List<CoolDownData> coolDownSecrets,
        int allyReceivedActionPoints, int enemyReceivedActionPoints, List<StatsUpdateData> updatedStats)
    {
        this.turnId = turnId;
        this.eligibleSpells = eligibleSpells;
        this.eligibleSecrets = eligibleSecrets;
        this.coolDownSpells = coolDownSpells;
        this.coolDownSecrets = coolDownSecrets;
        this.allyReceivedActionPoints = allyReceivedActionPoints;
        this.enemyReceivedActionPoints = enemyReceivedActionPoints;
        this.updatedStats = updatedStats;
    }
}

public class CoolDownData
{
    public int index { get; private set; }

    public int remainingTurns { get; private set; }

    public CoolDownData(int index, int remainingTurns)
    {
        this.index = index;
        this.remainingTurns = remainingTurns;
    }
}

public class StatsUpdateData
{
	public long ownerID;

	public List<SpellSingleStatChangeInfo> singleStatChanges;

	public BattleObjStats finalStats;
}
