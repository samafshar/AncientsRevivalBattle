using Spine;
using System;
using Spine.Unity;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Protect : MonoBehaviour, IExtraSpell
{
    [SerializeField]
    private SkeletonAnimation _skeletonAnim;
    [SerializeField]
    [SpineAnimation]
    private string _hit;
    [SerializeField]
    [SpineAnimation]
    private string _idle;
    [SerializeField]
    [SpineAnimation]
    private string _appear;

    [SerializeField]
    private Vector2 _offset = new Vector2(0f, .8f);


    private bool _showingHitAnim = false;
    private TrackEntry _curAnimTrackEntry;

    public void Appear(Vector3 position, Vector3 scale, CharacterVisual owner)
    {
        SetFollowingPoint(owner, gameObject);

        transform.localScale *= scale.x;

        PlayAnimation(_appear, false);
        _curAnimTrackEntry.Complete += OnAppearComplete;
    }

    public void CastSpell(SpellInfo spellInfo, Action<SpellInfo> onCastingMoment, Action onFinish)
    {
    }

    public void SpellReceieved(SpellEffectOnChar effect)
    {
        if (effect != SpellEffectOnChar.Protect)
            return;

        _showingHitAnim = true;

        PlayAnimation(_hit, false);
        _curAnimTrackEntry.Complete += OnSpellRecievedComplete;
    }

    public void Disappear()
    {
        if (_showingHitAnim)
            return;

        Destroy(gameObject, .1f);
    }

    public void TurnReached(){ }


    //Private
    private void PlayAnimation(string name, bool isIdle)
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
    private void OnAppearComplete(TrackEntry trackEntry)
    {
        PlayAnimation(_idle, true);
    }
    private void OnSpellRecievedComplete(TrackEntry trackEntry)
    {
        _showingHitAnim = false;

        Disappear();
    }
}
