using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;

public class Grave : MonoBehaviour
{
    //Serialize
    [SerializeField]
    [SpineAnimation]
    private string      _idle;

    [SerializeField]
    [SpineAnimation]
    private string[]    _appear;

    [SerializeField]
    [SpineAnimation]
    private string[]    _revive;

    [SerializeField]
    [SpineEvent]
    private string      _reviveEvent;

    [SerializeField]
    [SpineSlot]
    private string      _headSlot;

    [SerializeField]
    [SpineSlot]
    private string      _weaponSlot;
    

    //Private
    private System.Action       _onAppearDone;
    private System.Action       _onReviveEventHappened;
    private SkeletonAnimation   _skeletonAnim = null;


    //Public Methods
    public void Appear(System.Action onAppearDone, string moniker, bool flipX)
    {
        _onAppearDone = onAppearDone;
        
        if (_skeletonAnim == null)
            _skeletonAnim = GetComponent<SkeletonAnimation>();
        
        _skeletonAnim.skeleton.a = 1;

        _skeletonAnim.skeleton.flipX = flipX;

        if (_skeletonAnim.skeleton.GetAttachment(_headSlot, "heads/" + moniker) != null)
            _skeletonAnim.skeleton.SetAttachment(_headSlot, "heads/" + moniker);
        else
            DivineDebug.Log("Grave for " + moniker + " Doesn't Exist", DivineLogType.Error);

        if (_skeletonAnim.skeleton.GetAttachment(_weaponSlot, moniker) != null)
            _skeletonAnim.skeleton.SetAttachment(_weaponSlot, moniker);
        else
            _skeletonAnim.skeleton.SetAttachment(_weaponSlot, "bone");

        _skeletonAnim.state.SetAnimation(0, Utilities.GetRandomArrayElement<string>(_appear), false).Complete += OnAppear;
    }

    internal void Revive(Action reviveEventHappened)
    {
        _onReviveEventHappened = reviveEventHappened;

        _skeletonAnim.state.SetAnimation(0, Utilities.GetRandomArrayElement<string>(_revive), false).Event += OnReviveEventHappen;
    }

    private void OnReviveEventHappen(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        if(e.data.name == _reviveEvent)
        {
            LeanTween.value(gameObject, _skeletonAnim.skeleton.a, 0, 1f).
            setOnUpdate((float val) =>
            {
                _skeletonAnim.skeleton.a = val;
            });

            if (_onReviveEventHappened != null)
                _onReviveEventHappened();
        }
    }


    //Handlers
    private void OnAppear(Spine.TrackEntry trackEntry)
    {
        _skeletonAnim.state.SetAnimation(0, _idle, true);

        if (_onAppearDone != null)
            _onAppearDone();
    }
}
