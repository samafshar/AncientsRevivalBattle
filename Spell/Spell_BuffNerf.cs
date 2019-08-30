using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell_BuffNerf : Spell
{
    protected override void StartSpell()
    {
        transform.position = _spellInfo.spellEffectInfos[0].targetCharacter.position_pivot;

        foreach(var effect in _spellInfo.spellEffectInfos)
        {
            _effect = Instantiate<VFX>(_effect);
            _effect.transform.position = effect.targetCharacter.position_pivot;
            _effect.StartVfx();
        }

        ApplySpellEffectsAndDestroyIt();
    }
}
