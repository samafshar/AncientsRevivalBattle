using System;
using System.Collections.Generic;
using System.Linq;

public class FightDataAnalyzer
{
    private const string PREFAB_DAMAGERETURN = "Spell_LinkerDamageReturn";

    public OpponentInfo Player { get; set; }

    public OpponentInfo Opponent { get; set; }

    public int TurnTime { get; set; }

    public long[] Turns { get; set; }

    private Dictionary<long, Divine.Moniker> _monikers;

    private Dictionary<long, BattleObjStats> _curStats;

    public List<StatsUpdateData> ChangeTurn(IEnumerable<Divine.StatUpdateData> updatesData)
    {
        var statUpdates = new List<StatsUpdateData>();

        foreach (var updateData in updatesData)
        {
            var statUpdate = new StatsUpdateData();
            statUpdate.ownerID = updateData.Id;
            statUpdate.singleStatChanges = new List<SpellSingleStatChangeInfo>();

            var stat = _curStats[updateData.Id];
            int hpChange = 0;
            int shieldChange = 0;

            foreach (var change in updateData.Change)
            {
                switch (change.Type)
                {
                    case Divine.StatType.Health:
                        hpChange += change.Value;                        
                        stat.SetHP(stat.hp + change.Value);
                        break;
                    case Divine.StatType.Shield:
                        shieldChange += change.Value;
                        stat.SetShield(stat.shield + change.Value);
                        break;
                    case Divine.StatType.Attack:
                        statUpdate.singleStatChanges.Add(new SpellSingleStatChangeInfo(
                            SpellSingleStatChangeType.curDamageValChange,
                            change.Value));
                        stat.damage += change.Value;
                        break;
                    case Divine.StatType.Flag:
                        statUpdate.singleStatChanges.Add(new SpellSingleStatChangeInfo(
                            SpellSingleStatChangeType.curFlagValChange,
                            change.Value));
                        stat.flags = (BattleFlags)change.Value;
                        break;
                }
            }

            if (hpChange != 0)
            {
                statUpdate.singleStatChanges.Add(new SpellSingleStatChangeInfo(
                    SpellSingleStatChangeType.curHPValChange,
                    hpChange));
            }

            if (shieldChange != 0)
            {
                statUpdate.singleStatChanges.Add(new SpellSingleStatChangeInfo(
                    SpellSingleStatChangeType.curShieldValChange,
                    shieldChange));
            }

            statUpdate.finalStats = (BattleObjStats)stat.Clone();
            statUpdates.Add(statUpdate);
        }

        return statUpdates;
    }

    private void InitPartyStats(PartyInfo party)
    {
        _monikers[party.heroInfo.uniqueID] = party.heroInfo.moniker;
        _curStats[party.heroInfo.uniqueID] = (BattleObjStats)party.heroInfo.baseStats.Clone();

        foreach (var troop in party.charInfoes)
        {
            _monikers[troop.uniqueID] = troop.moniker;
            _curStats[troop.uniqueID] = (BattleObjStats)troop.baseStats.Clone();
        }
    }

    public void PrintStat()
    {
        int i = 0;

        foreach (var item in _curStats.Values)
        {
            DivineDebug.LogFormat("Name : {0}      Final Health : {1}", _monikers[_curStats.Keys.ElementAt(i++)], item.hp);
        }
    }

    private void InitBattle()
    {
        _curStats = new Dictionary<long, BattleObjStats>();
        _monikers = new Dictionary<long, Divine.Moniker>();

        InitPartyStats(Player.partyInfo);
        InitPartyStats(Opponent.partyInfo);
    }

    public BattleInfo MakeBattleInfo()
    {
        InitBattle();

        var battleInfo = new BattleInfo();
        battleInfo.Init(
            TurnTime, Turns,
            new PartyInfo[] { Player.partyInfo, Opponent.partyInfo },
            BattleSceneType.Jungle);           

        return battleInfo;
    }

    public List<ActionData> MakeFightActionDataList(Divine.BattleActionData action)
    {
        var result = new List<ActionData>();

        foreach (var spell in action.Spell)
        {
            var effectsInfo = new List<SpellEffectInfo>();
            var isCritical = spell.Effect.Count > 0 && spell.Effect[0].Type == Divine.SpellEffectType.Critical;

            foreach (var effect in spell.Effect)
            {
                var statChanges = new List<SpellSingleStatChangeInfo>();
                var stat = _curStats[effect.Target];

                int hpChange = 0;
                int shieldChange = 0;

                foreach (var change in effect.Change)
                {
                    switch (change.Type)
                    {
                        case Divine.StatType.Health:
                            hpChange += change.Value;
                            stat.SetHP(stat.hp + change.Value);
                            break;
                        case Divine.StatType.Shield:
                            shieldChange += change.Value;
                            stat.SetShield(stat.shield + change.Value);
                            break;
                        case Divine.StatType.Attack:
                            statChanges.Add(new SpellSingleStatChangeInfo(
                                SpellSingleStatChangeType.curDamageValChange,
                                change.Value));
                            stat.damage += change.Value;
                            break;
                        case Divine.StatType.Flag:
                            statChanges.Add(new SpellSingleStatChangeInfo(
                                SpellSingleStatChangeType.curFlagValChange,
                                 change.Value));
                            stat.flags = (BattleFlags)change.Value;
                            break;
                    }
                }

                if (hpChange != 0)
                {
                    statChanges.Add(new SpellSingleStatChangeInfo(
                        SpellSingleStatChangeType.curHPValChange,
                        hpChange));

                    if (effect.Target > 0 && _curStats.ContainsKey(-effect.Target))
                    {
                        _curStats[-effect.Target].SetHP(_curStats[-effect.Target].hp + hpChange);
                    }
                }

                if (shieldChange != 0)
                {
                    statChanges.Add(new SpellSingleStatChangeInfo(
                        SpellSingleStatChangeType.curShieldValChange,
                        shieldChange));
                }
                
                effectsInfo.Add(new SpellEffectInfo(statChanges, effect.Target, translate(effect.Type), (BattleObjStats)stat.Clone()));
            }

            AppearanceConfigData.MagicAppearanceData appearance;
            int spellIndex;
            if (spell.Spell < 100)
            {
                spellIndex = spell.Spell;
                appearance = AppearanceConfigData.instance.GetMagicWithIndex(_monikers[spell.Attacker], spellIndex);

                if (appearance._multiplePartDamage != null && appearance._multiplePartDamage.Count > 0)
                {
                    effectsInfo = multipart(appearance._multiplePartDamage, effectsInfo);
                }
                
                var fightAction = new FightActionData();
                fightAction.consumedActionPoint = spell.ConsumedActionPoint;
                fightAction.ownerID = spell.Attacker;
                fightAction.spellInfo.generatedActionPoint = spell.GeneratedActionPoint;

                result.Add(fightAction);
            }
            else if (spell.Spell < 1000)
            {
                spellIndex = spell.Spell - 100;
                var secret = getSide(spell.Attacker).partyInfo.availableSecrets[spellIndex];
                appearance = AppearanceConfigData.instance.GetSecretWithIndex(secret);

                var fightAction = new SecretActionData();
                fightAction.ownerID = spell.Attacker;
                fightAction._spellInfo = new SpellInfo(
                    spellIndex,
                    true, appearance._dontShowToUser, appearance._needTargetToComeNear,
                    appearance._spellName, appearance._cost, appearance._prefabName,
                    appearance._spellType, appearance._damageType, appearance._spellImpact, effectsInfo);
                fightAction._spellInfo.generatedActionPoint = spell.GeneratedActionPoint;

                result.Add(fightAction);
            }
            else
            {
                var fightAction = new FightActionData();
                fightAction.ownerID = spell.Attacker;
                fightAction.spellInfo = new SpellInfo(
                    0, false, false, false,
                    PREFAB_DAMAGERETURN, 0, PREFAB_DAMAGERETURN,
                    SpellType.DamageReturn, DamageType.Low, SpellImpactType.None, effectsInfo);

                result.Add(fightAction);
            }
        }

        return result;
    }

    private SpellEffectOnChar translate(Divine.SpellEffectType type)
    {
        switch (type)
        {
            case Divine.SpellEffectType.Normal:
                return SpellEffectOnChar.NormalDamage;
            case Divine.SpellEffectType.Critical:
                return SpellEffectOnChar.SeriousDamage;
            case Divine.SpellEffectType.Miss:
                return SpellEffectOnChar.Miss;
            case Divine.SpellEffectType.Dodge:
                return SpellEffectOnChar.Dodge;
            case Divine.SpellEffectType.Buff:
                return SpellEffectOnChar.Buff;
            case Divine.SpellEffectType.Nerf:
                return SpellEffectOnChar.Nerf;
            case Divine.SpellEffectType.Taunt:
                return SpellEffectOnChar.Taunt;
            case Divine.SpellEffectType.Burn:
                return SpellEffectOnChar.Burn;
            case Divine.SpellEffectType.Fear:
                return SpellEffectOnChar.Fear;
            case Divine.SpellEffectType.Appear:
                return SpellEffectOnChar.Appear;
            case Divine.SpellEffectType.Revive:
                return SpellEffectOnChar.Revive;
            case Divine.SpellEffectType.Prepare:
                return SpellEffectOnChar.Prepare;
            case Divine.SpellEffectType.Protected:
                return SpellEffectOnChar.Protect;
            default:
                return SpellEffectOnChar.None;
        }
    }

    private List<SpellEffectInfo> multipart(List<int> _multiplePartDamage, List<SpellEffectInfo> effectsInfo)
    {
        var mpEffectsInfo = new List<SpellEffectInfo>();
        var parts = new List<SpellEffectInfo>[_multiplePartDamage.Count];
    
        int i, j;
        for (i = 0; i < _multiplePartDamage.Count; i++)
        {
            parts[i] = new List<SpellEffectInfo>();
        }

        foreach (var spi in effectsInfo)
        {
            if (spi.effectOnCharacter != SpellEffectOnChar.NormalDamage && spi.effectOnCharacter != SpellEffectOnChar.SeriousDamage)
            {
                for (i = 0; i < parts[0].Count; i++)
                {
                    for (j = 0; j < _multiplePartDamage.Count; j++)
                    {
                        mpEffectsInfo.Add(parts[j][i]);
                    }
                }

                for (i = 0; i < _multiplePartDamage.Count; i++)
                {
                    parts[i].Clear();
                }

                mpEffectsInfo.Add(spi);
                continue;
            }

            var shield = 0;
            var health = 0;
            foreach (var ssc in spi.singleStatChanges)
            {
                switch (ssc.charStatChangeType)
                {
                    case SpellSingleStatChangeType.curHPValChange:
                        health -= ssc.intVal;
                        break;
                    case SpellSingleStatChangeType.curShieldValChange:
                        shield -= ssc.intVal;
                        break;
                }
            }

            var damage = shield + health;
            for (i = 0; i < _multiplePartDamage.Count - 1; i++)
            {
                var part = (damage * _multiplePartDamage[i] + 50) / 100;
                if (shield >= part)
                {
                    var statChanges = new List<SpellSingleStatChangeInfo>();
                    statChanges.Add(new SpellSingleStatChangeInfo(SpellSingleStatChangeType.curShieldValChange, -part));
                    parts[i].Add(new SpellEffectInfo(statChanges, spi.targetCharacterID, spi.effectOnCharacter, spi.finalCharacterStats));
                    shield -= part;
                }
                else if (shield == 0)
                {
                    var statChanges = new List<SpellSingleStatChangeInfo>();
                    statChanges.Add(new SpellSingleStatChangeInfo(SpellSingleStatChangeType.curHPValChange, -part));
                    parts[i].Add(new SpellEffectInfo(statChanges, spi.targetCharacterID, spi.effectOnCharacter, spi.finalCharacterStats));
                    health -= part;
                }
                else
                {
                    var statChanges = new List<SpellSingleStatChangeInfo>();
                    statChanges.Add(new SpellSingleStatChangeInfo(SpellSingleStatChangeType.curShieldValChange, -shield));
                    statChanges.Add(new SpellSingleStatChangeInfo(SpellSingleStatChangeType.curHPValChange, -(part - shield)));
                    parts[i].Add(new SpellEffectInfo(statChanges, spi.targetCharacterID, spi.effectOnCharacter, spi.finalCharacterStats));
                    health -= part;
                    shield = 0;
                }

                damage -= part;
            }

            if (shield >= damage)
            {
                var statChanges = new List<SpellSingleStatChangeInfo>();
                statChanges.Add(new SpellSingleStatChangeInfo(SpellSingleStatChangeType.curShieldValChange, -damage));
                parts[i].Add(new SpellEffectInfo(statChanges, spi.targetCharacterID, spi.effectOnCharacter, spi.finalCharacterStats));
            }
            else if (shield == 0)
            {
                var statChanges = new List<SpellSingleStatChangeInfo>();
                statChanges.Add(new SpellSingleStatChangeInfo(SpellSingleStatChangeType.curHPValChange, -damage));
                parts[i].Add(new SpellEffectInfo(statChanges, spi.targetCharacterID, spi.effectOnCharacter, spi.finalCharacterStats));
            }
            else
            {
                var statChanges = new List<SpellSingleStatChangeInfo>();
                statChanges.Add(new SpellSingleStatChangeInfo(SpellSingleStatChangeType.curShieldValChange, -shield));
                statChanges.Add(new SpellSingleStatChangeInfo(SpellSingleStatChangeType.curHPValChange, -(damage - shield)));
                parts[i].Add(new SpellEffectInfo(statChanges, spi.targetCharacterID, spi.effectOnCharacter, spi.finalCharacterStats));
            }
        }

        for (i = 0; i < parts[0].Count; i++)
        {
            for (j = 0; j < _multiplePartDamage.Count; j++)
            {
                mpEffectsInfo.Add(parts[j][i]);
            }
        }

        return mpEffectsInfo;
    }

    private OpponentInfo getSide(long troopId)
    {
        if (Player.partyInfo.heroInfo.uniqueID == troopId)
            return Player;

        foreach (var charInfo in Player.partyInfo.charInfoes)
        {
            if (charInfo.uniqueID == troopId)
                return Player;
        }

        return Opponent;
    }
}
