using System;
using Spine.Unity;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IExtraSpell
{
    void Appear(Vector3 position, Vector3 scale, CharacterVisual owner);

    void Disappear();

    void SpellReceieved(SpellEffectOnChar effect);

    void CastSpell(SpellInfo spellInfo, Action<SpellInfo> onCastingMoment, Action onFinish);

    void TurnReached();
}

public enum ExtraSpellType
{
    DamageReturn, 
    Protect,
    Confuse,
    Burn,
    Poison
}

public class ExtraSpellHandler
{
    private Dictionary<ExtraSpellType, IExtraSpell> _dic_extraSpells;

    public ExtraSpellHandler()
    {
        _dic_extraSpells = new Dictionary<ExtraSpellType, IExtraSpell>();
    }

    public void SetExtraSpells(BattleFlags prevFlag, BattleFlags newFlag, Vector3 pos, Vector3 scale, 
                                        CharacterVisual owner , bool isFromTurnStat)
    {
        if (isFromTurnStat)
            foreach (IExtraSpell exsp in _dic_extraSpells.Values)
                exsp.TurnReached();

        if (prevFlag == newFlag)
            return;

        //Damage Return
        bool newFlag_haveDmgRet = (newFlag & BattleFlags.DamageReturn) == BattleFlags.DamageReturn;
        bool prevFlag_haveDmgRet = (prevFlag & BattleFlags.DamageReturn) == BattleFlags.DamageReturn;

        if (!prevFlag_haveDmgRet && newFlag_haveDmgRet)
            AddSpell(ExtraSpellType.DamageReturn, pos, scale, owner);
        else if (prevFlag_haveDmgRet && !newFlag_haveDmgRet)
            RemoveSpell(ExtraSpellType.DamageReturn);

        //Protect
        bool newFlag_haveProtect = (newFlag & BattleFlags.Protect) == BattleFlags.Protect;
        bool prevFlag_haveProtect = (prevFlag & BattleFlags.Protect) == BattleFlags.Protect;

        if (!prevFlag_haveProtect && newFlag_haveProtect)
            AddSpell(ExtraSpellType.Protect, pos, scale, owner);
        else if (prevFlag_haveProtect && !newFlag_haveProtect)
            RemoveSpell(ExtraSpellType.Protect);

        //Confuse
        bool newFlag_haveConfuse = (newFlag & BattleFlags.Confuse) == BattleFlags.Confuse;
        bool prevFlag_haveConfuse = (prevFlag & BattleFlags.Confuse) == BattleFlags.Confuse;

        if (!prevFlag_haveConfuse && newFlag_haveConfuse)
            AddSpell(ExtraSpellType.Confuse, pos, scale, owner);
        else if (prevFlag_haveConfuse && !newFlag_haveConfuse)
            RemoveSpell(ExtraSpellType.Confuse);

        //Burn
        bool newFlag_haveBurn = (newFlag & BattleFlags.Burn) == BattleFlags.Burn;
        bool prevFlag_haveBurn = (prevFlag & BattleFlags.Burn) == BattleFlags.Burn;

        if (!prevFlag_haveBurn && newFlag_haveBurn)
            AddSpell(ExtraSpellType.Burn, pos, scale, owner);
        else if (prevFlag_haveBurn && !newFlag_haveBurn)
            RemoveSpell(ExtraSpellType.Burn);

        //Poison
        bool newFlag_havePoison = (newFlag & BattleFlags.Poison) == BattleFlags.Poison;
        bool prevFlag_havePoison = (prevFlag & BattleFlags.Poison) == BattleFlags.Poison;

        if (!prevFlag_havePoison && newFlag_havePoison)
            AddSpell(ExtraSpellType.Poison, pos, scale, owner);
        else if (prevFlag_havePoison && !newFlag_havePoison)
            RemoveSpell(ExtraSpellType.Poison);
    }

    public void SpellReceieved(SpellEffectOnChar effect)
    {
        foreach (IExtraSpell exsp in _dic_extraSpells.Values)
            exsp.SpellReceieved(effect);
    }    

    public void CastSpell(SpellInfo spellInfo, Action<SpellInfo> onCastingMoment, Action onFinish)
    {
        foreach (IExtraSpell exsp in _dic_extraSpells.Values)
            exsp.CastSpell(spellInfo, onCastingMoment, onFinish);
    }

    public void Die()
    {
        if (_dic_extraSpells.Count == 0)
            return;

        List<ExtraSpellType> extraSpellToRemove = new List<ExtraSpellType>();

        foreach (ExtraSpellType exsp in _dic_extraSpells.Keys)
            extraSpellToRemove.Add(exsp);

        for (int i = 0; i < extraSpellToRemove.Count; i++)
            RemoveSpell(extraSpellToRemove[i]);

        _dic_extraSpells.Clear();
    }


    //Privates
    private void AddSpell(ExtraSpellType type, Vector3 position, Vector3 scale,CharacterVisual owner)
    {
        if(_dic_extraSpells.ContainsKey(type))
        {
            DivineDebug.Log("Already has this Extra effect: " + type.ToString() + " owner: " + owner.moniker);
            return;
        }

        GameObject prefab = PrefabProvider_Battle.instance.GetPrefab_ExtraSpell(type.ToString());

        GameObject extraSpellGameObj = GameObject.Instantiate(prefab);
        IExtraSpell extraSp = extraSpellGameObj.GetComponent<IExtraSpell>();
        extraSp.Appear(position, scale, owner);
        
        _dic_extraSpells.Add(type, extraSp);
    }

    private void RemoveSpell(ExtraSpellType type)
    {
        if (!_dic_extraSpells.ContainsKey(type))
            return;

        _dic_extraSpells[type].Disappear();
        _dic_extraSpells.Remove(type);
    }
}
