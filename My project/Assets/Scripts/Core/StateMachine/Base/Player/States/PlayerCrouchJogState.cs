using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class PlayerCrouchJogState : PlayerMovementStateBase
{
    public PlayerCrouchJogState(PlayerMovementStateMachine ms, PlayerController c) : base(ms, c){ }
    public override void Enter() 
    {
        base.Enter();
        StartBufferTime();
    }
    public override void OnUpdate()
    {
        base.OnUpdate();
    }
    public override void OnFixedUpdate() 
    {
        base.OnFixedUpdate();
    }
    public override void OnLateUpdate() 
    {
        base.OnLateUpdate();
    }
    public override void Exit()
    {
        base.Exit();
    }
    protected override void OnBufferComplete()
    {
        base.OnBufferComplete();
    }
}
