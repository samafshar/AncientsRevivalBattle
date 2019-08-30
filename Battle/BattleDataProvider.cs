using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleDataProvider : MonoBehaviour
{
    public delegate void Deleg_BattleTurnTick(int elapsedTime);
    public delegate void Deleg_BattlePingTick();
    public delegate void Deleg_BattleStart(BattleInfo battleInfo);
    public delegate void Deleg_BattleTurnChange(TurnData turnData);

    public delegate void Deleg_BattleFinish(BattleResultData battleResultData, List<TroopCoolDownData> troopCoolDowns,bool connectionLost);
    public delegate void Deleg_ActionReceived(List<ActionData> actions);
    public delegate void Deleg_BattleStatChanged(List<Divine.BattleState> stats);

    public event Deleg_BattleStart Event_BattleStart;
    public event Deleg_BattleFinish Event_BattleFinish;
    public event Deleg_BattleTurnTick Event_BattleTurnTick;
    public event Deleg_BattlePingTick Event_BattlePingTick;
    public event Deleg_ActionReceived Event_ActionReceived;
    public event Deleg_BattleTurnChange Event_BattleTurnData;
    public event Deleg_BattleStatChanged Event_StateChanged;

    private BattleLogic _battleLogic;

    //Public Methods
    public void SetBattleLogic(BattleLogic battleLogic)
    {
        _battleLogic = battleLogic;
    }

    public void BattleStart(WBattleData battleData)
    {
        if (Event_BattleStart != null)
            Event_BattleStart(MakeBattleInfo(battleData));
    }
    
    public void Tick(int elapsedTime)
    {
        if (Event_BattleTurnTick != null)
            Event_BattleTurnTick(elapsedTime);
    }

    public void Ping()
    {
        if (Event_BattlePingTick != null)
            Event_BattlePingTick();
    }

    public void TurnChange(WTurnData turnData)
    {
        if (Event_BattleTurnData != null)
            Event_BattleTurnData(MakeTurnData(turnData));
    }

    public void FightAction(WFightAction fightAction)
    {
        if (Event_ActionReceived != null)
            Event_ActionReceived(MakeFightAction(fightAction));
    }

    public void BattleResult(WBattleResult result)
    {
        ChestInfo chestInfo = null;

        if (result.reward.chest_info != null)
            chestInfo = ChestInfoHelper.GetChestInfo(result.reward.chest_info);

        RewardData rewData = new RewardData(result.reward.coin,
                                            result.reward.trophy,
                                            chestInfo);

        BattleResultData btlResultData = new BattleResultData(result.total_score, result.current_rank,
            result.previous_rank, result.victorious, rewData);
        
        if (Event_BattleFinish != null)
            Event_BattleFinish(btlResultData , MakeTroopCooldownList(result.cooldown_data), result.connection_lost);
    }


    //Private Methods
    private BattleInfo MakeBattleInfo(WBattleData battleData)
    {
        BattleInfo bi = new BattleInfo();

        PartyInfo[] partyInfoes = new PartyInfo[2];

        for (int i = 0; i < partyInfoes.Length; i++)
            partyInfoes[i] = MakePartyInfo(battleData.party[i]);

        bi.Init(battleData.turnTime, battleData.turn, partyInfoes,
                    battleData.scene_type, battleData.is_bot,battleData.bot_ai);

        return bi;
    }

    private PartyInfo MakePartyInfo(WBattlePartyData partyData)
    {
        HeroInfo heroInf01 = null;
        List<CharInfo> charInfoes01 = new List<CharInfo>();

        for (int i = 0; i < partyData.troop.Length; i++)
        {
            if (i == 0) //Index of 0 is hero
                heroInf01 = MakeCharatcerInfo(partyData.troop[i], true) as HeroInfo;
            else
                charInfoes01.Add(MakeCharatcerInfo(partyData.troop[i], false));
        }

        //Secrets
        //Now it's only for working
        List<Divine.Secret> secrets = new List<Divine.Secret>(2) { Divine.Secret.Mirror, Divine.Secret.Ransom };

        return new PartyInfo(heroInf01, charInfoes01.ToArray(), secrets, 0, partyData.name);
    }

    private CharInfo MakeCharatcerInfo(WBattleUnitData unit, bool isHero)
    {
        BattleObjStats stats = new BattleObjStats(unit.health,
                                                  unit.maxHealth,
                                                  unit.attack,
                                                  unit.shield, unit.maxShield);

        var spellInfoList = new List<SpellInfo>();

        for (int i = 0; i < unit.spell.Length; i++)
        {
            var appearance = AppearanceConfigData.instance.GetMagicWithIndex(unit.moniker, unit.spell[i].index);

            spellInfoList.Add(new SpellInfo(
                i,
                appearance._isInstant, appearance._dontShowToUser, appearance._needTargetToComeNear,
                appearance._spellName, unit.spell[i].need_ap, appearance._prefabName,
                appearance._spellType, appearance._damageType, appearance._spellImpact, null));

            string spellParams = unit.spell[i].spell_params == null ? "" : unit.spell[i].spell_params.ToString();
            spellInfoList[i].SetSpellParams(spellParams);
        }

        if (isHero)
        {
            HeroInfo heroInf = new HeroInfo(
                unit.moniker,
                stats,
                spellInfoList,
                unit.dexterity,
                0, unit.level, unit.id, 0, 0, new NextUpgradeStats(), unit.is_active);

            heroInf.SetHeroItemsForBattle(unit.items);

            return heroInf;
        }
        else
        {
            var troop = new CharInfo(
            unit.moniker,
            stats,
            spellInfoList,
            unit.dexterity,
            0, unit.level, unit.id, 0, 0, new NextUpgradeStats(), Divine.Moniker.Unknown, unit.is_active);

            return troop;
        }
    }

    private TurnData MakeTurnData(WTurnData turn)
    {
        List<StatsUpdateData> stDataList = new List<StatsUpdateData>();

        for (int i = 0; i < turn.status_update_data.Length; i++)
        {
            StatsUpdateData stData = new StatsUpdateData();
            
            stData.ownerID = turn.status_update_data[i].owner_id;
            stData.finalStats = MakeBattleObject(turn.status_update_data[i].final_stats);
            stData.singleStatChanges = MakeSingleStatChangeList(turn.status_update_data[i].single_stat_changes);

            stDataList.Add(stData);
        }

        var turnData = new TurnData(
            turn.turn_id, 
            new List<int>(turn.eligible_spells),
            new List<int>(turn.eligible_secrets),
            new List<CoolDownData>(), //Spells
            new List<CoolDownData>(), //Secret
            turn.ap[0],
            turn.ap[1],
            stDataList);

        return turnData;
    }
    
    private List<ActionData> MakeFightAction(WFightAction fightActions)
    {
        List<ActionData> actionList = new List<ActionData>();

        for (int n = 0; n < fightActions.f_acts.Length; n++)
        {
            WFightActionData fightAc = fightActions.f_acts[n];

            FightActionData fad = new FightActionData();

            var effectsInfo = new List<SpellEffectInfo>();

            AppearanceConfigData.MagicAppearanceData appearance;
            appearance = AppearanceConfigData.instance.GetMagicWithIndex(_battleLogic.GetCharacterWithID(fightAc.owner_id).moniker,
                                                                                fightAc.spell_index);
            fad.consumedActionPoint = fightAc.con_ap;
            fad.ownerID = fightAc.owner_id;

            //if (appearance._multiplePartDamage != null && appearance._multiplePartDamage.Count > 0)
            //{
            //    effectsInfo = multipart(appearance._multiplePartDamage, effectsInfo);
            //}

            for (int i = 0; i < fightAc.spell_effect_info.Length; i++)
            {
                BattleObjStats finalStat = MakeBattleObject(fightAc.spell_effect_info[i].final_character_stats);

                List<SpellSingleStatChangeInfo> statChanges = MakeSingleStatChangeList(
                                                                fightAc.spell_effect_info[i].single_stat_changes);                                              

                SpellEffectInfo effect = new SpellEffectInfo(statChanges,
                                                             fightAc.spell_effect_info[i].target_character_id,
                                                             fightAc.spell_effect_info[i].effect_on_character,
                                                             finalStat);
                effectsInfo.Add(effect);
            }

            fad.spellInfo = new SpellInfo(
                fightAc.spell_index,
                appearance._isInstant, appearance._dontShowToUser, appearance._needTargetToComeNear,
                appearance._spellName, appearance._cost, appearance._prefabName,
                fightAc.spell_type, appearance._damageType, appearance._spellImpact, effectsInfo);

            fad.spellInfo.generatedActionPoint = fightAc.gen_ap;
            fad.spellInfo.isCritical = fightAc.is_critical;

            actionList.Add(fad);
        }

        return actionList;
    }

    private BattleFlags MakeBattleFlag(WBattleFlags[] wflags)
    {
        BattleFlags battleFlag = BattleFlags.None;

        for (int i = 0; i < wflags.Length; i++)
            battleFlag = battleFlag | (BattleFlags)wflags[i];

        return battleFlag;
    }

    private BattleObjStats MakeBattleObject(WBattleObjStat wbattleObj)
    {
        BattleFlags flags = (BattleFlags)wbattleObj.flag;
        BattleObjStats battleObj = new BattleObjStats(wbattleObj.hp,
                                                      wbattleObj.max_hp,
                                                      wbattleObj.damage,
                                                      wbattleObj.shield,
                                                      wbattleObj.max_shield,
                                                      flags);

        return battleObj;
    }

    private List<SpellSingleStatChangeInfo> MakeSingleStatChangeList(WSingleStatChangeInfo[] wsingleStats)
    {
        List<SpellSingleStatChangeInfo> statChanges = new List<SpellSingleStatChangeInfo>();

        for (int i = 0; i < wsingleStats.Length; i++)
            statChanges.Add(new SpellSingleStatChangeInfo(
                                        wsingleStats[i].character_stat_change_type,
                                        wsingleStats[i].int_val));

        return statChanges;
    }

    private List<TroopCoolDownData> MakeTroopCooldownList(WTroopCoolDownData[] troop_cooldowns)
    {
        List<TroopCoolDownData> clTrpList = new List<TroopCoolDownData>();

        for (int i = 0; i < troop_cooldowns.Length; i++)
            clTrpList.Add(new TroopCoolDownData(troop_cooldowns[i].character_id, troop_cooldowns[i].remain_time));

        return clTrpList;
    }

    //Instance
    private static BattleDataProvider _instance;
    public static BattleDataProvider instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<BattleDataProvider>();

            return _instance;
        }
    }
}

public class BattleResultData
{
    public int totalScore { get; private set; }
    public int currentRank { get; private set; }
    public int previousRank { get; private set; }
    public bool isVictorious { get; private set; }
    public RewardData rewardData { get; private set; }

    public BattleResultData(int totalScore, int currentRank, int previousRank, bool isVictorious, RewardData rewardData)
    {
        this.totalScore = totalScore;
        this.currentRank = currentRank;
        this.previousRank = previousRank;
        this.isVictorious = isVictorious;
        this.rewardData = rewardData;
    }
}

public class RewardData
{
    public int coin { get; private set; }

    public int trophy { get; private set; }

    public ChestInfo chestInfo { get; private set; }

    public RewardData(int coin, int trophy, ChestInfo chestInfo)
    {
        this.coin = coin;
        this.trophy = trophy;
        this.chestInfo = chestInfo;
    }
}
