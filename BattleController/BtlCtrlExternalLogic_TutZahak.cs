using System;
using System.Collections.Generic;
using UnityEngine;

public class BtlCtrlExternalLogic_TutZahak : IBtlCtrlExternalLogic
{
    private bool _firstAttack = false;
    private Action _onFinish;
    private BattleController _battleController;
    private Tutorial_ZahakBattle _tutorialCtrl;

    private Tutorial_ZahakBattle tutorialCtrl
    {
        get
        {
            if (_tutorialCtrl == null)
                _tutorialCtrl = (Tutorial_ZahakBattle)TutorialManager.instance.GetTutorial();
            return _tutorialCtrl;
        }
    }

    //Public Methods
    public void Init(BattleController battleController)
    {
        _battleController = battleController;

        tutorialCtrl.Event_StateChanged += OnTutorialStateChanged;
    }

    public void MakeDecision(BtlCtrlCharacter currentSlcCharacter,
                             BtlCtrlParty[] parties,
                             Action<long, int, List<long>> onMove,
                             Action<long, Divine.Secret, List<long>> onSecret)
    {
        //Player turn and we should wait till movement
        if (parties[0].HasCharacter(currentSlcCharacter))
        {
            //Player Logic
            if (tutorialCtrl.GetTutorialState() == Tutorial_ZahakBattle.State.SecondFight_Fereydoun_WaitTillReavilling)
                onSecret(currentSlcCharacter.id, Divine.Secret.Revival, new List<long> { 1, 2 });
            return;
        }        

        DivineDebug.Log(tutorialCtrl.GetTutorialState().ToString());

        switch (tutorialCtrl.GetTutorialState())
        {
            case Tutorial_ZahakBattle.State.SecondFight_WaitZahaakFirstAttack:
                NormalAttack(currentSlcCharacter, parties, Divine.Moniker.Hanzo, onMove);
                break;
                
            case Tutorial_ZahakBattle.State.SecondFight_ZahakSpecialAttack:
                NormalAttack(currentSlcCharacter, parties, Divine.Moniker.Archer, onMove, 1);
                break;

            case Tutorial_ZahakBattle.State.SecondFight_ZahakAttackToChakra:
                NormalAttack(currentSlcCharacter, parties, Divine.Moniker.FereydounChakra, onMove);
                break;

            case Tutorial_ZahakBattle.State.SecondFight_ZahakKillChakra:
                NormalAttack(currentSlcCharacter, parties, Divine.Moniker.FereydounChakra, onMove, 1);
                break;
        }
    }

    public void OnTick(int elapsedTime)
    {
        switch (_tutorialCtrl.GetTutorialState())
        {
            case Tutorial_ZahakBattle.State.SecondFight_WaitForTroopFirstAttack:
                if (elapsedTime >= (_battleController.turnTime / 2))
                {
                    _battleController.Pause();
                    _tutorialCtrl.StartEmphasisForAttack();
                }
                break;

            case Tutorial_ZahakBattle.State.SecondFight_WaitAgainForTroopFirstAttack:
                if(elapsedTime + 1>= _battleController.turnTime)
                {
                    _battleController.ResetTimer();
                    _battleController.Pause();
                    _tutorialCtrl.RepeatTheTimePart();
                }
                break;
        }
    }

    public void MoveDone(BtlCtrlCharacter btlCtrlCharacter)
    {
        if (!_firstAttack && btlCtrlCharacter.moniker == Divine.Moniker.Archer)
        {
            _firstAttack = true;
            _tutorialCtrl.FirstAttackDone();
        }
    }

    public void FightFinished(Action onFightFinished)
    {
        _onFinish = onFightFinished;

        _tutorialCtrl.BattleFinished();
    }

    public void DestroyIt()
    {
        _onFinish = null;
        _battleController = null;
        _tutorialCtrl = null;
    }


    //Private Methods
    private void NormalAttack(BtlCtrlCharacter currentSlcCharacter,
                                BtlCtrlParty[] parties,
                                BattleController.TeamTypes teamType,
                                Action<long, int, List<long>> onMove)
    {
        Divine.Moniker moniker;

        int partyIndex = teamType == BattleController.TeamTypes.Player ? 0 : 1;

        List<BtlCtrlCharacter> chs = parties[partyIndex].GetCharacters();
        List<BtlCtrlCharacter> availableCh = new List<BtlCtrlCharacter>();

        for (int i = 0; i < chs.Count; i++)
        {
            if (chs[i].isActive && !chs[i].isDead)
                availableCh.Add(chs[i]);
        }

        int rnd = UnityEngine.Random.Range(0, availableCh.Count);
        moniker = availableCh[rnd].moniker;

        NormalAttack(currentSlcCharacter, parties, moniker, onMove);
    }

    private void NormalAttack(BtlCtrlCharacter currentSlcCharacter,
                                BtlCtrlParty[] parties,
                                Divine.Moniker targetMoniker,
                                Action<long, int, List<long>> onMove,
                                int spellIndex = 0)
    {
        int indexOfSpell = spellIndex;
        BtlCtrlCharacter target = null;

        List<BtlCtrlCharacter> playerCharacters = parties[0].GetCharacters();

        foreach (BtlCtrlCharacter ch in playerCharacters)
        {
            if (ch.hp > 0 && ch.moniker == targetMoniker)
            {
                target = ch;
                break;
            }
        }

        if (onMove != null)
            onMove(currentSlcCharacter.id, indexOfSpell, new List<long> { target.id });
    }


    //Handlers
    private void OnTutorialStateChanged(Tutorial_ZahakBattle.State newState)
    {
        switch (newState)
        {
            case Tutorial_ZahakBattle.State.SecondFight_TimeIntro:
            case Tutorial_ZahakBattle.State.SecondFight_TroopDeadInfo:
            case Tutorial_ZahakBattle.State.SecondFight_ZahakPiffAfterFeriAttack:
            case Tutorial_ZahakBattle.State.SecondFight_FeriDialogBeforeAllies:
                _battleController.Pause();
                break;

            case Tutorial_ZahakBattle.State.SecondFight_Fereydoun_WaitTillReavilling:
            case Tutorial_ZahakBattle.State.SecondFight_WaitForTroopFirstAttack:
            case Tutorial_ZahakBattle.State.SecondFight_WaitAgainForTroopFirstAttack:
            case Tutorial_ZahakBattle.State.SecondFight_WaitForFereydounFirstAttack:
            case Tutorial_ZahakBattle.State.SecondFight_ZahakSpecialAttack:
                _battleController.Resume();
                break;

            case Tutorial_ZahakBattle.State.SecondFight_TheEnd:
                if (_onFinish != null)
                    _onFinish();
                break;
        }
    }

}
