using Spine;
using Spine.Unity;
using System;
using UnityEngine;

public class Burn : MonoBehaviour , IExtraSpell
{
    [SerializeField]
    private SkeletonAnimation _skeletonAnim;
    [SerializeField]
    [SpineAnimation]
    private string _hit;
    [SerializeField]
    [SpineAnimation]
    private string _idle;
    
    private Vector3 _zChangeVec = new Vector3(0f, 0f, .02f);
    private TrackEntry _curAnimTrackEntry;


    public void Appear(Vector3 position, Vector3 scale, CharacterVisual owner)
    {
        transform.position = position;
        transform.localScale *= scale.x;

        _skeletonAnim.transform.localPosition = new Vector3(_skeletonAnim.transform.localPosition.x,
                                                            _skeletonAnim.transform.localPosition.y,
                                                            -.01f);

        GoToIdle();
    }

    public void Disappear()
    {
        Destroy(gameObject, .5f);
    }

    public void SpellReceieved(SpellEffectOnChar effect){}

    public void CastSpell(SpellInfo spellInfo, Action<SpellInfo> onCastingMoment, Action onFinish){}

    public void TurnReached()
    {
        _skeletonAnim.transform.localPosition -= _zChangeVec;
        PlayAnimation(_hit, false);

        _curAnimTrackEntry.Complete += OnCompleteCasting;
    }


    //Private
    private void PlayAnimation(string name, bool isIdle)
    {
        _curAnimTrackEntry = _skeletonAnim.state.SetAnimation(0, name, isIdle);
    }
    private void GoToIdle()
    {
        _skeletonAnim.transform.localPosition += _zChangeVec;
        
        PlayAnimation(_idle, true);
    }

    //Events
    private void OnCompleteCasting(TrackEntry trackEntry)
    {
        GoToIdle();
    }
}
