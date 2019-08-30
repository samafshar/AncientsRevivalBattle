using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell_ActionThenEffect : Spell
{
    //Serialize
    [SerializeField]
    private float       _delayForEffect;

    //Override Methods
    protected override void StartSpell()
    {
        _InitialPos     = transform.position;

        Movement movement = GetComponent<Movement>();
        if (movement)        
            movement.StartMovement(ReachToPointOrTimeReached);        
        else        
            Invoke("ReachToPointOrTimeReached", _delayForEffect);        
    }

    //Private Methods
    private void ReachToPointOrTimeReached()
    {        
        Vector2 targetPos = _spellInfo.spellEffectInfos[0].targetCharacter.position_pivot;

        _effect = Instantiate(_effect);
        _effect.StartVfx(_InitialPos, targetPos, ApplySpellEffectsAndDestroyIt);
    }
}
