using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell_Range_FireRain : Spell_Range
{
    [SerializeField]
    private float _delayForDestroy;
    [SerializeField]
    private ParticleSystem _mainFire;
    [SerializeField]
    private GameObject _mainObject;

    protected override void DestroyIt()
    {
        if (_mainFire != null)
            _mainFire.Stop(false);
        else
            _mainObject.SetActive(false);

        Invoke("DestroyOnParent", _delayForDestroy);
    }

    protected override Vector3 GetTargetPos()
    {
        return _spellInfo.spellEffectInfos[0].targetCharacter.position_pivot;
    }

    private void DestroyOnParent()
    {
        base.DestroyIt();
    }
}
