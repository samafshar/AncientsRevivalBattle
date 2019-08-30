using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell_DamagingMultipleTime : Spell
{
    private int _currentIndex = 0;
    private bool _shouldDestroy = false;


    protected override void StartSpell()
    {
        if (_shouldDestroy)
            return;

        if (_currentIndex < _spellInfo.spellEffectInfos.Count)
        {
            _spellInfo.spellEffectInfos[_currentIndex].isMultiPart = true;
            ApplySpellEffect(_currentIndex);
        }

        if (_currentIndex >= _spellInfo.spellEffectInfos.Count - 1)
        {
            _shouldDestroy = true;
            DestroyIt();
        }
    }

    public override void HappenedAgain(bool isLastCall = false)
    {
        if (!isLastCall)
            NextSpell();
        else
        {
            while (!_shouldDestroy)
                NextSpell();
        }
    }

    private void NextSpell()
    {
        _currentIndex++;
        StartSpell();
    }
}
