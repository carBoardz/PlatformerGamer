using MySinleton;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// 单一职责：AB包版本对比、服务器地址对比、更新判断
/// </summary>
public class ABCompareManager : SingletonMono<ABCompareManager>
{
    [System.Serializable]
    public class ServerConfig
    {
        public string FtpServerUrl;
        public string loginServerUrl;// 登录服地址
        public string gameServerUrl;// 游戏主服地址
        //public ServerZone[] zones;// 大区列表（1区、2区、渠道区…）
    }
    [System.Serializable]
    public class LocalServerConfig
    {
        public ServerConfig currentServer;
    }
    [System.Serializable]
    public class BundleVersionInfo
    {
        public string bundleName;
        public string currentVersion;
        public string bundleURI;
        public string remoteVersion;
        public string remoteHash;
        public long remoteSize;
        public bool needsUpdate;
    }
    [System.Serializable]
    public class CompareResult
    {
        public bool hasBundleUpdates;
        public bool hasServerChange;
        public List<BundleVersionInfo> updatesNeeded = new List<BundleVersionInfo>();
        public List<BundleVersionInfo> upToDate = new List<BundleVersionInfo>();
        public long totalDownloadSize;
        public string latestRemoteVersion;
        public ServerConfig newServerConfig;
    }

    public string _localVersionPath;
    public string _remoteVersionPath;
    public string _localServerConfigPath;
    public string _remoteServerConfigPath;

    private LocalServerConfig _localConfig;
    //public void Awake()
    //{
    //    _localVersionPath = Path.Combine(Application.persistentDataPath, "version.json");
    //}

    /// <summary>
    /// 对比：本地版本 VS 远程版本 → 返回是否需要更新
    /// </summary>
    public CompareResult CompareBundleVersions(List<BundleVersionInfo> localVersions, List<BundleVersionInfo> remoteVersions)
    {
        CompareResult result = new CompareResult();

        

        return result;
    }
    /// <summary>
    /// 对比：本地服务器配置 VS 远程服务器配置 → 是否发生变更
    /// </summary>
    public bool CheckServerChanged(ServerConfig localConfig, ServerConfig remoteConfig)
    {
        if (localConfig == null || remoteConfig == null)
            return true;

        return localConfig.FtpServerUrl != remoteConfig.FtpServerUrl;
    }
    #region 加载配置文件
    /// <summary>
    /// 加载本地存储的服务器配置
    /// </summary>
    void LoadLocalServerConfig()
    {
        // 如果本地没有配置文件，创建默认配置
        if (!File.Exists(_localServerConfigPath))
        {
            _localConfig = new LocalServerConfig();
            _localConfig.currentServer = new ServerConfig()
            {
                FtpServerUrl = "http://26.166.242.49:8000/ABRes/", // 默认初始地址
                loginServerUrl = "",
                gameServerUrl = ""
            };
        }
        else
        {
            // 读取本地JSON文件并解析
            string json = File.ReadAllText(_localServerConfigPath);
            _localConfig = JsonUtility.FromJson<LocalServerConfig>(json);
        }
    }
    #endregion
}
