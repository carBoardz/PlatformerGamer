using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using XLua;

public class LuaPlayerState : IState
{
    readonly LuaTable _luaState;

    Action _enter;
    Action _exit;
    Action _onUpdate;
    Action _onFixedUpdate;
    Action _onLateUpdate;

    public readonly PlayerMovementStateMachine stateMachine;
    public readonly PlayerController controller;
    public readonly PlayerAnimationController animContl;

    protected StateTimer _bufferTimer;
    protected bool isBuffering;

    public LuaPlayerState(PlayerMovementStateMachine sm, PlayerController c, LuaTable luaTable)
    {
        controller = c;
        stateMachine = sm;
        animContl = c.playerAnimationController;
        _luaState = luaTable;
    }

    protected void Awake()
    {
        EventCenter.Instance.Register("LuaEnv_Ready", new Action(OnLuaReady));
    }
    private void OnLuaReady()
    {
        if (LuaMgr.Instance.Global == null) Debug.LogError("LuaMgr未被实例化");
        
        _enter = _luaState.Get<Action>("Enter");
        _exit = _luaState.Get<Action>("Exit");
        _onUpdate = _luaState.Get<Action>("OnUpdate");
        _onFixedUpdate = _luaState.Get<Action>("OnFixedUpdate");
        _onLateUpdate = _luaState.Get<Action>("OnLateUpdate");

        _luaState.Set("csharp", this);

        Debug.Log("LuaPlayerState 注入Lua成功");
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

    public void Enter()
    {
        _enter?.Invoke();
        Debug.Log($"玩家切换状态为{stateMachine.currentState}");
    }
    public void Exit()
    {
        _exit?.Invoke();
        TimerPool.Recycle(_bufferTimer);
        isBuffering = false;
    }
    public void OnUpdate()
    {
        _onUpdate?.Invoke();
        if (controller.HasMoveInput && !isBuffering)
        {
            StartBufferTime();
        }
        if (!controller.HasMoveInput)
        {
            isBuffering = false;
        }
    }

    public void OnFixedUpdate() => _onFixedUpdate?.Invoke();
    public void OnLateUpdate() => _onLateUpdate?.Invoke();
}
