using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

[System.Serializable]
public class WidgetBinding
{
    [Tooltip("Lua 中使用的控件名字")]
    public string widgetName;

    [Tooltip("控件在预制体中的相对路径（例如 'Bg/ProgressBar'）")]
    public string widgetPath;

    [Tooltip("组件类型全名，例如 'Slider, Text'")]
    public string componentType;

    private const string UiNamespace = "UnityEngine.UI";

    public string FullComponentType
    {
        get
        {
            if (string.IsNullOrWhiteSpace(componentType))
                return string.Empty;
            return $"{UiNamespace}.{componentType}";
        }
    }
}
