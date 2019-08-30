using System;
using System.Collections.Generic;
using UnityEngine;

public class BtlCtrlSpell_MultiPartDamage : BtlCtrlSpell
{
    private float[] _percents;

    //Override Methods
    public override void Init(SpellEffectOnChar effect, int damage, int actionPoint, string spellParams,
                                                        bool isMultipleTarget = false)
    {
        base.Init(effect, damage, actionPoint, spellParams, isMultipleTarget);

        BtlCtrlSpellSubset_MultiPartDamage effectMultiPartDmg = new BtlCtrlSpellSubset_MultiPartDamage();

        effectMultiPartDmg.SetPercent(_percents);

        _spellSubsets.Add(effectMultiPartDmg);
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

    //Public Methods
    public void SpecialInit(params float[] percents)
    {
        _percents = percents;
    }
}

public class BtlCtrlSpellSubset_MultiPartDamage : BtlCtrlSpellSubset
{
    private int _damage;
    private float[] _percents;

    BtlCtrlCharacter.ChangeStatInfo _changeStatInfo;

    public override void StartIt(BtlCtrlSpell main, List<BtlCtrlCharacter> targets)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            _damage = main.damage;
            _changeStatInfo = targets[i].ApplyDamage(_damage);

            BtlCtrlCharacter.ChangeStatInfo[] changeStatArray = CalculateChangeStateByPercent();

            for (int j = 0; j < changeStatArray.Length; j++)
                main.MakeAndAddSpellEffect(targets[i], SpellEffectOnChar.NormalDamage, changeStatArray[j]);
        }
    }

    public void SetPercent(float[] percents)
    {
        _percents = percents;
    }

    //Private Methods
    private BtlCtrlCharacter.ChangeStatInfo[] CalculateChangeStateByPercent()
    {
        BtlCtrlCharacter.ChangeStatInfo[] changeStatArray = new BtlCtrlCharacter.ChangeStatInfo[_percents.Length];

        int dmg, currentDmg;
        dmg = currentDmg = _damage;

        for (int i = 0; i < changeStatArray.Length; i++)
        {
            if (i != changeStatArray.Length - 1) //last one
            {
                currentDmg = (int)(_damage * _percents[i]);
                dmg -= currentDmg;
            }
            else
                currentDmg = dmg;

            int shieldChange = 0, hpChange = 0;

            if (_changeStatInfo.shieldChangeAmount != 0)
            {
                if (Mathf.Abs(currentDmg) <= Mathf.Abs(_changeStatInfo.shieldChangeAmount))
                {
                    shieldChange = currentDmg;

                    _changeStatInfo.ChangeShield(Mathf.Abs(currentDmg));

                    currentDmg = 0;
                }
                else
                {
                    shieldChange = _changeStatInfo.shieldChangeAmount;

                    currentDmg -= _changeStatInfo.shieldChangeAmount;

                    _changeStatInfo.ChangeShield(Mathf.Abs(_changeStatInfo.shieldChangeAmount));//make it 0
                }
            }

            if (currentDmg != 0)
            {
                hpChange = currentDmg;

                _changeStatInfo.ChangeHP(Mathf.Abs(currentDmg));

                currentDmg = 0;
            }

            changeStatArray[i].SetValues(hpChange, shieldChange);
        }

        return changeStatArray;
    }
}
