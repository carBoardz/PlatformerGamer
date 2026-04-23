using MySinleton;
using System;
using System.Collections;
using System.Collections.Generic;
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
    private const string ConfigBundleName = "configassets";
    private const string PlayerBundleName = "player";

    protected override void Awake()
    {
        base.Awake();
        // 第一步：同步初始化所有管理器
        InitManagers();
    }

    private IEnumerator Start()
    {
        // 第一步：检测资源更新
        ABUpdateManager.Instance.DownLoadCompareFile(async success =>
        {
            if (success)
                await ABUpdateManager.Instance.CheckUpdate((success) =>
                {
                    if (success)
                        Debug.Log("资源下载成功");
                    else
                        Debug.LogError("资源下载失败");
                }, (Progress) =>
                {

                });
        });
        

        // 第二步：异步预加载所有核心AB包（统一管理）
        yield return PreloadCoreAssetBundles();

        // 第三步：初始化Lua环境
        LuaMgr.Instance.Initialize();
        LuaMgr.Instance.DoString("LuaMain");

        // 第四步：触发全局事件（所有准备就绪）
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
    IEnumerator PreloadCoreAssetBundles()
    {
        bool loadLuaDone = false;
        bool loadConfigDone = false;
        bool loadPlayerDone = false;

        // 并发加载所有核心包
        ABManager.Instance.LoadABOnlyAsync(LuaBundleName, (ok) => loadLuaDone = ok);
        ABManager.Instance.LoadABOnlyAsync(ConfigBundleName, (ok) => loadConfigDone = ok);
        ABManager.Instance.LoadABOnlyAsync(PlayerBundleName, (ok) => loadPlayerDone = ok);

        // 等待全部加载完成
        while (!loadLuaDone || !loadConfigDone || !loadPlayerDone)
            yield return null;

        Debug.Log("<color=yellow> 核心AB包预加载完成：Lua + Config + PlayerAB</color>");
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        // 游戏退出时统一释放资源
        ABManager.Instance.ClearAllABCache();
    }
}