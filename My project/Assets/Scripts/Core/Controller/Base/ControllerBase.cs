using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerBase : MonoBehaviour
{
    protected virtual void Init(){ }
    protected virtual void OnUpdate() { }
    protected virtual void OnFixedUpdate() { }
    protected virtual void OnLateUpdate() { }
    protected virtual void Exit() { }

    void Update() => OnUpdate();
    void FixedUpdate() => OnFixedUpdate();
    void LateUpdate() => OnLateUpdate();
}
