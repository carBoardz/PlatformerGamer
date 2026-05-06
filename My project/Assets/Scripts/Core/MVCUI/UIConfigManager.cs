using MySinleton;
using System.Collections.Generic;
using Tool.MyAB;
using UnityEngine;

/// <summary>
/// 【单一职责】仅管理UI配置数据，不加载AB，不操作UI
/// </summary>
public class UIConfigManager : Singleton<UIConfigManager>
{
    public Dictionary<string, UIConfigItem> _configCache = new();
    public bool IsConfigLoaded { get; private set; }

    /// <summary>
    /// 【GameEntry 调用】初始化：加载ConfigAB里的SO总配置
    /// </summary>
    public void InitConfig()
    {
        ABManager.Instance.LoadResAsync("configassets", "UIAllConfig", typeof(UIConfigItem), (so) =>
        {
            // 解析SO，存入字典缓存
            _configCache.Clear();
            UISOConfigs UIConfigItemSO = so as UISOConfigs;
            foreach (var item in UIConfigItemSO.allUIConfigs)
            {
                _configCache[item.uiName] = item;
            }
            IsConfigLoaded = true;
            Debug.Log("UI配置SO加载完成！");
        });
    }

    /// <summary>
    /// 【UIManager 调用】同步获取配置
    /// </summary>
    public UIConfigItem GetUIConfig(string uiName)
    {
        _configCache.TryGetValue(uiName, out var config);
        return config;
    }

    /// <summary>
    /// 清理（切换账号/退出游戏才调用）
    /// </summary>
    public void ClearCache()
    {
        _configCache.Clear();
        IsConfigLoaded = false;
    }
}