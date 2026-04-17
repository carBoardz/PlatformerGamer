using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHotLogic
{
    #region 数据与属性
    public PlayerController Controller { get; private set; }

    public int Level { get; set; }
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Atk { get; set; }

    public float MoveSpeed { get; set; }
    public float RunSpeed { get; set; }
    public float JumpForce { get; set; }
    #endregion

    public void Init(PlayerController controller)
    {
        Controller = controller;
    }
    public void Update(float deltaTime)
    {
        
    }
}
