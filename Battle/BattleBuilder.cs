using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBuilder
{
    public static BattleLogic Build(BattleInfo battleInfo)
    {
        for (int i = 0; i < 2; i++)
            AddPartyInLoadingQueue(battleInfo.partyInfoes[i]);

        BattleLogic result = new BattleLogic(battleInfo);

        return result;
    }

    private static void AddPartyInLoadingQueue(PartyInfo partyInfo)
    {
        CharacterLoader.instance.AddCharacterVisualToLoad(
                                    new List<Divine.Moniker> { partyInfo.heroInfo.moniker }
                                    );

        List<Divine.Moniker> mons = new List<Divine.Moniker>();
        for (int j = 0; j < partyInfo.charInfoes.Length; j++)
            mons.Add(partyInfo.charInfoes[j].moniker);

        CharacterLoader.instance.AddCharacterVisualToLoad(mons);
    }
}
