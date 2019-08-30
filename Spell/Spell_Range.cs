using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell_Range : Spell
{
    //Serialize
    [SerializeField]
    protected bool _rotateAtStart;
    [SerializeField]
    protected bool _vfxNeedExtraInfo;
    [SerializeField]
    protected float _offsetX;
    [SerializeField]
    protected float _offsetY;
    [SerializeField]
    protected LeanTweenType _easeType = LeanTweenType.linear;
    [SerializeField]
    protected AudioClip _hit;

    protected override void StartSpell()
    {
        _InitialPos = transform.position;

        Movement movement = GetComponent<Movement>();
        if (movement)
        {
            Vector3 targetPos = GetTargetPos();
            Vector3 targetScale = _spellInfo.spellEffectInfos[0].targetCharacter.scale;
                        
            Vector3 endPos = Utilities.FindHitPointWithOffset(new Vector2(_offsetX, _offsetY), targetPos, targetScale);

            MoveInfo mi = new MoveInfo(endPos);
            mi.easeType = _easeType;

            bool shouldRotate = true;
            ParticleSystem parSys = transform.GetComponentInChildren<ParticleSystem>();

            if (_spellInfo.owner.isFliped)
            {
                if (parSys == null)
                    transform.localScale = new Vector3(transform.localScale.x * -1,
                                                transform.localScale.y,
                                                transform.localScale.z);
                else // For particle we should change scale of each not the parent
                    parSys.transform.localScale = new Vector3(parSys.transform.localScale.x * -1,
                        parSys.transform.localScale.y,
                        parSys.transform.localScale.z);
            }

            if (parSys != null)
            {
                shouldRotate = false;

                float angleDif = Utilities.AngleBetweenToPoint(transform.position, endPos, true);
                
                float angleRad = (angleDif * 2f * 3.14f) / 360f;

                var main = parSys.main;
                main.startRotation = new ParticleSystem.MinMaxCurve(angleRad);

                parSys.Play();              
            }

            movement.StartMovement(mi, OnReachTarget, shouldRotate);
        }
        else
            DivineDebug.Log("This Spell: " + gameObject.name + " doesnt have movement.");
    }

    protected virtual void OnReachTarget()
    {
        if (_effect != null)
        {
            _effect = Instantiate(_effect);

            _effect.transform.position = transform.position;

            if (_spellInfo.owner.isFliped)
                _effect.FlipIt();

            _effect.StartVfx((_effect) => { Destroy(_effect.gameObject); });            
        }

        for (int i = 0; i < _spellInfo.spellEffectInfos.Count; i++)
            ApplySpellEffect(i);
        

        AudioManager.instance.PlaySFX(_hit);

        DestroyIt();
    }

    protected virtual Vector3 GetTargetPos()
    {
        return _spellInfo.spellEffectInfos[0].targetCharacter.position_forHit;
    }
}
