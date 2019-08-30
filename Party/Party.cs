using System;
using System.Collections.Generic;
using UnityEngine;

public enum PartySide
{
    Player,
    Enemy,
}

public class Party
{
    //Events
    public delegate void Deleg_PartyReady(Party owner);
    public event Deleg_PartyReady Event_PartyReady;

    public delegate void Deleg_CharacterDied(Character deadCharacter, Party ownerParty);
    public event Deleg_CharacterDied Event_CharacterDied;

    public delegate void Deleg_CharacterRecieveSpell(SpellInfo spellInfo, int effectIndex, Character reciever);
    public event Deleg_CharacterRecieveSpell Event_CharacterRecieveSpell;

    public delegate void Deleg_ChakraAppeared(Party ownerParty);
    public event Deleg_ChakraAppeared Event_ChakraAppeared;

    public delegate void Deleg_TaregtSelected(Character character);
    public event Deleg_TaregtSelected Event_TargetSelected;

    public delegate void Deleg_CharacterClicked(Character character);
    public event Deleg_CharacterClicked Event_CharacterClicked;

    public delegate void Deleg_ActionPointChanged(int actionPoint, bool isOnTurnChange);
    public event Deleg_ActionPointChanged Event_ActionPointChanged;

    //Privates
    private int                         _actionPoint;
    private bool                        _isReady = false;
    private bool                        _wantedToSelect_isSelect;
    private bool                        _wantedToSelect_isTarget;
    private Hero                        _hero;
    private string                      _name;
    private Character                   _chakra;
    private Character[]                 _characters;
    private PartySide                   _side;
    private Transform                   _selectingPoint;
    private List<Divine.Secret>         _secrets;
    private List<Character>             _currentTargetList;
    private Dictionary<long, Character> _charsDic;


    //Properties
    public Hero hero                        { get { return _hero; } }
    public Character chakra                 { get { return _chakra; } }
    public bool isReady                     { get { return _isReady; } }
    public PartySide side                   { get { return _side; } }
    public Transform selectingStatePosition { get { return _selectingPoint; } }
    public List<Divine.Secret> secrets      { get { return _secrets; } }
    public string Name                      { get { return _name; } }
    public int actionPoint
    {
        get { return _actionPoint; }
    }

    //Base Methods
    public Party(PartyInfo partyInfo, PartySide side, Transform[] initialPoints, Transform selectingPoint, Deleg_ActionPointChanged onActionPointChanged)
    {
        Init(partyInfo,side, initialPoints, selectingPoint, onActionPointChanged);
    }


    //Public Methods
    public Character FindCharacter(long characterID)
    {
        if (_charsDic.ContainsKey(characterID))
            return _charsDic[characterID];
        else
            return null;
    }
    public void StartParty(Transform[] startingSlots, CharacterStartingPoint[] formationPoints)
    {
        _hero.ComeToStartPoint(startingSlots[0].position, startingSlots[0].localScale, formationPoints[0]);

        for (int i = 0; i < _characters.Length; i++)
        {
            if (i + 1 < startingSlots.Length)
                _characters[i].ComeToStartPoint(startingSlots[i + 1].position, startingSlots[i + 1].localScale, formationPoints[i + 1]);
            else
                _characters[i].ComeToStartPoint(startingSlots[0].position, startingSlots[0].localScale, formationPoints[0]);//Chakra
        }

        DivineDebug.Log("Party Started.");
    }

    public int GetPartyCount()
    {
        return _charsDic.Count - 1;
    }
    public bool ContainCharacter(Character character)
    {
        bool result = false;

        if (character == _hero)
            result = true;

        for (int i = 0; i < _characters.Length; i++)
        {
            if (character == _characters[i])
            {
                result = true;
                break;
            }
        }

        return result;
    }
    public bool ContainCharacter(long characterID)
    {
        return _charsDic.ContainsKey(characterID);
    }
    public void UpdateCharacterStats(StatsUpdateData statsUpdateData)
    {
        Character ch = _charsDic[statsUpdateData.ownerID];

        SpellEffectInfo spellEffectInf = new SpellEffectInfo(statsUpdateData.singleStatChanges
                                                , statsUpdateData.ownerID, SpellEffectOnChar.None, statsUpdateData.finalStats);

        ch.UpdateStats(spellEffectInf, true);
    }
    public void ChangePartyTargetMode(bool isTarget, bool ignoreTaunt = false, List<Character> mustTargetChar = null)
    {
        //Do some visual effect for characters

        if (isTarget)
        {
            SetCurrentTargets(ignoreTaunt, mustTargetChar);

            int ind = 0;
            foreach (var ch in _currentTargetList)
                DivineDebug.Log("PossibleTarget " + ind++ + " named: " + ch.moniker + " isTaunt: " + ch.isTaunt);
        }

        if (_currentTargetList != null)
        {
            for (int i = 0; i < _currentTargetList.Count; i++)
                _currentTargetList[i].SetTargetStatus(isTarget);
        }

        SetRegisterationForEvent_SelectedAsTarget(isTarget);
    }
    public void EndingBattle(bool isVictorious)
    {
        foreach(var ch in _charsDic.Values)
        {
            if (!ch.isDead)
                ch.BattleEndingAction(isVictorious);
        }
    }

    public void DestroyIt()
    {
        DivineDebug.Log("Deleting Party with hero named: + " + _hero.moniker);

        foreach (var ch in _charsDic.Values)
        {
            ch.Event_OnClick       -= OnCharacterClick;
            ch.Event_StateChange   -= OnCharacterStateChange;
            ch.Event_SpellReceived -= OnCharacterSpellReceived;
            ch.Event_ChakraApeared -= OnChakraApear;
            ch.DestroyIt();
        }

        _hero                           = null;
        _charsDic                       = null;
        _characters                     = null;
        _selectingPoint                 = null;
        _currentTargetList              = null;

        Event_PartyReady                = null;
        Event_CharacterDied             = null;
        Event_ChakraAppeared            = null;
        Event_TargetSelected            = null;
        Event_ActionPointChanged        = null;
    }
    
    public Character GetRandomCharacter()
    {
        List<Character> chars = new List<Character>();

        if (!_hero.isDead && _hero.isActive)
            chars.Add(_hero);

        for (int i = 0; i < _characters.Length; i++)
            if (!_characters[i].isDead && _characters[i].isActive)
                chars.Add(_characters[i]);

        if (!chakra.isDead && chakra.isActive)
            chars.Add(chakra);

        int rand = UnityEngine.Random.Range(0, chars.Count);

        if (chars.Count == 0)
            return null;

        return chars[rand];
    }
    
    public List<Character> GetAllAvailableCharacters()
    {
        List<Character> availChars = new List<Character>(_charsDic.Values);
        availChars.RemoveAll(x => !x.isActive || x.isDead);

        return availChars;
    }

    public void SetActionPoint(int newActionPoint, bool isOnTurnChange)
    {
        _actionPoint = newActionPoint;

        if (side == PartySide.Player && Event_ActionPointChanged != null)
            Event_ActionPointChanged(_actionPoint, isOnTurnChange);
    }

    //Private Methods
    private void Init(PartyInfo partyInfo,PartySide side, Transform[] initialPoints,Transform selectingPoint, Deleg_ActionPointChanged onActionPointChanged)
    {
        DivineDebug.Log("Initing party started.");

        _side = side;
        
        _name        = partyInfo.Name;

        _secrets     = partyInfo.availableSecrets;

        _actionPoint = partyInfo.actionPoint;


        Event_ActionPointChanged += onActionPointChanged;

        _selectingPoint = selectingPoint;

        _charsDic = new Dictionary<long, Character>();

        //Build Hero
        _hero = BuildCharacter(partyInfo.heroInfo, initialPoints[0], true) as Hero;
        _hero.Event_StateChange += OnCharacterStateChange;
        _charsDic.Add(partyInfo.heroInfo.uniqueID, _hero);

        _characters = new Character[partyInfo.charInfoes.Length];

        for (int i = 0; i < partyInfo.charInfoes.Length; i++)
        {
            if (i + 1 < initialPoints.Length)
                _characters[i] = BuildCharacter(partyInfo.charInfoes[i], initialPoints[i + 1], false);
            else
                _characters[i] = BuildCharacter(partyInfo.charInfoes[i], initialPoints[0], false); //For Chakra

            _characters[i].Event_StateChange += OnCharacterStateChange;
            _characters[i].Event_ChakraApeared += OnChakraApear;
            _charsDic.Add(partyInfo.charInfoes[i].uniqueID, _characters[i]);
        }

        //Register charaters for click and receive spells
        foreach (Character ch in _charsDic.Values)
        {
            ch.Event_OnClick += OnCharacterClick;
            ch.Event_SpellReceived += OnCharacterSpellReceived;
        }

        //if (!FakeServer.instance.isFake)
        //{
            _characters[_characters.Length - 1].SetIsChakra(true);
            _chakra = _characters[_characters.Length - 1];
        //}
    }

    private void OnChakraApear()
    {
        if (Event_ChakraAppeared != null)
            Event_ChakraAppeared(this);
    }

    private Character BuildCharacter(CharInfo charInfo,Transform initialSlot, bool isHero)
    {
        DivineDebug.Log("Building character " + charInfo.moniker.ToString() + " .");

        if (isHero)
            return HeroBuilder.Build(charInfo as HeroInfo, this, initialSlot.position, initialSlot.rotation);
        else
            return CharBuilder.Build(charInfo, this, initialSlot.position, initialSlot.rotation);
    }
    private void SetRegisterationForEvent_SelectedAsTarget(bool registered)
    {
        if (_currentTargetList == null)
            return;

        if(registered)
        {
            for (int i = 0; i < _currentTargetList.Count; i++)
                _currentTargetList[i].Event_SelectAsTarget += OnCharacterSelectAsTarget;
        }
        else
        {
            for (int i = 0; i < _currentTargetList.Count; i++)
                _currentTargetList[i].Event_SelectAsTarget -= OnCharacterSelectAsTarget;
        }
    }
    private void SetCurrentTargets(bool ignoreTaunt, List<Character> mustTargetChar = null)
    {
        _currentTargetList = new List<Character>();

        if (mustTargetChar != null)
        {
            foreach (Character ch in mustTargetChar)
            {
                if (!_charsDic.ContainsValue(ch))
                    DivineDebug.Log("Must Select Target Character Is Not In This Party " +
                                        ch.moniker + " Party side: " + side.ToString(), DivineLogType.Error);

                _currentTargetList.Add(ch);
            }
            return;
        }

        if (!ignoreTaunt)
        {
            foreach (var ch in _charsDic.Values)
            {
                if (ch.isTaunt && !ch.isDead)
                    _currentTargetList.Add(ch);
            }
        }

        if (_currentTargetList.Count == 0)
        {
            foreach (var ch in _charsDic.Values)
            {
                if (ch.isTargetable)
                    _currentTargetList.Add(ch);
            }
        }
    }

    private bool IsCharacterDied(Character character, CharacterState characterState)
    {
        if (characterState == CharacterState.Dying)
            return true;

        if ((character.isHero || character.isChakra) && characterState == CharacterState.Dead)
            return true;

        return false;
    }

    //Event Handlers
    private void OnCharacterStateChange(Character owner, CharacterState newState)
    {
        _isReady = false;

        if (IsCharacterDied(owner, newState))
            if (Event_CharacterDied != null)
                Event_CharacterDied(owner, this);

        if (!(newState == CharacterState.Idle || newState == CharacterState.Dead))
            return;

        foreach (Character ch in _charsDic.Values)
        {
            if (ch.state == CharacterState.Dying)
                return;

            if (!ch.isIdle && !ch.isDead)
                return;
        }

        _isReady = true;

        if (Event_PartyReady != null)
            Event_PartyReady(this);
    }
    private void OnCharacterSelectAsTarget(Character character)
    {
        DivineDebug.Log("Party, char: " + character.moniker);

        if (Event_TargetSelected != null)        
            Event_TargetSelected(character);        
    }
    private void OnCharacterClick(Character character)
    {
        DivineDebug.Log("Char: " + character.moniker + " Clicked.");

        if (Event_CharacterClicked != null)
            Event_CharacterClicked(character);
    }
    private void OnCharacterSpellReceived(SpellInfo spellInfo, int effectIndex, Character reciever)
    {
        if (Event_CharacterRecieveSpell != null)
            Event_CharacterRecieveSpell(spellInfo, effectIndex, reciever);
    }

    internal void NonEligibleTargetSelected(Character character)
    {
        List<Character> taunts = new List<Character>();

        for (int i = 0; i < _characters.Length; i++)
        {
            if (_characters[i].isTaunt)
                taunts.Add(_characters[i]);
        }

        // This means party have one or more taunt characters
        if (taunts.Count > 0)
        {
            foreach (var _char in taunts)
            {
                // Play Taunt animation
                _char.RunSpecificAnimation(CharacterSpecificAnimation.Taunt);
            }
        }
    }

    internal void PrintCharStats()
    {
        foreach (Character ch in _charsDic.Values)
            DivineDebug.Log(ch.moniker + " in this state: " + ch.state, DivineLogType.Error);
    }
}
