using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtlCtrlParty
{
    public delegate void Deleg_APChanged(BtlCtrlParty party, int changedValue);

    public event Deleg_APChanged Event_APChanged;

    private int _ap;
    private string _name;
    private BtlCtrlCharacter _hero;
    private BtlCtrlCharacter _chakra;
    private BattleController.TeamTypes _teamType;
    private Dictionary<int,BtlCtrlCharacter> _characters;

    public int ap { get { return _ap; } }
    public string partyName { get { return _name; } }
    public BtlCtrlCharacter hero { get { return _hero; } }
    public BtlCtrlCharacter chakra { get { return _chakra; } }
    public BattleController.TeamTypes teamType { get { return _teamType; } }

    public void Init(List<BtlCtrlCharacter> characters, BattleController.TeamTypes teamType, string partyName)
    {
        _characters = new Dictionary<int, BtlCtrlCharacter>();

        _teamType = teamType;

        _name = partyName;

        for (int i = 0; i < characters.Count; i++)
        {
            BtlCtrlCharacter.Type ty = characters[i].type;

            if (ty == BtlCtrlCharacter.Type.Hero)
                _hero = characters[i];
            else if (ty == BtlCtrlCharacter.Type.Chakra)
                _chakra = characters[i];

            characters[i].SetRelatedParty(this);
            _characters.Add(characters[i].id, characters[i]);
        }
    }

    public void ChangeAP(int changedValue)
    {
        _ap += changedValue;

        if (Event_APChanged != null)
            Event_APChanged(this, changedValue);
    }

    public bool HasCharacter(int id)
    {
        return _characters.ContainsKey(id);
    }
    public bool HasCharacter(BtlCtrlCharacter character)
    {
        return _characters.ContainsKey(character.id);
    }

    public BtlCtrlCharacter GetCharacter(int id)
    {
        if (!HasCharacter(id))
            return null;

        return _characters[id];
    }
    public List<BtlCtrlCharacter> GetCharacters()
    {
        return new List<BtlCtrlCharacter>(_characters.Values);
    }
    public BtlCtrlCharacter GetAliveTroop()
    {
        foreach (BtlCtrlCharacter ch in _characters.Values)
            if (ch.hp > 0 && ch.id != hero.id && ch.id != chakra.id)
                return ch;

        DivineDebug.Log("Couldn't find alived troop in Battle ctrl party.", DivineLogType.Error);

        return null;
    }    

    public void DestroyIt()
    {
        foreach (var ch in _characters.Values)
            ch.DestoryCharacter();

        _hero = null;
        _chakra = null;
        
        _characters.Clear();
        _characters = null;
    }
}
