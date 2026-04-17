using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerMovementStateBase : IState
{
    #region 参数与构造函数
    protected readonly PlayerMovementStateMachine stateMachine;
    protected readonly PlayerController controller;
    protected readonly PlayerHotLogic hotLogic;
    protected readonly PlayerAnimationController animContl;
    protected StateTimer _bufferTimer;
    protected bool isBuffering;

    public PlayerMovementStateBase(PlayerMovementStateMachine ms, PlayerController c)
    {
        stateMachine = ms;
        this.controller = c;
        hotLogic = c.playerHotLogic;
        animContl = c.playerAnimationController;
    }
    #endregion
    public virtual bool CheckInput() 
    {
        return controller.HasMoveInput;
    }
    #region 计时器方法
    /// <summary>
    /// 计时器结束回调函数
    /// </summary>
    protected virtual void OnBufferComplete()
    {
        isBuffering = false;
        TimerPool.Recycle(_bufferTimer);
        _bufferTimer = null;
    }
    /// <summary>
    /// 从缓存池取出计时器同时初始化
    /// </summary>
    public virtual void StartBufferTime()
    {
        if (isBuffering) return;
        _bufferTimer = TimerPool.Get(0.11f);
        _bufferTimer.OnComplete = OnBufferComplete;
        _bufferTimer.Start();
        stateMachine.CurrentTimer = _bufferTimer;
        isBuffering = true;
    }
    /// <summary>
    /// 重置计时器
    /// </summary>
    protected void RecycleTimer()
    {
        if (_bufferTimer != null)
        {
            _bufferTimer.Stop();
            _bufferTimer.Clear();
            TimerPool.Recycle(_bufferTimer);
            _bufferTimer = null;
        }
    }
    #endregion

    #region 重写父类逻辑
    /// <summary>
    /// 玩家进入当前状态时执行的函数
    /// </summary>
    public virtual void Enter() 
    {
        Debug.Log($"玩家切换状态为{stateMachine.currentState}");
    }
    /// <summary>
    /// 玩家退出当前状态时执行的函数
    /// </summary>
    public virtual void Exit()
    {
        TimerPool.Recycle(_bufferTimer);
        isBuffering = false;
    }
    /// <summary>
    /// 当前状态每帧检测的函数
    /// </summary>
    public virtual void OnUpdate() 
    {
        if (controller.HasMoveInput && !isBuffering)
        {
            StartBufferTime();
        }
        if (!controller.HasMoveInput)
        {
            isBuffering = false;
        }
    }
    public virtual void OnFixedUpdate() { }
    public virtual void OnLateUpdate() { }
    #endregion
}
