using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIInjuredState : IState
{
    AIControlleStateMechine stateMachine;
    AIController controller;
    AIHotLogic hotLogic;
    public AIInjuredState(AIControlleStateMechine ms, AIController c)
    {
        stateMachine = ms;
        this.controller = c;
        hotLogic = c.aiHotLogic;
    }
    public void Enter() { }
    public void OnUpdate() { }
    public void OnFixedUpdate() { }
    public void OnLateUpdate() { }
    public void Exit() { }
}
