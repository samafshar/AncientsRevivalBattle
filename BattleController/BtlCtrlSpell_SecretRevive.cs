using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtlCtrlSpell_SecretRevive : BtlCtrlSpell
{
    public override void Init(SpellEffectOnChar effect, int damage, int actionPoint, string spellParams, bool isMultipleTarget = false)
    {
        base.Init(effect, damage, actionPoint, spellParams, isMultipleTarget);

        _isSecret = true;
        _secretType = Divine.Secret.Revival;

        BtlCtrlSpellSubset_SecretRev subset = new BtlCtrlSpellSubset_SecretRev();

        _spellSubsets.Add(subset);
    }

    public override void StartSpell(List<BtlCtrlCharacter> targets, Action<BtlCtrlSpell> onSpellCastFinished)
    {
        base.StartSpell(targets, onSpellCastFinished);

        for (int i = 0; i < _spellSubsets.Count; i++)
            _spellSubsets[i].StartIt(this, targets);

        MakeAction();

        if (onSpellCastFinished != null)
            onSpellCastFinished(this);

        if (_eventSpellFinished != null)
            _eventSpellFinished(this);
    }
}
