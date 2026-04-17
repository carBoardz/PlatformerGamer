using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIControlleStateMechine : MovementStateMachineBase
{
    protected AIIdleState idleState { get; private set; }
    protected AIWalkState walkState { get; private set; }
    protected AIRunState runState { get; private set; }
    protected AIInjuredState injuredState { get; private set; }

    public AIControlleStateMechine(AIController controller) : base(controller)
    {
        idleState = new AIIdleState(this, controller);
        walkState = new AIWalkState(this, controller);
        runState = new AIRunState(this, controller);
        injuredState = new AIInjuredState(this, controller);

        Initialize(idleState);
    }
    public void SwitchToIdle() => ChangeState(idleState);
    public void SwitchToWalk() => ChangeState(walkState);
    public void SwitchToRun() => ChangeState(runState);
    public void SwitchToinjured() => ChangeState(injuredState);
}
