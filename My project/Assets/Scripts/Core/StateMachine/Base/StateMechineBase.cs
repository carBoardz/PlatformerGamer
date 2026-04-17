using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMechineBase : IState
{
    public IState currentState { get; private set; }
    protected void Initialize(IState initState)
    {
        currentState = initState;
        currentState.Enter();
    }
    public void ChangeState(IState newState)
    {
        if (newState == currentState) return;

        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }
    public virtual void Enter() => Debug.Log($"currentState:{currentState}");
    public virtual void OnUpdate() => currentState?.OnUpdate();
    public virtual void OnFixedUpdate() => currentState?.OnFixedUpdate();
    public virtual void OnLateUpdate() => currentState?.OnLateUpdate();
    public virtual void Exit() { }
}
