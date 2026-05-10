using MySinleton;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tool.MyAB;
using UnityEditor;
using UnityEngine;

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
        // 第一步：同步初始化所有管理器
        InitManagers();
    }

    private async Task Start()
    {
        bool needRestart = false;
        // 第一步：检测资源更新
        await CheckAndDownloadUpdates((ok) => needRestart = ok);

        if (needRestart)
        {
            GameRestart.Restart();
            return;
        }

        // 第二步：异步预加载所有核心AB包（统一管理）
        await PreloadCoreAndStartLua();

        // 第三步：触发全局事件（所有准备就绪）
        EventCenter.Instance.Trigger("Csharp_Managers_Ready");
        EventCenter.Instance.Trigger("LuaEnv_Ready");

        Debug.Log("<color=green> 游戏启动流程全部完成！</color>");
    }

    /// <summary>
    /// 同步初始化所有系统管理器
    /// </summary>
    void InitManagers()
    {
        new GameObject("EventCenter").AddComponent<EventCenter>();
        new GameObject("ABMgr").AddComponent<ABManager>();
        new GameObject("LuaMgr").AddComponent<LuaMgr>();
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

    async Task CheckAndDownloadUpdates(System.Action<bool> onComplete)
    {
        var tcs = new TaskCompletionSource<bool>();
        ABUpdateManager.Instance.DownLoadCompareFile(async success =>
        {
            if (success)
            {
                await ABUpdateManager.Instance.CheckUpdate((success) =>
                {
                    tcs.SetResult(true);
                    onComplete?.Invoke(success);
                }, (downloadedBytes, totalBytes, DownLoadProgress) =>
                {
                    LoadingManager.Instance.UpdateProgress(downloadedBytes, totalBytes, DownLoadProgress);
                });
            }
            else
            {
                tcs.SetResult(true);
                onComplete?.Invoke(false);
            }
        });
        await tcs.Task;
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        // 游戏退出时统一释放资源
        ABManager.Instance.ClearAllABCache();
    }
}