using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour
{
    //Static
    public delegate void Deleg_SpellWillDestroy(Spell spell);
    public event Deleg_SpellWillDestroy Event_SpellWillDestroy;

    //Serialized
    [SerializeField]
    protected float           _startDelayTime = 0f;
    [SerializeField]
    protected VFX             _effect;
    

    //Private
    protected float           _delayTimeCounter = 0;
    protected Vector3         _InitialPos;

    //Protected
    protected SpellInfo     _spellInfo;
    
    //Public Methods
    public void Init(SpellInfo spellInfo)
    {
        _spellInfo = spellInfo;

        Invoke("StartSpell", _startDelayTime);
    }

    public virtual void HappenedAgain(bool isLastCall = false){}

    //Private Methods
    protected virtual void StartSpell()
    {
        if (_spellInfo.isInstant)
            ApplySpellEffectsAndDestroyIt();
    }

    protected void ApplySpellEffects()
    {
        for (int i = 0; i < _spellInfo.spellEffectInfos.Count; i++)
        {
            ApplySpellEffect(i);
        }
    }

    protected void ApplySpellEffectsAndDestroyIt()
    {
        DivineDebug.Log("Spell Effect Info Count: " + _spellInfo.spellEffectInfos.Count);
        for (int i = 0; i < _spellInfo.spellEffectInfos.Count; i++)
        {
            ApplySpellEffect(i);
        }

        DestroyIt();
    }

    protected void ApplySpellEffect(int spEffectIndex)
    {
        Character target = _spellInfo.spellEffectInfos[spEffectIndex].targetCharacter;

        if (target != null)
        {
            target.ReceiveSpell(_spellInfo, spEffectIndex);
        }
        else
        {
            DivineDebug.Log("No Target", DivineLogType.Error);
        }
    }

    protected virtual void DestroyIt()
    {
        if (Event_SpellWillDestroy != null)
            Event_SpellWillDestroy(this);

        Destroy(gameObject);
    }
}
