using MySinleton;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tool.MyAB;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GridBrushBase;

/// <summary>
/// 游戏唯一总入口
/// 职责：初始化管理器 → 预加载核心资源 → 启动Lua → 触发全局事件
/// </summary>
public class GameEntry : SingletonMono<GameEntry>
{
    // 核心AB包常量（统一管理，方便修改）
    private const string LuaBundleName = "luaassets";

    protected override void Awake()
    {
        base.Awake();
        // 同步初始化所有管理器
        InitManagers();
    }

    private async void Start()
    {
        try
        {
            // 第一步：加载Loading场景
            await LoadStartupScene();

            // 第二步：检测资源更新
            bool needRestart = await CheckAndDownloadUpdates();
            Debug.Log("needRestart:"+ needRestart);
            if (needRestart)
            {
                GameRestart.Restart();
                Debug.Log("资源下载完毕，游戏重启成功");
                return;
            }

            // 第三步：异步预加载所有核心AB包（统一管理）
            await PreloadCoreAndStartLua();

            // 第四步：触发全局事件（所有准备就绪）
            EventCenter.Instance.Trigger("Csharp_Managers_Ready");
            EventCenter.Instance.Trigger("LuaEnv_Ready");

            Debug.Log("<color=green> 游戏启动流程全部完成！</color>");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// 同步初始化所有系统管理器
    /// </summary>
    void InitManagers()
    {
        new GameObject("EventCenter").AddComponent<EventCenter>();
        new GameObject("ABMgr").AddComponent<ABManager>();
        new GameObject("LuaMgr").AddComponent<LuaMgr>();
        new GameObject("LoadSceneMgr").AddComponent<LoadSceneMgr>();
        new GameObject("UIManager").AddComponent<UIManager>();
    }

    /// <summary>
    /// 统一预加载核心AB包（Lua/配置/动画）
    /// </summary>
    async Task PreloadCoreAndStartLua()
    {
        var tcs = new TaskCompletionSource<bool>();

        // 并发加载所有核心包
        ABManager.Instance.LoadABOnlyAsync(LuaBundleName, (ok) => 
        {
            if (ok)
            {
                tcs.SetResult(true);
            }
            else
            {
                Debug.LogError("加载AB包出错");
                tcs.SetResult(false);
            }
        }); 

        // 等待全部加载完成
        await tcs.Task;

        // 初始化Lua环境
        LuaMgr.Instance.Initialize();
        LuaMgr.Instance.DoString("LuaMain");
        UIConfigManager.Instance.InitConfig();

        Debug.Log("<color=yellow> 核心AB包预加载完成：Lua + Config + PlayerAB</color>");
    }
    private bool _isUpdating = false;
    /// <summary>
    /// 资源更新
    /// </summary>
    /// <returns></returns>
    async Task<bool> CheckAndDownloadUpdates()
    {
        if (_isUpdating)
        {
            Debug.LogWarning("更新已在执行");
            return false;
        }
        _isUpdating = true;
        
        bool success = await ABUpdateManager.Instance.DownLoadCompareFile();

        try
        {
            if (success)
            {
                bool ok = await ABUpdateManager.Instance.CheckUpdate((downloadedBytes, totalBytes, DownLoadProgress) =>
                {
                    LoadingManager.Instance.UpdateProgress(downloadedBytes, totalBytes, DownLoadProgress);
                });
                return ok;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"更新资源失败，原因：{ex}");
            _isUpdating = false;
            return false;
        }
    }
    async Task LoadStartupScene()
    {
        var asyncOp = SceneManager.LoadSceneAsync(0);
        asyncOp.allowSceneActivation = true;
        while (!asyncOp.isDone)
            await Task.Yield();
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        // 游戏退出时统一释放资源
        Debug.Log("OnApplicationQuit造成的AB包缓存清理");
        ABManager.Instance.ClearAllABCache();
    }
}