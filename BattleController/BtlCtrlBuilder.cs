using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IBtlCtrlBuilder
{
    BattleController Build();
}

public enum BattleControllerType
{
    Tutorial_MiniBoss,
    Tutorial_Zahak,
}

public class BtlCtrlBuilder
{
    public static BattleController Build(BattleControllerType type)
    {
        IBtlCtrlBuilder espcialBuilder = null;

        switch (type)
        {
            case BattleControllerType.Tutorial_MiniBoss:
                espcialBuilder = new BtlCtrlBuilder_TutMiniBoss();
                break;
            case BattleControllerType.Tutorial_Zahak:
                espcialBuilder = new BtlCtrlBuilder_TutZahak();
                break;
        }

        BattleController battleController = espcialBuilder.Build();

        BattleLogic battleLogic = MakeBattleLogic(battleController);

        battleController.SetBattleLogic(battleLogic);

        return battleController;
    }

    private static BattleLogic MakeBattleLogic(BattleController battleController)
    {
        return BattleBuilder.Build(MakeBattleInfo(battleController));
    }

    private static BattleInfo MakeBattleInfo(BattleController battleController)
    {
        BattleInfo battleInfo = new BattleInfo();

        PartyInfo[] partyInfoes = new PartyInfo[2];
        for (int i = 0; i < partyInfoes.Length; i++)
            partyInfoes[i] = MakePartyInfo(battleController.parties[i]);

        battleInfo.Init(battleController.turnTime,
            battleController.turnOrders.ToArray(),
            partyInfoes, battleController.sceneType);

        return battleInfo;
    }

    private static PartyInfo MakePartyInfo(BtlCtrlParty btlCtrlParty)
    {
        HeroInfo heroInf01 = null;
        List<CharInfo> charInfoes01 = new List<CharInfo>();

        List<BtlCtrlCharacter> characterList = btlCtrlParty.GetCharacters();

        foreach (BtlCtrlCharacter ch in characterList)
        {
            if (ch.type == BtlCtrlCharacter.Type.Hero)
                heroInf01 = MakeCharatcerInfo(ch, true) as HeroInfo;
            else
                charInfoes01.Add(MakeCharatcerInfo(ch, false));
        }
        
        //Secrets
        List<Divine.Secret> secrets = new List<Divine.Secret>(2) { Divine.Secret.Mirror, Divine.Secret.Ransom };

        return new PartyInfo(heroInf01, charInfoes01.ToArray(), secrets, 0, btlCtrlParty.partyName);
    }

    private static CharInfo MakeCharatcerInfo(BtlCtrlCharacter character, bool isHero)
    {
        BattleObjStats stats = new BattleObjStats(character.hp,
                                                  character.maxHp,
                                                  character.spells[0].damage,
                                                  character.shield, character.maxShield);

        var spellInfoList = new List<SpellInfo>();

        for (int i = 0; i < AppearanceConfigData.instance.GetUnitMagicsCount(character.moniker); i++)
        {
            if (i >= character.spells.Count)
                break;
            
            var appearance = AppearanceConfigData.instance.GetMagicWithIndex(character.moniker, i);

            SpellInfo spInf = new SpellInfo(
                i,
                appearance._isInstant, appearance._dontShowToUser, appearance._needTargetToComeNear,
                appearance._spellName, appearance._cost, appearance._prefabName,
                appearance._spellType, appearance._damageType, appearance._spellImpact, null);
                        
            spInf.SetSpellParams(character.spells[i].spellParams);

            spellInfoList.Add(spInf);            
        }

        if (isHero)
        {
            HeroInfo heroInf = new HeroInfo(
                character.moniker,
                stats,
                spellInfoList,
                Dexterity.MIDDLE,
                0, character.level, character.id, 0, 0, new NextUpgradeStats(), character.isActive);

            return heroInf;
        }
        else
        {
            var troop = new CharInfo(
            character.moniker,
            stats,
            spellInfoList,
            Dexterity.MIDDLE,
            0, character.level, character.id, 0, 0, new NextUpgradeStats(), 
            Divine.Moniker.Unknown, character.isActive);

            return troop;
        }
    }
}
