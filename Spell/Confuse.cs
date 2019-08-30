using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class Confuse : MonoBehaviour, IExtraSpell
{
    [SerializeField]
    private SkeletonAnimation _skeletonAnim;
    [SerializeField]
    [SpineAnimation]
    private string _idle;

    [SerializeField]
    private Vector2 _offset = new Vector2(0f, .8f);

    private TrackEntry _curAnimTrackEntry;

    public void Appear(Vector3 position, Vector3 scale, CharacterVisual owner)
    {
        SetFollowingPoint(owner, gameObject);

        transform.localScale *= scale.x;

        PlayAnimation(_idle, true);
    }

    public void SpellReceieved(SpellEffectOnChar effect){}

    public void CastSpell(SpellInfo spellInfo, Action<SpellInfo> onCastingMoment, Action onFinish){}

    public void Disappear()
    {
        Destroy(gameObject, .1f);
    }

    public void TurnReached(){}

    //Private
    private void PlayAnimation(string name, bool isIdle)
    {
        _curAnimTrackEntry = _skeletonAnim.state.SetAnimation(0, name, isIdle);
    }
    private void SetFollowingPoint(CharacterVisual owner, GameObject extraSpellGObj)
    {
        BoneFollower bf = extraSpellGObj.AddComponent<BoneFollower>();

        SkeletonAnimation skA = owner.GetComponentInChildren<SkeletonAnimation>();

        string boneName = skA.skeleton.FindBone("Head") != null ? "Head" : "head";        

        bf.skeletonRenderer = skA;
        bf.boneName = boneName;
        bf.followBoneRotation = false;
        bf.followSkeletonFlip = false;
    }
}
