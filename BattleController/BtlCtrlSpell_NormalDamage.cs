using System;
using System.Collections.Generic;

public class BtlCtrlSpell_NormalDamage : BtlCtrlSpell
{
    //Override Methods
    public override void Init(SpellEffectOnChar effect, int damage, int actionPoint, string spellParams,
                                                        bool isMultipleTarget = false)
    {
        base.Init(effect, damage, actionPoint, spellParams, isMultipleTarget);

        BtlCtrlSpellSubset_NormalDmg effectNormalDmg = new BtlCtrlSpellSubset_NormalDmg();

        _spellSubsets.Add(effectNormalDmg);
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

    public override List<BtlCtrlCharacter> FindTarget()
    {
        DivineDebug.Log("Normal Damage Spell Cant find any target", DivineLogType.Error);

        return base.FindTarget();
    }
}
