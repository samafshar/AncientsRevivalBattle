using Spine;
using System;
using Spine.Unity;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageReturn : MonoBehaviour , IExtraSpell
{
    [SerializeField]
    private SkeletonAnimation   _skeletonAnim;
    [SerializeField]
    [SpineEvent]
    private string          _castEvent;
    [SerializeField]
    [SpineAnimation]
    private string          _hit;
    [SerializeField]
    [SpineAnimation]
    private string          _idle;
    [SerializeField]
    [SpineAnimation]
    private string          _appear;
    [SerializeField]
    [SpineAnimation]
    private string          _castSpell;

    [SerializeField]
    private Vector2         _offset = new Vector2(0f, .8f);
    
    private bool                _ignoreFirstDamage = true;
    private SpellInfo           _spellInfo;
    private TrackEntry          _curAnimTrackEntry;
    private Action              _onFinish;
    private Action<SpellInfo>   _onCastingMoment;
    

    public void Appear(Vector3 position, Vector3 scale,CharacterVisual owner)
    {
        SetFollowingPoint(owner, gameObject);

        transform.localScale *= scale.x;
        _skeletonAnim.skeleton.flipX = owner.isFliped;
        
        PlayAnimation(_appear, false);
        _curAnimTrackEntry.Complete += OnAppearComplete;
    }

    public void Disappear()
    {
        Destroy(gameObject, .5f);
    }

    public void SpellReceieved(SpellEffectOnChar effect)
    {
        if (effect != SpellEffectOnChar.NormalDamage && effect != SpellEffectOnChar.SeriousDamage)
            return;

        if (_ignoreFirstDamage)
        {
            _ignoreFirstDamage = false;
            return;
        }

        PlayAnimation(_hit, false);
        _curAnimTrackEntry.Complete += OnAppearComplete; // Goes to idle again
    }

    public void CastSpell(SpellInfo spellInfo, Action<SpellInfo> onCastingMoment, Action onFinish)
    {
        DivineDebug.Log("DamageReturn Cast Spell");

        _onFinish = onFinish;
        _onCastingMoment = onCastingMoment;

        _spellInfo = spellInfo;

        PlayAnimation(_castSpell, false);
        _curAnimTrackEntry.Event += OnSpineEvent;
        _curAnimTrackEntry.Complete += OnCompleteCasting;
    }

    public void TurnReached(){}


    //Private
    private void PlayAnimation(string name,bool isIdle)
    {
        _curAnimTrackEntry = _skeletonAnim.state.SetAnimation(0, name, isIdle);
    }
    private void SetFollowingPoint(CharacterVisual owner, GameObject extraSpellGObj)
    {
        BoneFollower bf = extraSpellGObj.AddComponent<BoneFollower>();
        
        SkeletonAnimation skA = owner.GetComponentInChildren<SkeletonAnimation>();

        bf.skeletonRenderer = skA;
        bf.boneName = "body";
        bf.followBoneRotation = false;
        bf.followSkeletonFlip = false;
    }

    //Events
    private void OnCompleteCasting(TrackEntry trackEntry)
    {
        PlayAnimation(_idle, true);

        if (_onFinish != null)
            _onFinish();
    }
    private void OnSpineEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (e.data.name == _castEvent)
        {
            if (_onCastingMoment != null)
                _onCastingMoment(_spellInfo);
        }
    }
    private void OnAppearComplete(TrackEntry trackEntry)
    {
        PlayAnimation(_idle, true);
    }
}
