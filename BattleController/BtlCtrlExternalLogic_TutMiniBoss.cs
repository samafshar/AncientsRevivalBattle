using System;
using UnityEngine;
using System.Collections.Generic;

public class BtlCtrlExternalLogic_TutMiniBoss : IBtlCtrlExternalLogic
{
    private Action                  _onFinish;
    private BattleController        _battleController;
    private Tutorial_EntranceBattle _tutorialCtrl;

    private Tutorial_EntranceBattle tutorialCtrl
    {
        get
        {
            if (_tutorialCtrl == null)
                _tutorialCtrl = (Tutorial_EntranceBattle)TutorialManager.instance.GetTutorial();
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
            if (tutorialCtrl.GetTutorialState() == Tutorial_EntranceBattle.State.FirstFight_Fereydoun_WaitTillReavilling)
                onSecret(currentSlcCharacter.id, Divine.Secret.Revival, new List<long> { 1, 2 });

            return;
        }

        DivineDebug.Log("Make Decision, ExternalLogic: " + tutorialCtrl.GetTutorialState().ToString());

        switch (tutorialCtrl.GetTutorialState())
        {            
            case Tutorial_EntranceBattle.State.FirstFight_SkullWizardFight_Creating:
            case Tutorial_EntranceBattle.State.FirstFight_SkullWizard_Brag:
            case Tutorial_EntranceBattle.State.FirstFight_FereydounEasyyy:
            case Tutorial_EntranceBattle.State.FirstFight_SkullWDialogBefore3Skulls:
            case Tutorial_EntranceBattle.State.FirstFight_FeriDialogBeforeAllies:
                _battleController.Pause();
                break;

            case Tutorial_EntranceBattle.State.FirstFight_SkullWizard_WaitTillReavillingSkull:
                onSecret(currentSlcCharacter.id, Divine.Secret.Revival, new List<long> { 5 });
                break;

            case Tutorial_EntranceBattle.State.FirstFight_SkullWFirstAttack:
                NormalAttack(currentSlcCharacter, parties, Divine.Moniker.Fereydoun, onMove);
                break;

            case Tutorial_EntranceBattle.State.FirstFight_WaitForSkullWSecondAttack:
                NormalAttack(currentSlcCharacter, parties, Divine.Moniker.Fereydoun, onMove);
                break;           

            case Tutorial_EntranceBattle.State.FirstFight_SkullWizard_WaitTillReavillingSkull2:
                onSecret(currentSlcCharacter.id, Divine.Secret.Revival, new List<long> { 6, 7, 8 });
                break;

            case Tutorial_EntranceBattle.State.FirstFight_WaitForSkull1_1stGroupAttack:
                NormalAttack(currentSlcCharacter, parties, Divine.Moniker.Archer, onMove);
                break;

            case Tutorial_EntranceBattle.State.FirstFight_WaitForSkull2_1stGroupAttack:
                NormalAttack(currentSlcCharacter, parties, Divine.Moniker.Hanzo, onMove);
                break;

            case Tutorial_EntranceBattle.State.FirstFight_WaitForSkull3_1stGroupAttack:
                NormalAttack(currentSlcCharacter, parties, Divine.Moniker.Archer, onMove);
                break;

            case Tutorial_EntranceBattle.State.FirstFight_WaitForSkullW_1stGroupAttack:
                NormalAttack(currentSlcCharacter, parties, Divine.Moniker.Hanzo, onMove);
                break;

            case Tutorial_EntranceBattle.State.FirstFight_WaitForSkull1_2ndGroupAttack:
                NormalAttack(currentSlcCharacter, parties, Divine.Moniker.Archer, onMove);
                break;

            case Tutorial_EntranceBattle.State.FirstFight_WaitForSkull2_2ndGroupAttack:
                NormalAttack(currentSlcCharacter, parties, Divine.Moniker.Hanzo, onMove);
                break;

            case Tutorial_EntranceBattle.State.FirstFight_WaitForSkull3_2ndGroupAttack:
                NormalAttack(currentSlcCharacter, parties, Divine.Moniker.Archer, onMove);
                break;

            case Tutorial_EntranceBattle.State.FirstFight_WaitForSkullW_2ndGroupAttack:
                NormalAttack(currentSlcCharacter, parties, Divine.Moniker.Hanzo, onMove);
                break;

            case Tutorial_EntranceBattle.State.FirstFight_ShieldIntro:
                _battleController.Pause();
                break;

            case Tutorial_EntranceBattle.State.FirstFight_WaitTillLittleShiledAmount:
                if ((Math.Abs(currentSlcCharacter.spells[0].damage)) >= parties[0].hero.shield)
                {
                    _battleController.Pause();
                    _tutorialCtrl.ShieldWillCrushed();
                }
                else
                    NormalAttack(currentSlcCharacter, parties, Divine.Moniker.Fereydoun, onMove);
                break;

            case Tutorial_EntranceBattle.State.FirstFight_DoDamageToBecomeChakra:                
                NormalAttack(currentSlcCharacter, parties, Divine.Moniker.Fereydoun, onMove);
                break;

            case Tutorial_EntranceBattle.State.FirstFight_WaitTillChakraTurn:
                NormalAttack(currentSlcCharacter, parties, Divine.Moniker.FereydounChakra, onMove);
                break;
        }
    }

    public void OnTick(int elapsedTime) { }

    public void FightFinished(Action onFightFinished)
    {
        _onFinish = onFightFinished;

        _tutorialCtrl.BattleFinished();
    }

    public void MoveDone(BtlCtrlCharacter btlCtrlCharacter)
    {
        
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

        for (int i=0;i<chs.Count;i++)
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

        if (target == null)
            target = parties[0].GetAliveTroop();

        if (target == null)
            target = parties[0].hero;

        if (onMove != null)
            onMove(currentSlcCharacter.id, indexOfSpell, new List<long> { target.id });
    }
        

    //Handlers
    private void OnTutorialStateChanged(Tutorial_EntranceBattle.State newState)
    {
        switch (newState)
        {
            case Tutorial_EntranceBattle.State.FirstFight_SkullWizard_WaitTillReavillingSkull:
            case Tutorial_EntranceBattle.State.FirstFight_FereydounFirstAttack:
            case Tutorial_EntranceBattle.State.FirstFight_SkullWFirstAttack:
            case Tutorial_EntranceBattle.State.FirstFight_SkullWizard_WaitTillReavillingSkull2:
            case Tutorial_EntranceBattle.State.FirstFight_Fereydoun_WaitTillReavilling:
            case Tutorial_EntranceBattle.State.FirstFight_WaitTillLittleShiledAmount:
            case Tutorial_EntranceBattle.State.FirstFight_DoDamageToBecomeChakra:
                _battleController.Resume();
                break;
            case Tutorial_EntranceBattle.State.FirstFight_ShowingEnemy:
                _battleController.Pause();
                break;

            case Tutorial_EntranceBattle.State.FirstFight_TheEnd:
                if (_onFinish != null)
                    _onFinish();
                break;
        }
    }
}