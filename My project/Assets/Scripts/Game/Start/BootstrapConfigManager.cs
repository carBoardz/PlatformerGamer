using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tool.MyAB;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 启动配置管理器 - 从 JSON 动态加载配置
/// </summary>
public class BootstrapConfigManager : MonoBehaviour
{
    [System.Serializable]
    public class BundleConfig
    {
        public string bundleName;
        public string bundleUrl;
        public string version;
        public long size;
        public string hash;
    }

    [System.Serializable]
    public class LoadingStageConfig
    {
        public string stageName;
        public int stagePriority;
        public float estimatedProgress;
        public BundleConfig[] resources;
    }

    [System.Serializable]
    public class HotUpdateRules
    {
        public bool enableHotUpdate;
        public bool checkUpdateOnStartup;
        public bool autoDownloadUpdates;
        public int maxConcurrentDownloads;
        public int retryAttempts;
        public int timeoutSeconds;
    }

    [System.Serializable]
    public class BootstrapConfig
    {
        public string version;
        public string remoteConfigUrl;
        public string localCachePath;
        public LoadingStageConfig[] stages;
        public HotUpdateRules hotUpdateRules;
    }

    private static BootstrapConfigManager _instance;
    public static BootstrapConfigManager Instance => _instance;

    [SerializeField] private string localConfigPath = "Configs/bootstrap-config";
    [SerializeField] private bool useRemoteConfig = false;

    private BootstrapConfig _currentConfig;
    private string _persistentCachePath;

    public event System.Action<BootstrapConfig> OnConfigLoaded;
    public event System.Action<string> OnConfigLoadError;

    private void Awake()
    {
        // 单例初始化 + 持久化路径创建（基础功能，建议自己实现）
    }

    /// <summary>
    /// 加载启动配置（根据useRemoteConfig判断走本地/远程）
    /// </summary>
    public void LoadBootstrapConfig()
    {
        //ABUpdateManager.Instance.CheckUpdate();
    }

    /// <summary>
    /// 从Resources加载本地JSON配置并解析
    /// </summary>
    private IEnumerator LoadLocalConfigCoroutine()
    {
        yield return null;
    }

    /// <summary>
    /// 优先读缓存，再下载远程配置，失败则用缓存
    /// </summary>
    private IEnumerator LoadRemoteConfigCoroutine()
    {
        yield return null;
    }

    /// <summary>
    /// 用UnityWebRequest下载远程JSON配置
    /// </summary>
    private IEnumerator FetchRemoteConfigCoroutine(string url, System.Action<BootstrapConfig> onComplete)
    {

        yield return null;
    }

    /// <summary>
    /// 将配置对象转为JSON，写入本地持久化路径
    /// </summary>
    private void SaveConfigCache(BootstrapConfig config, string cachePath)
    {
    }

    /// <summary>
    /// 获取当前加载完成的配置对象
    /// </summary>
    public BootstrapConfig GetCurrentConfig()
    {
        return new BootstrapConfig();
    }

    /// <summary>
    /// 按stagePriority排序加载阶段
    /// </summary>
    //public LoadingStageConfig[] GetSortedStages()
    //{
    //    return new LoadingStageConfig[2]();
    //}

    /// <summary>
    /// 对比本地AB包哈希与远程配置哈希，判断是否需要更新
    /// </summary>
    public IEnumerator CheckForUpdates(System.Action<bool> onComplete)
    {
        yield return null;
    }

    /// <summary>
    /// 读取本地AB包文件，计算MD5哈希值
    /// </summary>
    //private string GetLocalBundleHash(string bundleName)
    //{
    //    return new GetLocalBundleHash();
    //}
}