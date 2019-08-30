using System.Collections.Generic;
using Divine;

public class BtlCtrlBuilder_TutZahak : IBtlCtrlBuilder
{
    BattleControllerDataFiller _dataFiller;

    public BattleController Build()
    {
        _dataFiller = new BattleControllerDataFiller();

        BattleController battleController = new BattleController();

        List<long> turns = new List<long>();
        List<BtlCtrlCharacter> ai = new List<BtlCtrlCharacter>();
        List<BtlCtrlCharacter> player = new List<BtlCtrlCharacter>();

        turns.AddRange(new long[5] { 0, 3, 1, 2, 4 });

        ai.Add(MakeTutCharacter(Moniker.Zahaak, 4, 1, true));
        ai.Add(MakeTutCharacter(Moniker.ClericChakra, 5, 1, false));

        player.Add(MakeTutCharacter(Moniker.Fereydoun, 0, 1, true));
        player.Add(MakeTutCharacter(Moniker.Archer, 1, 1, false));
        player.Add(MakeTutCharacter(Moniker.Hanzo, 2, 1, false));
        player.Add(MakeTutCharacter(Moniker.FereydounChakra, 3, 1, false));

        battleController.Init(
            player, 
            ai,
            LocalizationManager.instance.GetString(Moniker.Fereydoun.ToString()),
            LocalizationManager.instance.GetString(Moniker.Zahaak.ToString()),
            turns, 
            BattleControllerType.Tutorial_Zahak,
            25,
            BattleSceneType.Tutorial_Hell);

        return battleController;
    }

    private BtlCtrlCharacter MakeTutCharacter(Moniker moniker, int id, int level, bool isActive)
    {
        BtlCtrlCharacter character = _dataFiller.FillCharacterData(moniker, level, BattleControllerType.Tutorial_Zahak);

        character.id = id;
        character.isActive = isActive;

        return character;
    }
}
