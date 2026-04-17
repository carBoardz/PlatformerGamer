using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tool.MyAB;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using XLua;

[LuaCallCSharp]
public class PlayerAnimationController : MonoBehaviour
{
    const string PLAYER_AB = "player";
    const string ANIM_CONTROLLER = "PlayerMainAnimator";

    PlayerController _controller;
    Animator _animator;

    private readonly int _hashSpeed = Animator.StringToHash("Speed");
    private readonly int _hashDirX = Animator.StringToHash("directionX");
    private readonly int _hashDirY = Animator.StringToHash("directionY");
    private readonly int _hashIsCrouch = Animator.StringToHash("IsCrouch");
    public void Init(PlayerController playerController, Animator animator)
    {
        _controller = playerController;
        _animator = animator;

        // 从AB缓存加载动画控制器（只加载一次，常驻内存）
        
        RuntimeAnimatorController animCtrl =
            ABManager.Instance.LoadAssetSync<RuntimeAnimatorController>(PLAYER_AB, ANIM_CONTROLLER);

        if (animCtrl != null)
        {
            _animator.runtimeAnimatorController = animCtrl;
        }
    }
    public void OnAnimationUpdate()
    {
        UpdateAnimationParams();
    }
    void UpdateAnimationParams()
    {
        bool isRun = _controller.HasRunInput;
        bool hasMoveInput = _controller.HasMoveInput;
        bool hasCrouchInput = _controller.HasCrouchInput;
        float speed = hasMoveInput ? (isRun ? 2f : 1f) : 0f;

        Vector2 moveInput = _controller._inputDirection;
        float dirX = moveInput.x;
        float dirY = moveInput.y;

        _animator.SetFloat(_hashDirX, dirX);
        _animator.SetFloat(_hashDirY, dirY);
        _animator.SetFloat(_hashSpeed, speed);
        _animator.SetBool(_hashIsCrouch, true);
    }
}
