using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell_Simple : Spell
{
    [SerializeField]
    private AudioClip _hit;

    protected override void StartSpell()
    {
        ApplySpellEffectsAndDestroyIt();
    }

    protected override void DestroyIt()
    {
        if (_effect != null)
        {
            _effect = Instantiate(_effect);

            _effect.transform.position = transform.position;

            if (_spellInfo.owner.isFliped)
                _effect.FlipIt();

            _effect.StartVfx((_effect) => { Destroy(_effect.gameObject); });

            if (_hit != null)
                AudioManager.instance.PlaySFX(_hit);
        }

        base.DestroyIt();
    }
}
