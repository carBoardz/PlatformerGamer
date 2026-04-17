using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class AIHotLogic
{
    public AIController Controller { get; protected set; }

    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Atk { get; set; }

    public float MoveSpeed { get; set; }
    public float RunSpeed { get; set; }
    public float JumpForce { get; set; }
    public void Init(AIController controller)
    {
        Controller = controller;
    }
    public void Update(float deltaTime)
    {

    }
}
