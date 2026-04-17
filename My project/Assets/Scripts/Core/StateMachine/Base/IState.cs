using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public interface IState
{
    public void Enter() { }
    public void Exit() { }
    public void OnUpdate() { }
    public void OnFixedUpdate() { }
    public void OnLateUpdate() { }
}
