using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public static class TimerPool
{
    private static readonly ObjectPool<StateTimer> _pool;
    static TimerPool()
    {
        _pool = new ObjectPool<StateTimer>(
            () => new StateTimer(),
            timer => timer.Init(0),
            timer => timer.Clear()
        );
    }
    public static StateTimer Get(float duration, bool isUnscaled = false)
    {
        StateTimer timer = _pool.Get();
        timer.Init(duration, isUnscaled);
        timer.Reset();
        return timer;
    }
    public static void Recycle(StateTimer timer)
    {
        if (timer == null) return;
        _pool.Release(timer);
    }
}
