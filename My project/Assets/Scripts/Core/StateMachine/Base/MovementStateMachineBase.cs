using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class MovementStateMachineBase : StateMechineBase
{
    protected PlayerController playerController { get; private set; }
    protected AIController aiController { get; private set; }
    protected PlayerHotLogic playerHotLogic { get; private set; }
    protected AIHotLogic aiHotLogic { get; private set; }
    public StateTimer CurrentTimer { get; set; }
    protected MovementStateMachineBase(PlayerController playerController)
    {
        this.playerController = playerController;
        this.playerHotLogic = playerController.playerHotLogic;
    }
    protected MovementStateMachineBase(AIController aIController)
    {
        this.aiController = aIController;
        this.aiHotLogic = aIController.aiHotLogic;
    }
    // 统一更新计时器
    public void UpdateTimer(float deltaTime)
    {
        CurrentTimer?.OnUpdate(deltaTime);
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
}
