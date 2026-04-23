using MySinleton;
using System.Collections.Generic;

/// <summary>
/// 【单一职责】仅管理UI配置数据，不加载AB，不操作UI
/// </summary>
public class UIConfigManager : Singleton<UIConfigManager>
{
    // 配置缓存：UI名 -> 配置信息
    private Dictionary<string, UIConfigItem> _configCache = new();

    /// <summary>
    /// 【由GameEntry调用】注入配置数据
    /// </summary>
    public void SetConfigData(List<UIConfigItem> configList)
    {
        _configCache.Clear();
        foreach (var item in configList)
        {
            _configCache[item.uiName] = item;
        }
    }

    /// <summary>
    /// 【由UIManager调用】获取UI配置
    /// </summary>
    public UIConfigItem GetUIConfig(string uiName)
    {
        _configCache.TryGetValue(uiName, out var config);
        return config;
    }
}

[System.Serializable]
public class UIConfigItem
{
    public string uiName;       // UI唯一名称
    public string abName;       // 所在AB包
    public string prefabName;   // 预制体名称
    public string controller;   // 控制器全名（热更反射用）
}