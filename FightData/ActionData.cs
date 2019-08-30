using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ActionData
{
    void RunAction(object properDelegateForAction);

    void Init(BattleLogic battleLogic);
}

public class FightActionData : ActionData
{
    private Character   fightOwner;

    public int          consumedActionPoint;
    public bool         isSecret = false;
    public long         ownerID;
    public SpellInfo    spellInfo;

    //Public Methods
    public void RunAction(object delegateForCastSpell)
    {
        Character.Deleg_SpellCasted onCharacterCastSpell = delegateForCastSpell as Character.Deleg_SpellCasted;
                
        DivineDebug.Log("Consumed Action Point:" + consumedActionPoint);
        DivineDebug.Log("spellEffectInfoCount: " + spellInfo.spellEffectInfos.Count);

        fightOwner.CastSpell(spellInfo, consumedActionPoint);

        if (onCharacterCastSpell != null)
            fightOwner.Event_SpellCast += onCharacterCastSpell;
    }
    public void Init(BattleLogic battleLogic)
    {
        fightOwner = battleLogic.GetCharacterWithID(ownerID);

        spellInfo.owner = fightOwner;
        foreach (var effect in spellInfo.spellEffectInfos)
            effect.SetTarget(battleLogic.GetCharacterWithID(effect.targetCharacterID));

        DivineDebug.Log("FightAction Owner: " + fightOwner.moniker);

        DivineDebug.Log("SpellEffectInfo Count: " + spellInfo.spellEffectInfos.Count);
        for (int j = 0; j < spellInfo.spellEffectInfos.Count; j++)
        {
            DivineDebug.Log("EffectOnCharacter: " + spellInfo.spellEffectInfos[j].effectOnCharacter);

            DivineDebug.Log("GeneratedAP: " + spellInfo.generatedActionPoint);

            if (spellInfo.spellEffectInfos[j].targetCharacter != null)
                DivineDebug.Log("Target: " + spellInfo.spellEffectInfos[j].targetCharacter.moniker);

            if (spellInfo.spellEffectInfos[j].finalCharacterStats != null)
                DivineDebug.Log("Final HP: " + spellInfo.spellEffectInfos[j].finalCharacterStats.hp);

            DivineDebug.Log("singleStatChanges Count: " + spellInfo.spellEffectInfos[j].singleStatChanges.Count);
            for (int k = 0; k < spellInfo.spellEffectInfos[j].singleStatChanges.Count; k++)
            {
                DivineDebug.Log("Stat change type: " + spellInfo.spellEffectInfos[j].singleStatChanges[k].charStatChangeType.ToString()
                    + " value: " + spellInfo.spellEffectInfos[j].singleStatChanges[k].intVal.ToString());
            }            
        }
    }
}

public class SecretActionData : ActionData
{
    private Character   _secretOwner;
    private BattleLogic _battleLogic;

    public long ownerID;
    public SpellInfo _spellInfo;

    //Public Methods
    public void RunAction(object delegateForSecretCallBack)
    {
        _battleLogic.SecretReveal(_spellInfo.charSpellsIndex, _spellInfo.owner.side, OnSecretReveal);
    }
    public void Init(BattleLogic battleLogic)
    {
        _battleLogic = battleLogic;

        _secretOwner = battleLogic.GetCharacterWithID(ownerID);

        _spellInfo.owner = _secretOwner;
        foreach (var effect in _spellInfo.spellEffectInfos)
            effect.SetTarget(battleLogic.GetCharacterWithID(effect.targetCharacterID));

        DivineDebug.Log("SecretAction Inited, SpellEffectInfo Count: " + _spellInfo.spellEffectInfos.Count);
        DivineDebug.Log("Secret Generated Action Point: " + _spellInfo.generatedActionPoint);

        if (_spellInfo.spellEffectInfos[0].singleStatChanges != null && _spellInfo.spellEffectInfos[0].singleStatChanges.Count > 0)
            DivineDebug.Log("Secret Hp Change: " + _spellInfo.spellEffectInfos[0].singleStatChanges[0].intVal);
    }


    //Private Methods
    private void OnSecretReveal()
    {
        DivineDebug.Log("Running Secret, spelleffectCount:" + _spellInfo.spellEffectInfos.Count);
        foreach (var effect in _spellInfo.spellEffectInfos)
            DivineDebug.Log("SpellEffect:" + effect.effectOnCharacter);
        _secretOwner.CastSpell(_spellInfo, 0);
    }
}
