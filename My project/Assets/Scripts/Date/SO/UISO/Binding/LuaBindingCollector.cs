
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "创建SO/UI/创建 UI Component 绑定配置", fileName = "UIComponentBindConfig")]
public class LuaBindingCollector : ScriptableObject
{
    [Tooltip("对应的 UI 预制体名字（方便识别）")]
    public string uiName;

    [Tooltip("所有需要暴露给 Lua 的控件绑定列表")]
    public List<WidgetBinding> bindings = new();
}
