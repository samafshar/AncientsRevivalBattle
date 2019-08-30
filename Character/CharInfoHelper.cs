using System.Collections.Generic;
using Divine;

public static class CharInfoHelper
{
    public static HeroInfo GetHeroInfo(WHeroData heroData)
    {
        var stats = new BattleObjStats(
            heroData.health, heroData.max_health, heroData.attack, heroData.shield, heroData.max_shield);

        HeroInfo hInfo = new HeroInfo(
            heroData.moniker, stats, CharInfoHelper.GetCharacterSpells(heroData.moniker),
            heroData.dexterity, 0, heroData.level, heroData.id, 0,
            heroData.quantity,GetNextUpgradeStats(heroData.next_upgrade_stats));

        hInfo.SetOtherStats(heroData.critical_ratio,
                            heroData.critical_chance,
                            heroData.miss_chance);

        hInfo.SetChakra(heroData.chakra, GetCharacterSpells(heroData.chakra.chakra_moniker));
        
        hInfo.SetIsSelectedHero(heroData.selected_hero);

        hInfo.SetHeroItems(heroData.items);

        return hInfo;
    }
    
    public static CharInfo GetCharInfo(WUnitData troopData)
    {
        var stats = new BattleObjStats(
            troopData.health, troopData.max_health, troopData.attack, troopData.shield, troopData.max_shield);

        CharInfo chInf = new CharInfo(
            troopData.moniker, stats, CharInfoHelper.GetCharacterSpells(troopData.moniker),
            troopData.dexterity, 0, troopData.level, 
            troopData.id, troopData.cool_down_remaining_seconds, troopData.quantity,
            GetNextUpgradeStats(troopData.next_upgrade_stats), troopData.ownerMoniker);

        chInf.SetOtherStats(troopData.critical_ratio,
                            troopData.critical_chance,
                            troopData.miss_chance);

        //chInf.SetUnlockData(troopData.used_status, troopData.unlock_league, troopData.unlock_league_step_number);
        chInf.SetUnlockData(WCharExistenceType.unlock, troopData.unlock_league, troopData.unlock_league_step_number);

        return chInf;
    }

    public static NextUpgradeStats GetNextUpgradeStats(WNextStats nextStats)
    {
        if (nextStats == null) return null;

        NextUpgradeStats stats = new NextUpgradeStats();

        stats.attack = nextStats.attack;
        stats.card_cost = nextStats.card_cost;
        stats.card_count = nextStats.card_count;
        stats.critical_chance = nextStats.critical_chance;
        stats.critical_ratio = nextStats.critical_ratio;
        stats.dodge_chance = nextStats.dodge_chance;
        stats.health = nextStats.health;
        stats.miss_chance = nextStats.miss_chance;
        stats.shield = nextStats.shield;

        return stats;
    }

    public static List<SpellInfo> GetCharacterSpells(Divine.Moniker moniker)
    {
        var spellInfoList = new List<SpellInfo>();

        var spellCount = AppearanceConfigData.instance.GetUnitMagicsCount(moniker);
        for (var index = 0; index < spellCount; ++index)
        {
            var appearance = AppearanceConfigData.instance.GetMagicWithIndex(moniker, index);
            spellInfoList.Add(new SpellInfo(
                index,
                appearance._isInstant, appearance._dontShowToUser, appearance._needTargetToComeNear,
                appearance._spellName, appearance._cost, appearance._prefabName,
                appearance._spellType, appearance._damageType, appearance._spellImpact, null));
        }

        return spellInfoList;
    }

    public static bool IsMonikerHero(Moniker moniker)
    {
        return moniker == Moniker.Warrior
               || moniker == Moniker.WarriorChakra
               || moniker == Moniker.Cleric
               || moniker == Moniker.ClericChakra
               || moniker == Moniker.Wizard
               || moniker == Moniker.WizardChakra;
    }
}
