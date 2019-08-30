using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IBtlCtrlExternalLogic
{
    void Init(BattleController battleController);
    void MakeDecision(BtlCtrlCharacter _currentSlcCharacter,
                        BtlCtrlParty[] parties,
                        Action<long, int, List<long>> onMove,
                        Action<long, Divine.Secret, List<long>> onSecret);
    void OnTick(int elapsedTime);
    void FightFinished(Action onFightFinished);
    void MoveDone(BtlCtrlCharacter btlCtrlCharacter);
    void DestroyIt();
}


public class BattleController
{
    public delegate void Deleg_NextTurn(BtlCtrlCharacter character);
    public event Deleg_NextTurn Event_OnNextTurnStarted;

    public enum TeamTypes
    {
        Player,
        Enemy,
    }

    private enum States
    {
        WaitToStartFight,
        TurnLoop_WaitTillNextCharSelected,
        TurnLoop_WaitToMoveCompleted,
        TurnLoop_WaitToTurnFinish,
    }    

    private int                     _timer;
    private int                     _turnTime;
    private int                     _currentTurnIndex;
    private int                     _currentGeneratedAP;
    private bool                    _battleShouldFinished;
    private bool                    _isPause;
    private States                  _currentState;
    private List<long>               _turnOrders;
    private FakeServer              _fakeServer;
    private BattleLogic             _battleLogic;
    private BtlCtrlParty[]          _parties; //0 Player 1 AI
    private BattleSceneType         _sceneType;
    private BtlCtrlCharacter        _currentSlcCharacter;
    private List<ActionData>        _waitedActionList;
    private Stack<ActionData>       _currentActionsStack;
    private List<BtlCtrlSpell>      _currentActiveSpells;
    private BattleControllerType    _type;
    private IBtlCtrlExternalLogic   _externalLogic;
    private BtlCtrlSecretController _secretController;

    public int turnTime                 { get { return _turnTime; } }
    public List<long> turnOrders         { get { return _turnOrders; } }
    public BattleLogic battleLogic      { get { return _battleLogic; } }
    public BtlCtrlParty[] parties       { get { return _parties; } }
    public BattleSceneType sceneType    { get { return _sceneType; } }


    //Public Methods
    public void Init(List<BtlCtrlCharacter> playerCharacters, List<BtlCtrlCharacter> aiCharacters, string playerName, string aiName,
                        List<long> turns, BattleControllerType type, int turnTime = 0, BattleSceneType sceneType = BattleSceneType.Tutorial_Hell)
    {
        _parties = new BtlCtrlParty[2];

        _parties[0] = new BtlCtrlParty();
        _parties[0].Init(playerCharacters, TeamTypes.Player, playerName);

        _parties[1] = new BtlCtrlParty();
        _parties[1].Init(aiCharacters, TeamTypes.Enemy, aiName);

        ListenToPartiesAndCharactersEvents();

        _turnOrders = turns;
        _turnTime = turnTime;

        _type = type;

        _sceneType = sceneType;

        _currentState = States.WaitToStartFight;

        _currentTurnIndex = -1;

        _currentActionsStack = new Stack<ActionData>();
        _currentActiveSpells = new List<BtlCtrlSpell>();

        _externalLogic = null;
        SetExternalLogic();

        _secretController = new BtlCtrlSecretController();
        _secretController.Event_ActionGenerated += OnActionGenerated;

        _fakeServer = FakeServer.instance;

        _fakeServer.Event_OnMove += OnMove;
        _fakeServer.Event_OnReadyCome += OnReadyCome;
    }

    public void SetBattleLogic(BattleLogic battleLogic)
    {
        _battleLogic = battleLogic;
    }

    public Character GetBattleHero(TeamTypes side)
    {
        int sideIndex = side == TeamTypes.Player ? 0 : 1;

        return battleLogic.GetCharacterWithID(parties[sideIndex].hero.id);
    }
    public Character GetBattleCharacter(TeamTypes teamType, int index)
    {
        int partyIndex = teamType == TeamTypes.Player ? 0 : 1;

        return _battleLogic.GetCharacterWithID((_parties[partyIndex].GetCharacters()[index].id));
    }

    public Character GetBattleCharacter(int id)
    {
        return _battleLogic.GetCharacterWithID(id);
    }

    public void Pause()
    {
        _isPause = true;

        _battleLogic.SetPause(true, true);
    }

    public void Resume()
    {
        _isPause = false;

        _battleLogic.SetPause(false, true);

        if (_waitedActionList != null)
            SendActions(_waitedActionList);
        else
            _externalLogic.MakeDecision(_currentSlcCharacter, parties, OnMove, OnSecret);
    }

    public void ResetTimer()
    {
        _timer = turnTime;
        _battleLogic.ResetTimer();
    }

    public void DestoryBattle()
    {
        UnlistenToPartiesAndCharactersEvents();

        _fakeServer.Event_OnMove -= OnMove;
        _fakeServer.Event_OnReadyCome -= OnReadyCome;

        _secretController.Event_ActionGenerated -= OnActionGenerated;

        TimeManager.instance.Event_OneSecTick -= OnTick;

        for (int i = 0; i < _parties.Length; i++)
            _parties[i].DestroyIt();

        _fakeServer = null;
        _turnOrders = null;
        _battleLogic = null;
        _currentActiveSpells = null;
        _currentSlcCharacter = null;
        _currentActionsStack = null;

        _externalLogic.DestroyIt();
        _externalLogic = null;

        _parties[0] = null;
        _parties[1] = null;
        _parties = null;
    }

    //Private Methods
    private void ListenToPartiesAndCharactersEvents()
    {
        for (int i = 0; i < _parties.Length; i++)
        {
            List<BtlCtrlCharacter> chars = _parties[i].GetCharacters();

            for (int j = 0; j < chars.Count; j++)
            {
                chars[j].Event_SpellFinished    += OnSpellFinished;
                chars[j].Event_ActionGenerated  += OnActionGenerated;
            }
        }

        for (int i = 0; i < _parties.Length; i++)        
            _parties[i].Event_APChanged += OnActionPointChanged;        
    }

    private void UnlistenToPartiesAndCharactersEvents()
    {
        for (int i = 0; i < _parties.Length; i++)
        {
            List<BtlCtrlCharacter> chars = _parties[i].GetCharacters();

            for (int j = 0; j < chars.Count; j++)
            {
                chars[j].Event_SpellFinished -= OnSpellFinished;
                chars[j].Event_ActionGenerated -= OnActionGenerated;
            }
        }

        for (int i = 0; i < _parties.Length; i++)
            _parties[i].Event_APChanged -= OnActionPointChanged;
    }

    private void StartNextTurn(int nextCharacterID, bool justStarted)
    {
        _currentState = States.TurnLoop_WaitTillNextCharSelected;

        _currentActionsStack.Clear();

        _currentSlcCharacter = GetCharacter(nextCharacterID);

        if (Event_OnNextTurnStarted != null)
            Event_OnNextTurnStarted(_currentSlcCharacter);        
        
        _battleLogic.FakeServer_StartNextTurn(nextCharacterID, _currentSlcCharacter.GetEligibleSpells(), _currentGeneratedAP);

        _currentGeneratedAP = 0;

        if (turnTime > 0)
            StartTimer();

        DivineDebug.Log(_currentSlcCharacter.moniker.ToString());
    }

    private void StartTimer()
    {
        _timer = turnTime;

        TimeManager.instance.Event_OneSecTick += OnTick;
    }

    private void WaitForMove()
    {
        _currentState = States.TurnLoop_WaitToMoveCompleted;

        _externalLogic.MakeDecision(_currentSlcCharacter, parties, OnMove, OnSecret);
    }

    private void FinishThisTurn()
    {
        if (!IsBattleEnd())
        {
            _timer = 0;
            TimeManager.instance.Event_OneSecTick -= OnTick;
                        
            StartNextTurn(FindNextTurnCharacterID(), false);
        }
        else
            TryToFinishBattle();
    }

    private bool IsBattleEnd()
    {
        if (_parties[0].hero.isDead || _parties[1].hero.isDead || _parties[0].chakra.isDead)
        {
            _battleShouldFinished = true;
            return true;
        }

        return false;
    }

    private void TryToFinishBattle()
    {
        _externalLogic.FightFinished(FinishBattle);
    }

    private void FinishBattle()
    {
        bool isVictory = _parties[1].hero.isDead;

        _fakeServer.FinishBattle(isVictory, new RewardData(isVictory == true ? AppManager.instance.config.initialCoin : 0, 0, new ChestInfo(0, ChestType.wooden, ChestStatus.close, new ChestRewardData())), new List<TroopCoolDownData>());
    }
    
    private void SetExternalLogic()
    {
        switch (_type)
        {
            case BattleControllerType.Tutorial_MiniBoss:
                _externalLogic = new BtlCtrlExternalLogic_TutMiniBoss();
                break;
            case BattleControllerType.Tutorial_Zahak:
                _externalLogic = new BtlCtrlExternalLogic_TutZahak();
                break;
        }

        _externalLogic.Init(this);
    }

    private void SendActions(List<ActionData> actions)
    {
        if (_battleShouldFinished)
            return;

        if (_isPause)
        {
            _waitedActionList = actions;
            return;
        }

        _waitedActionList = null;

        _fakeServer.ActionAnalyzed(actions);
    }

    //Helper Methods
    private BtlCtrlCharacter GetCharacter(int id)
    {
        if (_parties[0].HasCharacter(id))
            return _parties[0].GetCharacter(id);
        else
            return _parties[1].GetCharacter(id);
    }
    private List<BtlCtrlCharacter> GetCharacters(List<long> targets)
    {
        List<BtlCtrlCharacter> charTargets = new List<BtlCtrlCharacter>();

        for (int i = 0; i < targets.Count; i++)
            charTargets.Add(GetCharacter((int)targets[i]));

        return charTargets;
    }
    private int FindNextTurnCharacterID()
    {
        int count = 0;
        bool found = false;
        BtlCtrlCharacter character = null;

        while (!found)
        {
            count++;
            _currentTurnIndex++;

            if (count > _turnOrders.Count)
            {
                DivineDebug.Log("BattleController cant find any live character.", DivineLogType.Error);
                break;
            }

            if (_currentTurnIndex >= _turnOrders.Count)
                _currentTurnIndex = 0;

            int selectedID = (int)_turnOrders[_currentTurnIndex];
            character = GetCharacter(selectedID);

            if (character.hp > 0 && character.isActive)
                found = true;
        }
        
        return character.id;
    }
    
    //Handle Events
    private void OnReadyCome(BattleLogic battleLogic, BattleLogicStates curState)
    {
        if (_currentState == States.WaitToStartFight)
            StartNextTurn(FindNextTurnCharacterID(), true);
        else if (_currentState == States.TurnLoop_WaitTillNextCharSelected)
            WaitForMove();
        else if (_currentState == States.TurnLoop_WaitToMoveCompleted)
            FinishThisTurn();
    }

    private void OnMove(long id, int currentSelectedSpellIndex, List<long> targets)
    {
        _currentSlcCharacter.CastSpell(currentSelectedSpellIndex, GetCharacters(targets), OnSpellCasted);

        _externalLogic.MoveDone(GetCharacter((int)id));
    }

    private void OnSecret(long id, Divine.Secret secretType, List<long> targets)
    {
        _secretController.CastSecret(GetCharacter((int)id), secretType, GetCharacters(targets), OnSpellCasted);
    }

    private void OnSpellCasted(BtlCtrlSpell castedSpell)
    {
        _currentActiveSpells.Add(castedSpell);

        List<ActionData> acList = new List<ActionData>();

        int count = _currentActionsStack.Count;
        for (int i = 0; i < count; i++)
            acList.Add(_currentActionsStack.Pop());

        SendActions(acList);
    }

    private void OnSpellFinished(BtlCtrlSpell spell)
    {
        _currentActiveSpells.Remove(spell);
    }

    private void OnActionGenerated(ActionData actionData)
    {
        _currentActionsStack.Push(actionData);
    }

    private void OnActionPointChanged(BtlCtrlParty party, int changedValue)
    {
        if (party.teamType == TeamTypes.Player && changedValue > 0)
            _currentGeneratedAP += changedValue;
    }

    private void OnTick(float actualTimeInterval)
    {        
        if (!_isPause && !_battleShouldFinished)
        {
            _timer -= TimeManager.ONE_SECOND_TICK;

            int elapsedTime = turnTime - _timer;
            
            _fakeServer.DoTick(elapsedTime);

            _externalLogic.OnTick(elapsedTime);

            if (_timer <= 0)
                FinishThisTurn();
        }
    }
}