using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "创建SO/UI/创建 UIPanel 绑定配置", fileName = "UISOConfigs")]
public class UISOConfigs : ScriptableObject
{
    public List<UIConfigItem> allUIConfigs;
}

[System.Serializable]
public class UIConfigItem
{
    public string uiName;       // UI唯一名称
    public string abName;       // 所在AB包
    public string controller;   // 控制器全名（热更反射用）
    public string bindingConfig; //panel中UI组件绑定信息
    public UILayer uiLayer = UILayer.Normal; //panel挂载的层级
}