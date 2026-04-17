using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWalkState : IState
{
    AIControlleStateMechine stateMachine;
    AIController controller;
    AIHotLogic hotLogic;
    public AIWalkState(AIControlleStateMechine ms, AIController c)
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
