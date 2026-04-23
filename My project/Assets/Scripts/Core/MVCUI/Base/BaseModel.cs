using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseModel
{
    /// <summary>
    /// 初始化Model
    /// </summary>
    public abstract void InitModel();

    /// <summary>
    /// 释放Model资源
    /// </summary>
    public abstract void DisposeModel();
}
