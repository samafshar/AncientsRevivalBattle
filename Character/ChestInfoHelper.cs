using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Divine;

public static class ChestInfoHelper
{
    public static ChestInfo GetChestInfo(WChestInfo chestInfo)
    {
        var chestType = chestInfo.chest_type;
        var chestStatus = chestInfo.status;

        List<RewardCardData> rewardCardDataList = new List<RewardCardData>();

        ChestRewardData rewardData = new ChestRewardData();

        if (chestInfo.reward_data != null && chestInfo.reward_data.units != null)
        {
            for (int i = 0; i < chestInfo.reward_data.units.Length; i++)
            {
                RewardCardData rc = new RewardCardData();
                rc.count = chestInfo.reward_data.units[i].count;
                rc.moniker = chestInfo.reward_data.units[i].unit;
                rewardCardDataList.Add(rc);
            }

            rewardData = new ChestRewardData(chestType, chestInfo.reward_data.gems, chestInfo.reward_data.coins, rewardCardDataList.ToArray(), chestInfo.reward_data.reward_range);
        }

        return new ChestInfo(
            chestInfo.id,
            chestType,
            chestStatus,
            rewardData,
            chestInfo.initial_time,
            chestInfo.remain_time);
    }
    public static ChestRewardData GetChestRewardData(WChestRewardData wRewardData)
    {
        RewardCardData[] cards = new RewardCardData[wRewardData.units.Length];

        for (Int16 i = 0; i < wRewardData.units.Length; i++)
        {
            cards[i] = new RewardCardData(wRewardData.units[i].count, wRewardData.units[i].unit);
        }

        return new ChestRewardData(wRewardData.chest_type, wRewardData.gems, wRewardData.coins, cards, wRewardData.reward_range);
    }
}

public struct ChestRewardData
{
    public ChestType chestType;
    public RewardCardData[] rewardCards;
    public int gems;
    public int coins;
    public WShopChestRewardData rewardRange;
    
    public ChestRewardData(ChestType chestType, int gems, int coins, RewardCardData[] rewardCards, WShopChestRewardData rewardRange)
    {
        this.chestType = chestType;
        this.gems = gems;
        this.coins = coins;
        this.rewardCards = rewardCards;
        this.rewardRange = rewardRange;
    }
}

public struct RewardCardData
{
    public int count;
    public Moniker moniker;

    public RewardCardData(int count, Moniker moniker)
    {
        this.count = count;
        this.moniker = moniker;
    }
}
