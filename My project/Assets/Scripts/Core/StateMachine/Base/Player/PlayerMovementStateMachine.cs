using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using XLua;

public class PlayerMovementStateMachine : MovementStateMachineBase
{
    public readonly Dictionary<string, IState> _stateDict;
    public PlayerMovementStateMachine(PlayerController controller) :base(controller)
    {
        _stateDict = new();   
    }

    public void LuaRisterState(string StateName, LuaTable luaState)
    {
        var state = new LuaPlayerState(this, playerController,luaState);
        if (!_stateDict.ContainsKey(StateName))
            _stateDict.Add(StateName, state);
    }
    public void SwitchState(string stateName)
    {
        if (!_stateDict.ContainsKey(stateName))
        {
            Debug.LogError($"×´Ì¬²»´æÔÚ£º{stateName}");
            return;
        }
        ChangeState(_stateDict[stateName]);
    }
    public override void OnUpdate()
    {
        currentState?.OnUpdate();
        playerController.playerAnimationController.OnAnimationUpdate();
    }
}
