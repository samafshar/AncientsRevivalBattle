using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell_Heal_Healer : Spell
{
    [SerializeField]
    private VFX _healEffect;
    [SerializeField]
    private VFX _damageEffect;


    protected override void StartSpell()
    {
        if (_spellInfo.spellEffectInfos != null && _spellInfo.spellEffectInfos.Count > 0)
            transform.position = _spellInfo.spellEffectInfos[0].targetCharacter.position_pivot;

        VFX wantedVFX = null;

        foreach (var effect in _spellInfo.spellEffectInfos)
        {
            if (_spellInfo.spellEffectInfos[0].effectOnCharacter == SpellEffectOnChar.Buff)
                wantedVFX = Instantiate<VFX>(_healEffect);
            else
                wantedVFX = Instantiate<VFX>(_damageEffect);

            wantedVFX.transform.position = effect.targetCharacter.position_pivot;
            wantedVFX.StartVfx();
        }

        ApplySpellEffectsAndDestroyIt();
    }
}
