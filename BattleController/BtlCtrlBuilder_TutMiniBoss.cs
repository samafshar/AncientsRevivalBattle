using System;
using System.Collections.Generic;
using Divine;

public class BtlCtrlBuilder_TutMiniBoss : IBtlCtrlBuilder
{
    BattleControllerDataFiller _dataFiller;

    public BattleController Build()
    {
        _dataFiller = new BattleControllerDataFiller();

        BattleController battleController = new BattleController();

        List<long> turns = new List<long>();
        List<BtlCtrlCharacter> ai = new List<BtlCtrlCharacter>();
        List<BtlCtrlCharacter> player = new List<BtlCtrlCharacter>();

        turns.AddRange(new long[8] { 4, 0, 3, 1, 2, 6, 7, 8 });

        ai.Add(MakeTutCharacter(Moniker.SkullWizard, 4, 1, true));
        ai.Add(MakeTutCharacter(Moniker.Skeleton_Swordman, 5, 1, false));
        ai.Add(MakeTutCharacter(Moniker.Skeleton_Archer, 6, 3, false));
        ai.Add(MakeTutCharacter(Moniker.Skeleton_Archer, 7, 2, false));
        ai.Add(MakeTutCharacter(Moniker.Skeleton_Swordman, 8, 3, false));
        ai.Add(MakeTutCharacter(Moniker.ClericChakra, 9, 3, false));

        player.Add(MakeTutCharacter(Moniker.Fereydoun, 0, 1, true));
        player.Add(MakeTutCharacter(Moniker.Archer, 1, 1, false));
        player.Add(MakeTutCharacter(Moniker.Hanzo, 2, 1, false));
        player.Add(MakeTutCharacter(Moniker.FereydounChakra, 3, 1, false));

        battleController.Init(
            player, 
            ai, 
            LocalizationManager.instance.GetString(Moniker.Fereydoun.ToString()), 
            LocalizationManager.instance.GetString(Moniker.SkullWizard.ToString()),
            turns,
            BattleControllerType.Tutorial_MiniBoss,
            0,
            BattleSceneType.Tutorial_Castle);

        return battleController;
    }

    private BtlCtrlCharacter MakeTutCharacter(Moniker moniker, int id,int level, bool isActive)
    {
        BtlCtrlCharacter character = _dataFiller.FillCharacterData(moniker, level, BattleControllerType.Tutorial_MiniBoss);

        character.id = id;
        character.isActive = isActive;

        return character;
    }
}
