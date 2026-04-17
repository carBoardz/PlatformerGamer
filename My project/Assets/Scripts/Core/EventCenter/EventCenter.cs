using MySinleton;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using XLua;
using static Unity.Collections.AllocatorManager;
using static UnityEngine.Application;

public class EventCenter : SingletonMono<EventCenter>
{
    /// <summary>
    /// 事件封装
    /// </summary>
    public class EventCallbackWrapper
    {
        public Delegate CSharpCallback;
        public LuaFunction LuaCallback;

        public string EventName;//名字
        public int Priority;//优先级（越小优先级越高）
        public UnityEngine.Object Owner;//所属Unity对象（自动解绑用）
        public bool MainThreadOnly;//是否仅主线程执行
        public bool Once;//是否只执行一次

        public void Invoke(params object[] obj)
        {
            try
            {
                if (Owner != null && Owner.Equals(null)) return; // Unity对象已销毁
                if (CSharpCallback == null && (LuaCallback == null || LuaCallback.Equals(null))) return;
                if (CSharpCallback != null)
                {
                    CSharpCallback.DynamicInvoke(obj);
                }
                else if (LuaCallback != null && !LuaCallback.Equals(null))
                {
                    LuaCallback.Call(obj);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"事件{EventName}回调执行失败: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                if (Once)
                {
                    EventCenter.Instance.UnRegister(Owner, null, this);
                }
            }
        }
    }

    readonly Dictionary<string, List<EventCallbackWrapper>> _eventDict = new();//按优先级存储事件字典
    readonly Dictionary<EventCallbackWrapper, object[]> _callbackDedupDict = new();//当前这一帧的去重字典
    private readonly Queue<(EventCallbackWrapper wrapper, object[] args)> _singleCallbackQueue = new();
    private bool _isMainThreadCheckTriggered = false;//是否有待处理的主线程回调
    readonly object _callbackQueueLock = new();
    readonly ObjectPool<EventCallbackWrapper> objectPool = new ObjectPool<EventCallbackWrapper>(
        // 1. 创建函数：当池为空时，新建包装类
        () => new EventCallbackWrapper(),
        // 2. 重置函数：回收时清空包装类数据
        actionOnRelease: (wrapper) =>
        {
            wrapper.EventName = null;
            wrapper.CSharpCallback = null;
            wrapper.LuaCallback?.Dispose(); // 释放LuaFunction，防止内存泄漏
            wrapper.LuaCallback = null;
            wrapper.Priority = 0;
            wrapper.MainThreadOnly = true;
            wrapper.Owner = null;
            wrapper.Once = false;
        }
    );

    protected override void Awake()
    {
        base.Awake();
        Application.quitting += OnApplicationQuit;

        EventCenter.Instance.Register(
        "Csharp_Managers_Ready",
        new Action(OnCsharpManagersReady),
        owner: this,
        once: true // 只执行一次
        );
    }
    void Update()
    {
        ProcessMainThreadCallbacks();
    }
    #region 对外接口
    /// <summary>
    /// 注册事件
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="callback">回调事件</param>
    /// <param name="Priority">事件执行优先级</param>
    /// <param name="owner">事件所属对象</param>
    /// <param name="MainThreadOnly">是否在主线程执行</param>
    /// <param name="once">是否只执行一次</param>
    public void Register(string eventName, Delegate callback, int Priority = 0, UnityEngine.Object owner = null, bool MainThreadOnly = true, bool once = false)
    {
        if (string.IsNullOrEmpty(eventName) || callback == null)
        {
            Debug.LogError("事件注册失败：事件名或回调为空");
            return;
        }
        var wrapper = objectPool.Get();
        wrapper.EventName = eventName;
        wrapper.CSharpCallback = callback;
        wrapper.Priority = Priority;
        wrapper.MainThreadOnly = MainThreadOnly;
        wrapper.Owner = owner;
        wrapper.Once = once;

        if (!_eventDict.ContainsKey(eventName))
        {
            _eventDict[eventName] = new List<EventCallbackWrapper>();
        }
        var callbackList = _eventDict[eventName];
        callbackList.Add(wrapper);
        callbackList.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        /// 监听所属对象销毁（自动解绑）
        if (owner != null)
        {
            GameObject targetGo = GetGameObjectFromUnityObject(owner);
            if (targetGo == null)
            {
                Debug.LogError("未能获取事件所属监听对象"); 
                return;
            }
            targetGo.TryGetComponent(out EventAutoUnBinder binder);
            if (binder == null) binder = targetGo.AddComponent<EventAutoUnBinder>();
            binder.AddUnBindAction(() => UnRegister(owner, eventName));
        }
    }
    /// <summary>
    /// lua注册事件
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="LuaCallback">lua回调事件</param>
    /// <param name="Priority">事件执行优先级</param>
    /// <param name="MainThreadOnly">是否在主线程执行</param>
    /// <param name="Once">是否只执行一次</param>
    public void RegisterLua(string eventName, LuaFunction LuaCallback, int Priority = 0, bool MainThreadOnly = true, bool once = false)
    {
        if (string.IsNullOrEmpty(eventName) || LuaCallback == null || LuaCallback.Equals(null))
        {
            Debug.LogError("事件注册失败：事件名或回调为空");
            return;
        }
        var wrapper = objectPool.Get();
        wrapper.LuaCallback = LuaCallback;
        wrapper.Priority = Priority;
        wrapper.MainThreadOnly = MainThreadOnly;
        wrapper.Once = once;

        if (!_eventDict.ContainsKey(eventName))
        {
            _eventDict[eventName] = new List<EventCallbackWrapper>();
        }
        var callbackList = _eventDict[eventName];
        callbackList.Add(wrapper);
        callbackList.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }
    /// <summary>
    /// 注销事件
    /// </summary>
    /// <param name="owner">事件所属对象</param>
    /// <param name="eventName">事件名称</param>
    /// <param name="targetWrapper">要移除的事件</param>
    public void UnRegister(UnityEngine.Object owner = null, string eventName = null, EventCallbackWrapper targetWrapper = null)
    {
        //解绑指定对象的所有事件
        if (owner != null && string.IsNullOrEmpty(eventName))
        {
            foreach (var kvp in _eventDict)
            {
                var callbackList = kvp.Value;
                if (callbackList == null)
                {
                    _eventDict.Remove(kvp.ToString());
                }
                for (int i = callbackList.Count - 1; i >= 0; i--)
                {
                    var wrapper = callbackList[i];
                    if (wrapper.Owner == owner)
                    {
                        callbackList.RemoveAt(i);
                        RecycleWrapper(wrapper);
                    }
                }
            }
            return;
        }
        if (!string.IsNullOrEmpty(eventName) && _eventDict.ContainsKey(eventName))
        {
            var callbackList = _eventDict[eventName];
            if (callbackList == null)
            {
                _eventDict.Remove(eventName);
            }
            for (int i = callbackList.Count - 1; i >= 0; i--)
            {
                var wrapper = callbackList[i];
                bool needRemove = false;
                if (callbackList[i].Owner == owner) needRemove = true;
                if (targetWrapper != null && wrapper == targetWrapper) needRemove = true;
                if (needRemove)
                {
                    callbackList.RemoveAt(i);
                    RecycleWrapper(wrapper);
                }
            }
        }
    }
    /// <summary>
    /// 执行事件
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="args">事件参数</param>
    public void Trigger(string eventName, params object[] args)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            Debug.LogError("事件触发失败：事件名为空");
            return;
        }
        InternalTrigger(eventName, args);
    }
    #endregion
    #region 内部优化
    void InternalTrigger(string eventName,params object[] args)
    {
        if (!_eventDict.TryGetValue(eventName, out var list)) return;
        var tempWrappers = new List<EventCallbackWrapper>(_eventDict[eventName]);
        bool hasMainThreadCallback = false;
        foreach (var wrapper in tempWrappers)
        {
            if (Thread.CurrentThread.ManagedThreadId == 1)
            {
                wrapper.Invoke(args);
                continue;
            }

            if(wrapper.MainThreadOnly)
            {
                lock (_callbackQueueLock)
                {
                    if (_callbackDedupDict.ContainsKey(wrapper))
                    {
                        _callbackDedupDict[wrapper] = args;  // 只更新最新参数
                    }
                    else
                    {
                        _singleCallbackQueue.Enqueue((wrapper, args));
                        _callbackDedupDict[wrapper] = args;
                    }
                    hasMainThreadCallback = true;
                }
            }
            else
            {
                wrapper.Invoke(args);
            }

            if (hasMainThreadCallback && !_isMainThreadCheckTriggered)
            {
                _isMainThreadCheckTriggered = true;
                Invoke(nameof(ProcessMainThreadCallbacks), 0);
            }

            if (wrapper.Once)
            {
                UnRegister(null, eventName, wrapper);
            }
        }
    }
    void ProcessMainThreadCallbacks()
    {
        if (Thread.CurrentThread.ManagedThreadId != 1) return;
        lock (_callbackQueueLock)
        {
            while (_singleCallbackQueue.Count > 0)
            {
                var (wrapper, _) = _singleCallbackQueue.Dequeue();
                // 获取最新参数
                var args = _callbackDedupDict[wrapper];
                _callbackDedupDict.Remove(wrapper);
                if (wrapper != null && !_isCallbackRemoved(wrapper))
                {
                    wrapper.Invoke(args);
                    if (wrapper.Once)
                    {
                        UnRegister(null, wrapper.EventName, wrapper);
                    }
                }
            }
        }
        _isMainThreadCheckTriggered = false;
    }
    /// <summary>
    /// 辅助方法
    /// </summary>
    /// <param name="wrapper">要回调的事件</param>
    /// <returns>检测缓存中是否存在此回调</returns>
    bool _isCallbackRemoved(EventCallbackWrapper wrapper)
    {
        foreach (var kvp in _eventDict)
        {
            if (kvp.Value.Contains(wrapper))
            {
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// 回收回调包装类到对象池（GC优化）
    /// </summary>
    void RecycleWrapper(EventCallbackWrapper wrapper)
    {
        wrapper.CSharpCallback = null;
        wrapper.LuaCallback?.Dispose(); // 释放LuaFunction（XLua内存优化）
        wrapper.LuaCallback = null;
        wrapper.Priority = 0;
        wrapper.MainThreadOnly = true;
        wrapper.Owner = null;
        wrapper.Once = false;
        objectPool.Release(wrapper);
    }
    void OnCsharpManagersReady()
    {

    }
    void OnApplicationQuit()
    {
        _eventDict.Clear();
        objectPool.Clear();
        lock (_callbackQueueLock) _callbackDedupDict.Clear();
        lock (_callbackQueueLock) _singleCallbackQueue.Clear();
    }
    #endregion
    #region 配套工具类
    /// <summary>
    /// 工具方法：将UnityEngine.Object转换为可挂载组件的GameObject
    /// 处理Component/GameObject类型，过滤Texture/AudioClip等非挂载对象
    /// </summary>
    private GameObject GetGameObjectFromUnityObject(UnityEngine.Object unityObj)
    {
        if (unityObj == null) return null;

        if (unityObj is GameObject go)
        {
            return go;
        }

        if (unityObj is Component comp)
        {
            return comp.gameObject;
        }

        return null;
    }
    /// <summary>
    /// Unity对象销毁时自动解绑事件
    /// </summary>
    class EventAutoUnBinder : MonoBehaviour
    {
        List<Action> _UnBindAction = new();
        public void AddUnBindAction(Action action)
        {
            _UnBindAction.Add(action);
        }
        public void OnDestroy()
        {
            foreach (var action in _UnBindAction)
            {
                action.Invoke();
            }
            _UnBindAction.Clear();
        }
    }
    #endregion
}