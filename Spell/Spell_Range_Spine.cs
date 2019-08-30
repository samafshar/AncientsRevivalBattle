using Spine;
using Spine.Unity;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spell_Range_Spine : Spell_Range
{
    [SpineAnimation]
    [SerializeField]
    private string moveAnim;
    [SpineAnimation]
    [SerializeField]
    private string attackAnim;
    [SpineAnimation]
    [SerializeField]
    private string goBack;
    [SpineEvent]
    [SerializeField]
    private string _event_CastSpell;

    private SkeletonAnimation _sklAnim;
    

    void Start()
    {
        _sklAnim = GetComponentInChildren<SkeletonAnimation>();

        if (_sklAnim == null)
            DivineDebug.Log("Spell_Range_Spine cant find skeleton for this object " + name);
    }

    protected override void StartSpell()
    {
        base.StartSpell();

        _sklAnim.state.SetAnimation(0, moveAnim, false);
    }

    protected override Vector3 GetTargetPos()
    {        
        return _spellInfo.spellEffectInfos[0].targetCharacter.position_pivot - new Vector3(0f, 0f, 0f);
    }

    protected override void OnReachTarget()
    {
        StartCoroutine(PlayAfterTime());
    }

    IEnumerator PlayAfterTime()
    {
        yield return new WaitForSeconds(0.1f);

        TrackEntry entry = _sklAnim.state.SetAnimation(0, attackAnim, false);

        entry.Event += OnCastSpell;
        entry.Complete += OnAttackComplete;
    }

    private void OnAttackComplete(TrackEntry trackEntry)
    {
        _sklAnim.state.SetAnimation(0, goBack, false);

        MoveInfo mi = new MoveInfo(_InitialPos);

        Movement movement = GetComponent<Movement>();
        movement.StartMovement(mi, OnEndAction, false);
    }

    private void OnCastSpell(TrackEntry trackEntry, Spine.Event e)
    {
        if(e.data.name == _event_CastSpell)
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
        }
    }

    private void OnEndAction()
    {
        DestroyIt();
    }
}
