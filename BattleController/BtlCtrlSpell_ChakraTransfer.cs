using System;
using System.Collections.Generic;
using UnityEngine;

public class BtlCtrlSpell_ChakraTransfer : BtlCtrlSpell
{
    //Override Methods
    public override void Init(SpellEffectOnChar effect, int damage, int actionPoint, string spellParams,
                                                        bool isMultipleTarget = false)
    {
        base.Init(effect, damage, actionPoint, spellParams, isMultipleTarget);

        BtlCtrlSpellSubset_ChakraTransfer effectMultiPartDmg = new BtlCtrlSpellSubset_ChakraTransfer();
        
        _spellSubsets.Add(effectMultiPartDmg);
    }

    public override void StartSpell(List<BtlCtrlCharacter> targets, Action<BtlCtrlSpell> onSpellCastFinished)
    {
        base.StartSpell(targets, onSpellCastFinished);

        for (int i = 0; i < _spellSubsets.Count; i++)
            _spellSubsets[i].StartIt(this, targets);

        //Temp
        _actionPointAmount = 0;

        MakeAction();

        if (onSpellCastFinished != null)
            onSpellCastFinished(this);

        if (_eventSpellFinished != null)
            _eventSpellFinished(this);
    }
}

public class BtlCtrlSpellSubset_ChakraTransfer : BtlCtrlSpellSubset
{
    BtlCtrlCharacter.ChangeStatInfo _changeStatInfo = new BtlCtrlCharacter.ChangeStatInfo();

    public override void StartIt(BtlCtrlSpell main, List<BtlCtrlCharacter> targets)
    {
        main.owner.SetEnable(false);

        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].SetEnable(true);

            main.MakeAndAddSpellEffect(targets[i], SpellEffectOnChar.Appear, _changeStatInfo);
        }
    }
}
