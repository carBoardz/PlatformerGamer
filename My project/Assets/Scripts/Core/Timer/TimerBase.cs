using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerBase
{
    protected float duration;       // 总时长
    protected float currentTime;    // 当前计时
    protected bool isRunning;       // 是否运行
    protected bool isUnscaled;      // 是否无视时间缩放
    public System.Action OnComplete; //计时完成回调
    public void Init(float duration, bool isUnscaled = false)
    {
        this.duration = duration;
        this.isUnscaled = isUnscaled;
    }
    public virtual void OnUpdate(float deltaTime)
    {
        if (!isRunning) return;
        currentTime += isUnscaled? Time.unscaledDeltaTime : deltaTime;
        if (currentTime >= duration)
        {
            Complete();
        }
    }
    protected virtual void Complete()
    {
        Stop();
        OnComplete?.Invoke();
    }
    public void Reset()
    {
        currentTime = 0;
        isRunning = false;
    }
    public void Start()
    {
        isRunning = true;
    }
    public void Stop()
    {
        isRunning = false;
    }
    public virtual void Clear()
    {
        Reset();
        OnComplete = null;
    }
}
