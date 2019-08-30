using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpInfo
{
    public int attack;
    public int max_health;
    public int max_shield;
    public float miss_chance;
    public float dodge_chance;
    public float critical_ratio;
    public float critical_chance;
    public WNextStats nextStats;
    public WChakraData chakraData;

    public LevelUpInfo(){}
    public LevelUpInfo(int attack, int maxHealth, int maxShield, float missChance, float dodgeChance, float criticalRatio, float criticalChance, WNextStats nextStats, WChakraData chakraData)
    {
        this.attack = attack;
        max_health = maxHealth;
        max_shield = maxShield;
        miss_chance = missChance;
        dodge_chance = dodgeChance;
        critical_ratio = criticalRatio;
        critical_chance = criticalChance;
        this.nextStats = nextStats;
        this.chakraData = chakraData;
    }
}

public static class LevelUpHelper {
    public static LevelUpInfo GetLevelUpInfo(WHeroData heroData)
    {
        return new LevelUpInfo(heroData.attack,
            heroData.max_health,
            heroData.max_shield,
            heroData.miss_chance,
            heroData.dodge_chance,
            heroData.critical_ratio,
            heroData.critical_chance,
            heroData.next_upgrade_stats,
            heroData.chakra);
    }
    public static LevelUpInfo GetLevelUpInfo(WUnitData unitData)
    {
        return new LevelUpInfo(unitData.attack,
            unitData.max_health,
            unitData.max_shield,
            unitData.miss_chance,
            unitData.dodge_chance,
            unitData.critical_ratio,
            unitData.critical_chance,
            unitData.next_upgrade_stats,
            null);
    }
    public static void Net_CharLevelUp(NewUIGroup relatedGroup, CharInfo charInfo, Action actionOnSuccess = null, Action actionOnFail = null )
    {
        relatedGroup.SetIsBusyInLogicForAMoment();

        NetRequestPage netReqPage = NewUIGroup.CreateGroup(NewUIGroup.NAME__NETREQUESTPAGE, relatedGroup) as NetRequestPage;
        netReqPage.Init("upgrading");

            DivineDebug.Log("Net: CharLevelUp request sent. Char: '" + charInfo.moniker.ToString() + "'.");
            
            NewNetworkManager.instance.CharLevelUp(charInfo,
                (data) =>
                {
                    DivineDebug.Log("Old MaxHP: " + charInfo.baseStats.maxHp);
                    DivineDebug.Log("Next MaxHP: " + charInfo.nextUpgradeStats.health);
                    
                    DivineDebug.Log("Net: CharLevelUp was successful. Char: '" + charInfo.moniker.ToString() + "'.");
                    netReqPage.SetSuccessHappened("done");

                    GameManager.instance.player.coin -= charInfo.nextUpgradeStats.card_cost;
                    charInfo.curCardCount -= charInfo.nextUpgradeStats.card_count; 

                    GameAnalyticsSDK.GameAnalytics.NewDesignEvent("Level up character : " + charInfo.moniker.ToString());
                    
                    charInfo.level++;
                    charInfo.SetBaseStats(data.max_health, data.max_health, data.attack, data.max_shield, data.max_shield);
                    charInfo.SetOtherStats(data.critical_ratio, data.critical_chance, data.dodge_chance);
                    charInfo.nextUpgradeStats = CharInfoHelper.GetNextUpgradeStats(data.nextStats);

                    if (data.chakraData != null)
                    {
                        HeroInfo heroInfo = (HeroInfo) charInfo;

                        heroInfo.chakra.level++;
                        
                        heroInfo.chakra.SetBaseStats(data.chakraData.chakra_max_health,
                                                     data.chakraData.chakra_max_health,
                                                     data.chakraData.chakra_attack,
                                                     data.chakraData.chakra_max_shield,
                                                     data.chakraData.chakra_max_shield);
                        
                        heroInfo.chakra.SetOtherStats(data.chakraData.chakra_critical_ratio,
                                                      data.chakraData.chakra_critical_chance,
                                                      data.chakraData.chakra_dodge_chance);
                        
                        heroInfo.chakra.nextUpgradeStats = CharInfoHelper.GetNextUpgradeStats(data.chakraData.next_upgrade_stats);
                    }

                    if (actionOnSuccess != null)
                        actionOnSuccess();

                    DivineDebug.Log("MaxHP: " + charInfo.baseStats.maxHp);
                    DivineDebug.Log("MaxHP: " + charInfo.nextUpgradeStats.health);
                },
                (errorMsg) =>
                {
                    DivineDebug.Log("Net: CharLevelUp failed. Char: '" + charInfo.moniker.ToString() + "'.");
                    netReqPage.SetFailHappened("connectionError");

                    if (actionOnFail != null)
                        actionOnFail();
                });
    }
}
