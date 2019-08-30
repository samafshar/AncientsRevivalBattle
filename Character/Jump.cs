using System;
using UnityEngine;
using Spine.Unity;

public class Jump : MonoBehaviour
{
    //Serialized
    [SerializeField]
    private bool _shouldChangeScale;
    [SerializeField]
    private LeanTweenType _easeType_Jump;
    [SpineAnimation]
    [SerializeField]
    private string[] _jumpAnims;
    [SpineAnimation]
    [SerializeField]
    private string _jumpAnim_Critical;
    [SpineAnimation]
    [SerializeField]
    private string _preJumpBackAnim;
    [SpineAnimation]
    [SerializeField]
    private string _jumpBackAnim;
    [SpineEvent]
    [SerializeField]
    private string _event_Jump;
    [SerializeField]
    private float _time_forward;
    [SerializeField]
    private float _time_back;
    [SerializeField]
    private bool _isNewJump = false;

    //Private    
    private bool _animCompleted = false;
    private bool _movingEnded = false;
    private Action _endAction;
    private Vector3 _startPos;
    private MoveInfo _moveInfo;
    private Movement _movement;
    private SkeletonAnimation _skeletonAnim;


    //Base Methods
    void Start()
    {
        _movement = GetComponent<Movement>();
        _skeletonAnim = GetComponentInChildren<SkeletonAnimation>();
    }


    //Public Methods
    public void StartIt(Divine.Moniker moniker, MoveInfo moveInfo, int spellIndex, bool isCritical, Action endAction)
    {
        _movingEnded = false;
        _animCompleted = false;

        _moveInfo = moveInfo;
        _endAction = endAction;

        _startPos = transform.position;

        if (_skeletonAnim == null)
            _skeletonAnim = GetComponentInChildren<SkeletonAnimation>();

        string animName;
        if (!_isNewJump)
            animName = _jumpAnims[0];
        else
            animName = !isCritical ? _jumpAnims[spellIndex] : _jumpAnim_Critical;

        AudioManager.instance.PlayProperJump(moniker, spellIndex, _isNewJump, isCritical);

        Spine.TrackEntry tr = _skeletonAnim.state.SetAnimation(0, animName, false);
        if (!_isNewJump)
        {
            tr.Event += OnJumpForward;
            tr.Complete += OnJumpComplete;
        }
        else
            tr.Complete += OnNewJumpPreAnimComplete;
    }

    public void GoBack(MoveInfo moveInfo, Action endAction)
    {
        _movingEnded = false;
        _animCompleted = false;

        _moveInfo = moveInfo;
        _endAction = endAction;

        Spine.TrackEntry tr = _skeletonAnim.state.SetAnimation(0, _preJumpBackAnim, false);

        if (!_isNewJump)
        {
            tr.Event += OnJumpBackward;
            tr.Complete += OnJumpComplete;
        }
        else
            tr.Complete += OnNewJumpBackPreAnimComplete;
    }

    private void OnNewJumpBackPreAnimComplete(Spine.TrackEntry trackEntry)
    {
        _animCompleted = true;

        _moveInfo.shouldChangeScale = _shouldChangeScale;
        _moveInfo.easeType = _easeType_Jump;
        _moveInfo.time = _time_back;

        _movement.StartMovement(_moveInfo, OnNewJumpBackEnd);
    }

    private void OnJumpForward(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        if (e.data.name == _event_Jump)
        {
            trackEntry.Event -= OnJumpForward;

            _moveInfo.shouldChangeScale = _shouldChangeScale;
            _moveInfo.easeType = _easeType_Jump;
            _moveInfo.time = _time_forward;

            _movement.StartMovement(_moveInfo, OnJumpMoveEnd);
        }
    }
    private void OnJumpBackward(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        if (e.data.name == _event_Jump)
        {
            trackEntry.Event -= OnJumpBackward;

            _moveInfo.shouldChangeScale = _shouldChangeScale;
            _moveInfo.easeType = _easeType_Jump;
            _moveInfo.time = _time_back;

            _movement.StartMovement(_moveInfo, OnJumpMoveEnd);
        }
    }
    private void OnJumpBackward()
    {
        _animCompleted = true;

        _moveInfo.shouldChangeScale = _shouldChangeScale;
        _moveInfo.easeType = _easeType_Jump;
        _moveInfo.time = _time_back;

        _movement.StartMovement(_moveInfo, OnJumpMoveEnd);
    }

    //Private Methods
    private void OnNewJumpPreAnimComplete(Spine.TrackEntry trackEntry)
    {
        _animCompleted = true;

        trackEntry.Complete -= OnNewJumpPreAnimComplete;

        _moveInfo.shouldChangeScale = _shouldChangeScale;
        _moveInfo.easeType = _easeType_Jump;

        _movement.StartMovement(_moveInfo, OnJumpMoveEnd);
    }
    private void OnJumpMoveEnd()
    {
        _movingEnded = true;

        OnJumpEnd();
    }
    private void OnNewJumpBackEnd()
    {
        Spine.TrackEntry tr = _skeletonAnim.state.SetAnimation(0, _jumpBackAnim, false);
        tr.Complete += NewJumpBackAnimComplete;
    }

    private void NewJumpBackAnimComplete(Spine.TrackEntry trackEntry)
    {
        OnJumpMoveEnd();
    }

    private void OnJumpComplete(Spine.TrackEntry trackEntry)
    {
        trackEntry.Complete -= OnJumpComplete;

        _animCompleted = true;

        OnJumpEnd();
    }
    private void OnJumpComplete()
    {
        _animCompleted = true;

        OnJumpEnd();
    }
    private void OnJumpEnd()
    {
        if (_movingEnded && _animCompleted)
            if (_endAction != null)
                _endAction();
    }
}
