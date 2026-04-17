using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStrafeWalkState : PlayerMovementStateBase
{
    public PlayerStrafeWalkState(PlayerMovementStateMachine ms, PlayerController c) : base(ms, c) { }
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
        //base.OnBufferComplete();
        //if (controller.HasMoveInput)
        //{
        //    if (controller.HasCrouchInput)
        //    {
        //        if (controller.HasRunInput)
        //        {
        //            stateMachine.ChangeState(stateMachine.CrouchJogState);
        //        }
        //        else
        //        {
        //            stateMachine.ChangeState(stateMachine.CrouchWalkState);
        //        }
        //    }
        //    else if(controller.HasRunInput)
        //    {
        //        stateMachine.ChangeState(stateMachine.JogState);
        //    }
        //}
    }
}
