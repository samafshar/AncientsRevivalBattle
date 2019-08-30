using System;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;

public enum BattleLogicStates
{
    Start,
    WaitForPartiesReadyToStartFight,
    PartiesReadyToStartFight,
    WaitForPrefight,
    TurnLoop_Start,
    TurnLoop_WaitForAction,
    TurnLoop_CastingSecret,
    TurnLoop_DoingFightActions,
    TurnLoop_TurnFinished,
    TurnLoop_TryToFinishTurn,    
    TryToEndBattle,
}

public class BattleLogic
{
    //Private
    private int                     _currentCharIndex;
    private int                     _indexOfCurrentAction;
    private int                     _generatedApOnDeath;
    private int                     _generatedApDamageTaken;
    private int                     _numOfKilledCharacterInThisAct;
    private int                     _currentSelectedSpellIndex = -1;
    private int                     _currentSelectedSecretIndex = -1;
    private int                     _timerForStartBattle;
    private int                     _timerForShowingVictoryLose;
    private int                     _maxTimeForStartBattle = 2;
    private int                     _maxTimeForShowingVictoryLose = 3;
    private int                     _playerScore;
    private int                     _opponentScore;
    private long                    _waitingCharacterIDToSelect;
    private bool                    _prevBadConnection;
    private bool                    _currentBadConnection;
    private bool                    _firstPing;
    private bool                    _apGeneratedInThisTurn;
    private bool                    _isFirstTurn;
    private bool                    _battleEndCame;
    private bool                    _waitingToStartNewTurn;
    private float                   _battleTimer;
    private float                   _pingProblem;
    private float                   _timeOutTime;
    private float                   _clientCurrentTime;
    private Party[]                 _parties;
    private List<int>               _waitingEligibleSpellsForSelect;
    private List<int>               _waitingEligibleSecretsForSelect;
    private List<CoolDownData>      _waitingCooldownSpells;
    private List<CoolDownData>      _waitingCooldownSecrets;
    private Character               _currentSelectedTarget = null;
    private Character               _currentSelectedCharacter = null;
    private SpellInfo               _currentSpellInfo;
    private BattleInfo              _battleInfo;
    private BattleScene             _battleScene;
    private List<Spell>             _currentSpells;
    private List<Character>         _currentselectedCharacters;
    private BattleUI                _newBattleUI;
    private BattleResultData         _battleResultData;

    private BotLogic                _botLogic;
    private BattleSpellDataProvider _battleSpellDataProvider;

    private BattleLogicStates       _curState;    
    private List<ActionData>        _actionData;

    public ICharacterVisitor        characterVisitor { get; set; }

    //Public Methods
    public BattleLogic(BattleInfo battleInfo)
    {
        Init(battleInfo);
    }

    public void FakeServer_StartNextTurn(List<int> eligibleSpells)
    {
        DivineDebug.Log("Next Turn Started.");

        _curState = BattleLogicStates.TurnLoop_Start;
        _indexOfCurrentAction = 0;
        _actionData = new List<ActionData>();

        StartTimer();

        SelectNextCharacter(eligibleSpells);
    }
    public void FakeServer_StartNextTurn(long nextCharacterID, List<int> eligibleSpells, int actionPoint)
    {
        DivineDebug.Log("Next Turn Started.");

        for (int i = 0; i < _parties.Length; i++)
            if (_parties[i].side == PartySide.Player)
                _parties[i].SetActionPoint(_parties[i].actionPoint + actionPoint, true);

        _curState = BattleLogicStates.TurnLoop_Start;
        _indexOfCurrentAction = 0;
        _actionData = new List<ActionData>();

        StartTimer();

        List<int> eligibleSecrets = new List<int>() { };
        List<CoolDownData> cooldownSecrets = new List<CoolDownData>(2) { new CoolDownData(0, 0), new CoolDownData(1, 0) };

        List<CoolDownData> cooldownSpells = new List<CoolDownData>(eligibleSpells.Count);
        foreach (var item in eligibleSpells)
        {
            cooldownSpells.Add(new CoolDownData(item, 2));
        }

        SelectACharacter(GetCharacterWithID(nextCharacterID), eligibleSpells, new List<CoolDownData>() { }, eligibleSecrets, new List<CoolDownData>() { }, true);
    }

    public Character GetCharacterWithID(long id)
    {
        return FindCharacter(id);
    }

    public void SecretReveal(int index, PartySide side, Action OnRevealComplete)
    {
        int partyIndex = side == PartySide.Player ? 0 : 1;
        _newBattleUI.ShowSecretRevealUI(_parties[partyIndex].secrets[index], OnRevealComplete);        
    }

    public void SetPause(bool isPaused, bool justTime)
    {
        _newBattleUI.SetIsTimerPaused(isPaused);

        if (justTime)
            return;
    }

    public void ResetTimer()
    {
        _battleTimer = _battleInfo.turnTime;

        _newBattleUI.UpdateTimer(_battleTimer);
    }
    
    public Character GetRandomTarget()
    {
        if (!_parties[0].ContainCharacter(_currentSelectedCharacter))
            return _parties[0].GetRandomCharacter();
        else
            return _parties[1].GetRandomCharacter();
    }

    public long GetCurrentCharacterID()
    {        
        return _currentSelectedCharacter.id;
    }

    public Vector3 GetSpellIconPosition(int index)
    {
        return _newBattleUI.GetSpellIconPosition(index);
    }
    

    //Private Methods
    private void Init(BattleInfo battleInfo)
    {
        if (Application.isMobilePlatform)
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

        DivineDebug.Log("Initing battle Logic.");

        _curState       = BattleLogicStates.Start;
        _battleInfo     = battleInfo;
        _battleScene    = BattleSceneManager.instance.SelectBattleScene(_battleInfo.battleSceneType);
        _newBattleUI    = NewUIGroup.CreateGroup(NewUIGroup.NAME__BATTLEUI, NewUIGroup.PrefabProviderType.Battle) as BattleUI;

        _generatedApOnDeath     = AppManager.instance.config.apDeath;
        _generatedApDamageTaken = AppManager.instance.config.apTakenDamage;

        _isFirstTurn = true;
        _battleEndCame = false;

        _firstPing          = true;
        _pingProblem        = AppManager.instance.config.pingProblemNumber;
        _timeOutTime        = _pingProblem * 5;
        _clientCurrentTime  = Time.time;

        _currentselectedCharacters = new List<Character>();

        MakeParties(_battleInfo.partyInfoes);
        StartParties();

        List<Character> orderArray = new List<Character>(_battleInfo.charactersTurnOrder_id.Length);
        foreach (var item in _battleInfo.charactersTurnOrder_id)
        {
            var characterOrder = _parties[0].FindCharacter(item);

            if (characterOrder != null)
                orderArray.Add(characterOrder);
            else
                orderArray.Add(_parties[1].FindCharacter(item));
        }

        _newBattleUI.Init(_parties[0].Name, _parties[1].Name,
            _battleInfo.turnTime,
            battleInfo.partyInfoes[0].availableSecrets,
            OnSpellSelectedByCurrentChar,
            OnSecretSelected,
            orderArray);
        
        _newBattleUI.InitScore(0, 0);

        BattleDataProvider.instance.SetBattleLogic(this);

        BattleDataProvider.instance.Event_BattleFinish += OnBattleFinishedComeFromServer;
        BattleDataProvider.instance.Event_BattleTurnData += OnTurnChanged;
        BattleDataProvider.instance.Event_ActionReceived += OnActionReceived;
        BattleDataProvider.instance.Event_BattlePingTick += OnPinging;
        //BattleDataProvider.instance.Event_Secret += OnSecretCastSuccessful;

        if (!TutorialManager.instance.IsInBattleTutorial())
            TimeManager.instance.Event_OneSecTick += OnSecTick;
        
        _battleSpellDataProvider = BattleSpellDataProvider.instance;

        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, _parties[0].hero.moniker.ToString());

        if (_battleInfo.isBot)
            _botLogic = new BotLogic(orderArray, battleInfo.botPower);

        //Test
        FakeServer.instance.Event_ActionAnalyzed        += OnActionReceived;
        FakeServer.instance.Event_BattleFinished        += OnBattleFinishedComeFromServer;
    }

    private void TryToStartNextTurn()
    {
        if (_isFirstTurn)
        {
            _isFirstTurn = false;
            _newBattleUI.FirstTurnStarted();
        }

        _curState = BattleLogicStates.WaitForPrefight;

        _actionData = new List<ActionData>();

        BattleDataProvider.instance.Event_BattleTurnTick += OnFirstTimeTickCome;
    }

    private void StartNextTurn(long nextCharacterID, List<int> eligibleSpells, List<CoolDownData> cooldownSpells, List<int> eligibleSecrets, List<CoolDownData> cooldownSecrets)
    {
        DivineDebug.Log("Next Turn Started.");
        
        if (_actionData == null)
            _actionData = new List<ActionData>();
        _curState = BattleLogicStates.TurnLoop_Start;
        _indexOfCurrentAction = 0;
        _waitingToStartNewTurn = false;

        StartTimer();

        SelectACharacter(GetCharacterWithID(nextCharacterID), eligibleSpells, cooldownSpells, eligibleSecrets, cooldownSecrets, true);
    }

    private void MakeParties(PartyInfo[] partyInfoes)
    {
        DivineDebug.Log("Making Parties started.");

        int numberOfValidParties = 2;
        _parties = new Party[numberOfValidParties];

        if (partyInfoes.Length > 2)        
            DivineDebug.Log(String.Concat("party infoes from battle info is more than ", numberOfValidParties, "."), DivineLogType.Error);
        
        for (int i = 0; i < numberOfValidParties; i++)
        {
            if (i == 0)
                _parties[i] = new Party(partyInfoes[i], PartySide.Player, _battleScene.spawnPointsForLeftSide, _battleScene.selectedPoint_left, UpdateActioPoints);                
            else
                _parties[i] = new Party(partyInfoes[i], PartySide.Enemy, _battleScene.spawnPointsForRightSide, _battleScene.selectedPoint_right, UpdateActioPoints);
        }
        
        DivineDebug.Log("Making Parties Finished.");
    }

    private void UpdateActioPoints(int newActionPoint, bool isOnTurnChange)
    {
        _newBattleUI.UpdateLogicalActionPoint(newActionPoint , isOnTurnChange);
    }

    private void StartParties()
    {
        DivineDebug.Log("Starting Parties.");

        for (int i = 0; i < _parties.Length; i++)
        {
            if (i == 0)
                _parties[i].StartParty(_battleScene.startingPointsForLeftSide, _battleScene.formationPointsForLeftSide);
            else               
                _parties[i].StartParty(_battleScene.startingPointsForRightSide, _battleScene.formationPointsForRightSide);

            _parties[i].Event_PartyReady += OnPartyReady;
            _parties[i].Event_CharacterDied += OnCharacterDied;
            _parties[i].Event_ChakraAppeared += OnChakraAppeared;
            _parties[i].Event_CharacterClicked += OnCharacterClicked;
            _parties[i].Event_CharacterRecieveSpell += OnCharacterReceiveSpell;
        }

        _curState = BattleLogicStates.WaitForPartiesReadyToStartFight;
    }

    private void StartTimer()
    {
        _battleTimer = _battleInfo.turnTime;
        
        _newBattleUI.UpdateTimer(_battleTimer);
        
        BattleDataProvider.instance.Event_BattleTurnTick += OnBattleTimeTick;
        FakeServer.instance.Event_BattleTick += OnBattleTimeTick;
    }

    private void BattleTimeFinish_CountDownToZero()
    {
        BattleDataProvider.instance.Event_BattleTurnTick -= OnBattleTimeTick;
        FakeServer.instance.Event_BattleTick -= OnBattleTimeTick;
        
        TryToFinishTurn();        

        if (_currentSelectedSpellIndex > -1)
        {
            for (int i = 0; i < _parties.Length; i++)
            {
                _parties[i].Event_TargetSelected -= OnCharacterSelectAsTarget;
                _parties[i].ChangePartyTargetMode(false);
            }
        }
    }

    private void FinishBattleTime_Intentional()
    {
        _newBattleUI.Disappear();

        BattleDataProvider.instance.Event_BattleTurnTick -= OnBattleTimeTick;
        FakeServer.instance.Event_BattleTick -= OnBattleTimeTick;
    }

    private bool IsCurrentStepOfFightActionFinished()
    {
        for (int i = 0; i < _parties.Length; i++)
            if (!_parties[i].isReady)
                return false;
        
        if (_currentSpells.Count == 0)
        {
            if (_currentSelectedCharacter != null)
                _currentSelectedCharacter.Event_SpellCast -= OnCharacterCastSpell;
            return true;
        }
        else
            return false;
    }

    private void SelectNextCharacter(List<int> eligibleSpells)
    {
        //For fake server
        int count = 0;
        bool found = false;
        Character character = null;
        
        while (!found)
        {
            count++;

            if (count > _battleInfo.charactersTurnOrder_id.Length)            
                DivineDebug.Log("Battle logic cant find any character that is alive. So return null in SelectNextCharacter.", DivineLogType.Error);                
            
            if (_currentCharIndex >= _battleInfo.charactersTurnOrder_id.Length)
                _currentCharIndex = 0;

            character = FindCharacter(_battleInfo.charactersTurnOrder_id[_currentCharIndex]);

            _currentCharIndex++;

            if (!character.isDead)
                found = true;
        }

        List<int> eligibleSecrets = new List<int>() { 0, 1 };
        List<CoolDownData> cooldownSecrets = new List<CoolDownData>(2) { new CoolDownData(0, 0), new CoolDownData(1, 0) };

        List<CoolDownData> cooldownSpells = new List<CoolDownData>(eligibleSpells.Count);
        foreach (var item in eligibleSpells)
        {
            cooldownSpells.Add(new CoolDownData(item, 0)); 
        }

        SelectACharacter(character, eligibleSpells, cooldownSpells, eligibleSecrets, cooldownSecrets, true);
    }

    private void SelectACharacter(Character selectedChar, List<int> eligibleSpells, List<CoolDownData> cooldownSpells, List<int> eligibleSecrets,
                    List<CoolDownData> cooldownSecrets, bool isSelected)
    {
        if (selectedChar == null) //It means actions done without selecting character like Abbot
        {
            CheckStates();
            return;
        }

        selectedChar.SetSelected(isSelected);
        
        DivineDebug.Log("SelectACharacter " + selectedChar.moniker + "  " + isSelected);

        if (isSelected)
        {
            _currentSelectedCharacter = selectedChar;

            Party selectedParty = _parties[0].ContainCharacter(selectedChar) ? _parties[0] : _parties[1];
            
            _newBattleUI.SetCharacterSelected(selectedChar, eligibleSpells, cooldownSpells, eligibleSecrets, cooldownSecrets, 
                            _battleSpellDataProvider.GetBattleSpellDatas(selectedChar.charInfo, selectedParty.actionPoint));
        }
        else
        {
            _currentSelectedCharacter = null;
            
            _newBattleUI.SetTurnFinished();
        }                

        if (_currentSelectedCharacter == null) //Turn try to finished should check state again
            CheckStates();
    }

    private Character FindCharacter(long characterID)
    {
        Character findedChar = null;

        for (int i = 0; i < _parties.Length; i++)
        {
            findedChar = _parties[i].FindCharacter(characterID);

            if (findedChar != null)
                return findedChar;
        }

        DivineDebug.Log("No Character with id " + characterID + " found.", DivineLogType.Error);
        return null;
    }
    
    private void CheckFightActionList()
    {
        if (IsCurrentStepOfFightActionFinished())
        {
            if (_numOfKilledCharacterInThisAct > 1)
                _newBattleUI.ShowMultipleKillComboEffect(_numOfKilledCharacterInThisAct);

            if (!RunNextAction_IfWeHaveAny())
            {
                DivineDebug.Log("No Next Action To Run");
                TryToFinishTurn();
            }
        }
    }

    private bool RunNextAction_IfWeHaveAny()
    {
        if (_actionData.Count == 0)
        {
            DivineDebug.Log("No action data to run.");

            return false;
        }

        _numOfKilledCharacterInThisAct = 0;
        _apGeneratedInThisTurn = false;

        _curState = BattleLogicStates.TurnLoop_DoingFightActions;

        DivineDebug.Log("Run action number " + _indexOfCurrentAction + " .");

        ActionData ad = _actionData[0];
        if ((ad as FightActionData) != null)
        {
            Character.Deleg_SpellCasted onCharacterCastSpell = OnCharacterCastSpell;
            ad.RunAction(onCharacterCastSpell);            
        }
        else if((ad as SecretActionData) !=null)
        {
            ad.RunAction(null);
        }

        _actionData.Remove(ad);
        _indexOfCurrentAction++;

        return true;
    }

    private void StartWaitingForActionReceive()
    {
        _curState = BattleLogicStates.TurnLoop_WaitForAction;

        if (_actionData != null && _actionData.Count > 0)
            StartFightActions();

        if (_battleInfo.isBot && _currentSelectedCharacter.side == PartySide.Enemy)
            _botLogic.RunLogic(_currentSelectedCharacter, _parties, Net_SendAction);

        //<Test>
        if (FakeServer.instance.isFake)
            Net_ImReady();
        //</Test>
    }

    private void StartFightActions()
    {
        if (_curState != BattleLogicStates.WaitForPrefight)
            FinishBattleTime_Intentional();

        RunNextAction_IfWeHaveAny();
    }

    private void TryToFinishTurn()
    {
        DivineDebug.Log("Trying To Finish Turn");

        _curState = BattleLogicStates.TurnLoop_TryToFinishTurn;               

        SelectACharacter(_currentSelectedCharacter, null, null, null, null, false);
    }

    private void FinishTurn()
    {
        _curState = BattleLogicStates.TurnLoop_TurnFinished;

        _currentSelectedCharacter = null;

        _indexOfCurrentAction = 0;
        _actionData = null;

        if (_waitingToStartNewTurn) //For prefight actions, need to say ready to server too
        {
            TryToStartNextTurn();
            return;
        }

        if (_battleEndCame)
            OnBattleFinished();
        else
            Net_ImReady();
    }

    private void TryToEndBattle()
    {
        GameAnalytics.NewProgressionEvent(
            _battleResultData.isVictorious == true ? GameAnalyticsSDK.GAProgressionStatus.Complete : GameAnalyticsSDK.GAProgressionStatus.Fail,
            _parties[0].hero.moniker.ToString());

        //UnRegister from all events
        for (int i = 0; i < _parties.Length; i++)
        {
            _parties[i].Event_PartyReady -= OnPartyReady;
            _parties[i].Event_CharacterDied -= OnCharacterDied;
            _parties[i].Event_ChakraAppeared -= OnChakraAppeared;

            _parties[i].EndingBattle(i == 0 ? _battleResultData.isVictorious : !_battleResultData.isVictorious);

            _parties[i].Event_CharacterClicked -= OnCharacterClicked;
            _parties[i].Event_CharacterRecieveSpell -= OnCharacterReceiveSpell;
        }

        if (!FakeServer.instance.isFake)
        {
            GameAnalytics.NewDesignEvent("Battle Finished");
            NewNetworkManager.instance.BattleFinished();
        }
        
        if(BattleUI.instance != null)
            BattleUI.instance.SetBattleFinished();

        WaitToShowVictoryLose();
    }

    private void WaitToStartBattle()
    {
        _timerForStartBattle = _maxTimeForStartBattle;
        TimeManager.instance.Event_OneSecTick += OneSecTick_StartBattle;

        if (!FakeServer.instance.isFake)
        {
            _newBattleUI.BattleStarted();

            AudioManager.instance.PlayMusic();
        }
    }

    private void OneSecTick_StartBattle(float actualTimeInterval)
    {
        _timerForStartBattle -= TimeManager.ONE_SECOND_TICK;

        if (_timerForStartBattle <= 0)
        {
            TimeManager.instance.Event_OneSecTick -= OneSecTick_StartBattle;

            Net_ImReady();
        }
    }

    private void WaitToShowVictoryLose()
    {
        _timerForShowingVictoryLose = _maxTimeForShowingVictoryLose;
        TimeManager.instance.Event_OneSecTick += OneSecTick_ShowingVictoryLose;
    }

    private void OneSecTick_ShowingVictoryLose(float actualTimeInterval)
    {
        _timerForShowingVictoryLose -= TimeManager.ONE_SECOND_TICK;

        if (_timerForShowingVictoryLose <= 0)
        {
            TimeManager.instance.Event_OneSecTick -= OneSecTick_ShowingVictoryLose;

            ShowVictoryLose();
        }
    }

    private void ShowVictoryLose()
    {
        VicDefView.Type result   = _battleResultData.isVictorious ? VicDefView.Type.Victory : VicDefView.Type.Defeat;
        BattleResult matchRes    = (BattleResult)NewUIGroup.CreateGroup(NewUIGroup.NAME__BATTLERESULT, NewUIGroup.PrefabProviderType.Battle);

        matchRes.Init(result, _battleResultData, CleaningBattleAndDestroying);
    }

    private void CleaningBattleAndDestroying(bool shouldReset)
    {
        DivineDebug.Log("Deleting the battle logic.");

        for (int i = 0; i < _parties.Length; i++)
            _parties[i].DestroyIt();
        
        _newBattleUI.StartDestroying();        
        
        BattleDataProvider.instance.Event_BattleFinish    -= OnBattleFinishedComeFromServer;
        BattleDataProvider.instance.Event_BattleTurnTick  -= OnBattleTimeTick;
        BattleDataProvider.instance.Event_ActionReceived  -= OnActionReceived;
        BattleDataProvider.instance.Event_BattleTurnData  -= OnTurnChanged;
        BattleDataProvider.instance.Event_BattlePingTick  -= OnPinging;
        //BattleDataProvider.instance.SecretInfoReceived -= OnSecretCastSuccessful;

        TimeManager.instance.Event_OneSecTick             -= OnSecTick;

        FakeServer.instance.Event_BattleTick              -= OnBattleTimeTick; 
        FakeServer.instance.Event_ActionAnalyzed          -= OnActionReceived;
        FakeServer.instance.Event_BattleFinished          -= OnBattleFinishedComeFromServer;     

        AudioManager.instance.StopMusic();

        Grave[] remainingGraves = GameObject.FindObjectsOfType<Grave>();
        for (int i = 0; i < remainingGraves.Length; i++)
            GameObject.Destroy(remainingGraves[i].gameObject);

        for (int i = 0; i < _parties.Length; i++)
            _parties[i] = null;

        _parties        = null;
        _newBattleUI    = null;
        _currentSelectedCharacter = null;        
    }

    private void CheckStates()
    {
        DivineDebug.Log("State checked in this state: " + _curState);

        if (_curState == BattleLogicStates.WaitForPartiesReadyToStartFight)
        {
            _curState = BattleLogicStates.PartiesReadyToStartFight;

            WaitToStartBattle();
        }
        else if (_curState == BattleLogicStates.TurnLoop_DoingFightActions)
            CheckFightActionList();
        else if (_curState == BattleLogicStates.TryToEndBattle)
            TryToEndBattle();
        else if (_curState == BattleLogicStates.TurnLoop_Start)
            StartWaitingForActionReceive();
        else if (_curState == BattleLogicStates.TurnLoop_TryToFinishTurn && _currentSelectedCharacter == null)
            FinishTurn();
        else if (_curState == BattleLogicStates.WaitForPrefight && _battleEndCame)
            OnBattleFinished();
    }

    private void AddKillingScore(bool isKillerPlayer, bool isSlainedAHero, Vector3 sourcePos)
    {
        int val = isSlainedAHero ? AppManager.instance.config.heroScoreDeath : AppManager.instance.config.troopScoreDeath;
        
        if (isKillerPlayer)
            _playerScore += val;
        else
            _opponentScore += val;
        
        BattleUI.instance.AddScore(val, isKillerPlayer ? _playerScore : _opponentScore, isKillerPlayer, isSlainedAHero, sourcePos);
    }


    //Handle Events
    private void OnPartyReady(Party owner)
    {
        for (int i = 0; i < _parties.Length; i++)        
            if (!_parties[i].isReady)
                return;

        DivineDebug.Log("Parties Ready in this state: " + _curState.ToString() + " .");           

        CheckStates();
    }

    private void OnCharacterDied(Character deadCharacter, Party ownerParty)
    {
        DivineDebug.Log("This character: " + deadCharacter + " Just Died.");

        _numOfKilledCharacterInThisAct++;

        _newBattleUI.OnCharacterDie(deadCharacter);

        if (deadCharacter.side == PartySide.Player)
        {
            _newBattleUI.CreateAPGenEffects(deadCharacter.position_pivot, _generatedApOnDeath);
        }

        AddKillingScore(deadCharacter.side != PartySide.Player,
                            deadCharacter.isHero || deadCharacter.isChakra, deadCharacter.position_forHit);
    }

    private void OnChakraAppeared(Party ownerParty)
    {
        _newBattleUI.OnChakraAppeared(ownerParty.hero, ownerParty.chakra);
    }

    private void OnSecretSelected(int secretIndex)
    {
        _currentSelectedSecretIndex = secretIndex;

        for (int i = 0; i < _parties.Length; i++)
        {
            _parties[i].Event_TargetSelected -= OnCharacterSelectAsSecretTarget;
            _parties[i].ChangePartyTargetMode(false);
        }
        
        int partyIndex = _currentSelectedCharacter.side == PartySide.Player ? 0 : 1;

        SpellTargetType tarType = AppearanceConfigData.instance.GetSecretWithIndex(_parties[partyIndex].secrets[secretIndex])._targetType;

        if (tarType == SpellTargetType.Hero)
        {
            _parties[0].Event_TargetSelected += OnCharacterSelectAsSecretTarget;
            _parties[0].ChangePartyTargetMode(true, true, new List<Character> { _parties[0].hero, _parties[0].chakra });
        }
    }
    
    private void OnSpellSelectedByCurrentChar(Character character, int spellIndex)
    {
        for (int i = 0; i < _parties.Length; i++)
        {
            _parties[i].Event_TargetSelected -= OnCharacterSelectAsTarget;
            _parties[i].ChangePartyTargetMode(false);
        }
        
        _currentSelectedSpellIndex = spellIndex;

        SpellTargetType tarType = AppearanceConfigData.instance.
                                        GetMagicWithIndex(character.moniker, spellIndex)._targetType;
        bool isSplash = AppearanceConfigData.instance.
                                        GetMagicWithIndex(character.moniker, spellIndex)._isSplash;
        
        bool ignoreTaunt = (tarType == SpellTargetType.Ally) || isSplash;

        switch (tarType)
        {
            case SpellTargetType.Ally:
                _parties[0].Event_TargetSelected += OnCharacterSelectAsTarget;
                _parties[0].ChangePartyTargetMode(true, ignoreTaunt);
                break;
            case SpellTargetType.Enemy:
                _parties[1].Event_TargetSelected += OnCharacterSelectAsTarget;
                _parties[1].ChangePartyTargetMode(true, ignoreTaunt);
                break;
            case SpellTargetType.All:
                _parties[0].Event_TargetSelected += OnCharacterSelectAsTarget;
                _parties[0].ChangePartyTargetMode(true, true);
                _parties[1].Event_TargetSelected += OnCharacterSelectAsTarget;
                _parties[1].ChangePartyTargetMode(true, ignoreTaunt);
                break;
            case SpellTargetType.Self:
                _parties[0].Event_TargetSelected += OnCharacterSelectAsTarget;
                _parties[0].ChangePartyTargetMode(true, ignoreTaunt, new List<Character> { character });
                break;
            default:
                break;
        }

        DivineDebug.Log("OnSpellSelectedByCurrentChar, targetType: " + tarType.ToString());

        DivineDebug.Log("OnSpellSelectedByCurrentChar, char: " + character.moniker);

        if (characterVisitor != null)
            characterVisitor.OnCharacterSpellSelected(character, spellIndex);
    }

    private void OnCharacterSelectAsTarget(Character character)
    {
        _currentSelectedTarget = character;

        _currentSelectedCharacter.SpellSelectionDone();
        
        _newBattleUI.SetTurnFinished();
        
        for (int i = 0; i < _parties.Length; i++)
        {
            _parties[i].Event_TargetSelected -= OnCharacterSelectAsTarget;
            _parties[i].ChangePartyTargetMode(false);
        }

        DivineDebug.Log("BattleLogic, char: " + character.moniker);

        SkillShotType skillShotType = AppearanceConfigData.instance.GetMagicWithIndex(_currentSelectedCharacter.moniker,
                                                                                _currentSelectedSpellIndex)._skillShotType;

        DivineDebug.Log("OnCharacterSelectAsTarget ----------> skillShotType: " + skillShotType.ToString());

        if (skillShotType == SkillShotType.None)
            Net_SendAction(character.id, _currentSelectedSpellIndex);
        //else
        //    ; //Skill Shot???
        //(UIGroup.CreateGroup("Skillshot_" + skillShotType.ToString()) as ISkillshot).Init(OnSkillShotFinished);
    }

    private void OnCharacterSelectAsSecretTarget(Character character)
    {
        for (int i = 0; i < _parties.Length; i++)
        {
            _parties[i].Event_TargetSelected -= OnCharacterSelectAsSecretTarget;
            _parties[i].ChangePartyTargetMode(false);
        }

        //Net API
        Net_SendSecret(_currentSelectedSecretIndex, new List<long>() { character.id });
    }

    private void OnSkillShotFinished(int value)
    {
        Net_SendAction(_currentSelectedTarget.id, _currentSelectedSpellIndex, value);
    }

    private void OnBattleFinishedComeFromServer(BattleResultData battleResultData, List<TroopCoolDownData> troopCoolDowns,
                                    bool connectionLost)
    {
        TimeManager.instance.Event_OneSecTick -= OnSecTick;
        BattleDataProvider.instance.Event_BattlePingTick -= OnPinging;

        DivineDebug.Log("Battle Finish Come From Server", DivineLogType.Normal);
                
        _battleResultData = battleResultData;

        GameAnalytics.NewDesignEvent(
            "Battle Result " + (_battleInfo.isBot ? "With" : "Without") + "Bot: " +
            (_battleResultData.isVictorious ? "Win" : "Lose"));
        
        _battleEndCame = true;

        PlayerInfo player = GameManager.instance.player;
        for (int i = 0; i < troopCoolDowns.Count; i++)
            player.FindTroop(troopCoolDowns[i].id).SetRemainingCooldownTime(troopCoolDowns[i].remainingSeconds);                

        if (connectionLost)
            OnBattleFinished();

        if (FakeServer.instance.isFake)
            OnBattleFinished();                
    }

    private void OnBattleFinished()
    {
        _curState = BattleLogicStates.TryToEndBattle;

        CheckStates();
    }

    private void OnActionReceived(List<ActionData> actionList)
    {
        DivineDebug.Log("Action come from fight data analyzer. In This State: " + _curState.ToString());
        DivineDebug.Log("Fight Action Datas: Action Count: " + actionList.Count);
        
        for (int i = 0; i < actionList.Count; i++)
        {
            actionList[i].Init(this);
            _actionData.Add(actionList[i]);
        }

        if (_curState != BattleLogicStates.TurnLoop_WaitForAction && _curState != BattleLogicStates.WaitForPrefight)
        {
            DivineDebug.Log("Action wont run because we are in this state now: " + _curState.ToString());
            return;
        }

        if (_curState == BattleLogicStates.WaitForPrefight)
        {
            BattleDataProvider.instance.Event_BattleTurnTick -= OnFirstTimeTickCome;            
            _waitingToStartNewTurn = true;
        }
        
        StartFightActions();      
    }

    private void OnTurnChanged(TurnData turnData)
    {
        DivineDebug.Log("Turn Changed Num of update Stats: " + turnData.updatedStats.Count +
                            " Char: " + GetCharacterWithID(turnData.turnId).moniker, DivineLogType.Warn);
        DivineDebug.Log("Eligible Spells: " + turnData.eligibleSpells.Count);

        for (int i = 0; i < _parties.Length; i++)
        {
            if (_parties[i].side == PartySide.Player)
                _parties[i].SetActionPoint(turnData.allyReceivedActionPoints, true);
            else
                _parties[i].SetActionPoint(turnData.enemyReceivedActionPoints, true);
        }

        for (int i = 0; i < turnData.updatedStats.Count; i++)
        {
            DivineDebug.Log("UpdateStat: Char: " + FindCharacter(turnData.updatedStats[i].ownerID) +
                            " FinalHP: " + turnData.updatedStats[i].finalStats.hp +
                            "Shield: " + turnData.updatedStats[i].finalStats.shield +
                            "flag: " + turnData.updatedStats[i].finalStats.flags.ToString());

            for (int j = 0; j < turnData.updatedStats[i].singleStatChanges.Count; j++)
                DivineDebug.Log("singleStatChange: type: " +
                    turnData.updatedStats[i].singleStatChanges[j].charStatChangeType.ToString() + " intval: "
                    + turnData.updatedStats[i].singleStatChanges[j].intVal);

            if (_parties[0].ContainCharacter(turnData.updatedStats[i].ownerID))
                _parties[0].UpdateCharacterStats(turnData.updatedStats[i]);
            else
                _parties[1].UpdateCharacterStats(turnData.updatedStats[i]);
        }

        _waitingCharacterIDToSelect         = turnData.turnId;
        _waitingEligibleSpellsForSelect     = turnData.eligibleSpells;
        _waitingEligibleSecretsForSelect    = turnData.eligibleSecrets;

        _waitingCooldownSpells              = turnData.coolDownSpells;
        _waitingCooldownSecrets             = turnData.coolDownSecrets;

        TryToStartNextTurn();
    }

    private void OnFirstTimeTickCome(int elapsedSeconds)
    {
        BattleDataProvider.instance.Event_BattleTurnTick -= OnFirstTimeTickCome;

        StartNextTurn(_waitingCharacterIDToSelect, _waitingEligibleSpellsForSelect, _waitingCooldownSpells, _waitingEligibleSecretsForSelect, _waitingCooldownSecrets);     
    }

    private void OnBattleTimeTick(int elapsedSeconds)
    {
        _battleTimer = _battleInfo.turnTime - elapsedSeconds;
        
        _newBattleUI.UpdateTimer(_battleTimer);

        if (_battleTimer <= 0)
            BattleTimeFinish_CountDownToZero();
    }

    private void OnCharacterCastSpell(Spell spell)
    {
        spell.Event_SpellWillDestroy += OnSpellDestroyed;
        if (_currentSpells == null)
            _currentSpells = new List<Spell>();

        _currentSpells.Add(spell);
    }

    private void OnSpellDestroyed(Spell spell)
    {
        spell.Event_SpellWillDestroy -= OnSpellDestroyed;
        _currentSpells.Remove(spell);

        CheckFightActionList();
    }

    private void OnSecretCastSuccessful(int enabledSecret)
    {
        DivineDebug.Log("Secret Cast Successful with this ID: " + enabledSecret);

        _curState = BattleLogicStates.TurnLoop_CastingSecret;

        bool isForMe = _currentSelectedCharacter.side == PartySide.Player;
        int partyIndex = isForMe ? 0 : 1;
        _newBattleUI.ShowSecretCastUI(_parties[partyIndex].secrets[enabledSecret], isForMe, OnSecretCastAnimComplete);
    }

    private void OnSecretCastAnimComplete()
    {
        StartWaitingForActionReceive();
    }

    private void OnCharacterClicked(Character character)
    {
        if (characterVisitor != null)
            characterVisitor.OnCharacterClicked(character);
    }

    private void OnCharacterReceiveSpell(SpellInfo spellInfo, int effectIndex, Character reciever)
    {
        if ((spellInfo.spellEffectInfos[effectIndex].effectOnCharacter == SpellEffectOnChar.NormalDamage ||
            spellInfo.spellEffectInfos[effectIndex].effectOnCharacter == SpellEffectOnChar.SeriousDamage) &&
            reciever.side == PartySide.Player &&
            _currentSelectedCharacter.side == PartySide.Enemy &&
            !_apGeneratedInThisTurn)
            _newBattleUI.CreateAPGenEffects(reciever.position_pivot, _generatedApDamageTaken);

        _apGeneratedInThisTurn = true;
    }

    private void OnPinging()
    {
        if(_firstPing)
        {
            _firstPing = false;

            _prevBadConnection = false;
            _currentBadConnection = false;

            _clientCurrentTime = Time.time;
        }
        else
        {
            _currentBadConnection = (Time.time - _clientCurrentTime) > _pingProblem;
            
            if (_currentBadConnection && !_prevBadConnection)
                _newBattleUI.BadConnection( ConnectionProblemType.SlowConnection, true);

            if (_prevBadConnection && !_currentBadConnection)
                _newBattleUI.BadConnection(ConnectionProblemType.SlowConnection, false);

            _clientCurrentTime = Time.time;
            _prevBadConnection = _currentBadConnection;
        }
    }

    private void OnSecTick(float actualTimeInterval)
    {
        if (Time.time - _clientCurrentTime > _timeOutTime)
        {
            _newBattleUI.BadConnection(ConnectionProblemType.DC, false);

            //End Battle By Force
        }
    }

    //Net Methods
    private void Net_ImReady()
    {
        DivineDebug.Log("Sending Ready in this state: " + _curState);

        if (FakeServer.instance.isFake)
            FakeServer.instance.ImReady(this, _curState);
        else
            NewNetworkManager.instance.Ready(_battleInfo.isBot ? 1f : 0f);        
    }
    private void Net_SendAction(long id, int currentSelectedSpellIndex)
    {
        DivineDebug.Log("Sending Action Data-> Selected spell index: " + currentSelectedSpellIndex +
            " target: " + GetCharacterWithID(id).moniker + " ID: " + id);

        if (FakeServer.instance.isFake)
            FakeServer.instance.Move(_currentSelectedCharacter.id, currentSelectedSpellIndex, new List<long> { id });
        else
            NewNetworkManager.instance.Move(currentSelectedSpellIndex, id);
    }
    private void Net_SendAction(long id, int currentSelectedSpellIndex , int skillShotValue)
    {
        List<long> targets = new List<long>();
        targets.Add(id);

        DivineDebug.Log("Sending Action Data With Skill Shot-> Selected spell index: " + currentSelectedSpellIndex +
            " target: " + GetCharacterWithID(id).moniker + " ID: " + id + " Skillshot Value: " + skillShotValue);

        NetworkManager.instance.Move(_currentSelectedCharacter.id, currentSelectedSpellIndex, skillShotValue, targets);
    }
    private void Net_SendSecret(int secretID, List<long> targets)
    {        
        NetworkManager.instance.Secret(_currentSelectedCharacter.id, secretID, targets);
    }
}
