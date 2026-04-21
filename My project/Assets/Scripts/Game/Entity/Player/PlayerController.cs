using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XInput;

[XLua.LuaCallCSharp]
public class PlayerController : CharacterControllerBase
{
    #region 参数属性
    public Vector2 _inputDirection;
    public bool HasMoveInput => _inputDirection.magnitude > 0.1f;
    public bool HasRunInput => inputActions.GamePlay.Run.IsPressed();
    public bool HasCrouchInput => inputActions.GamePlay.Crouch.IsPressed();
    public Vector3 pos;

    #region 显示玩家当前状态
    [HideInInspector]
    public IState currentState;
    [SerializeField]
    private string _currentStateName;
    public string CurrentStateName => _currentStateName;
    #endregion
    #endregion
    protected void Awake()
    {
        //玩家数据初始化
        playerHotLogic = new PlayerHotLogic();
        playerHotLogic.Init(this);
        Init();

        EventCenter.Instance.Register("LuaEnv_Ready", new Action(OnLuaReady), owner: this);
    }
    protected override void OnUpdate()
    {
        base.OnUpdate();
        UpdateInputCache();
        //驱动状态机
        playerMovementStateMachine.OnUpdate();//状态机一变，通过StateMechineBase中的currentState?.OnUpdate();执行切换的state的变更逻辑
        playerMovementStateMachine.UpdateTimer(Time.deltaTime);
        playerHotLogic.Update(Time.deltaTime);

        //显示玩家状态
        currentState = playerMovementStateMachine.currentState;
        _currentStateName = currentState?.GetType().Name ?? "Null";
    }

    private void OnLuaReady()
    {
        if (LuaMgr.Instance.Global == null) Debug.LogError("LuaMgr未被实例化");

        LuaMgr.Instance.Global.Set("PlayerCtrl", this);
        Debug.Log("PlayerCtrl 注入Lua成功");
    }

    private void OnDisable()
    {
        EventCenter.Instance.UnRegister(owner: this);
    }

    #region 封装检测输入
    void UpdateInputCache()
    {
        _inputDirection = inputActions.GamePlay.Move.ReadValue<Vector2>();
    }
    #endregion
}