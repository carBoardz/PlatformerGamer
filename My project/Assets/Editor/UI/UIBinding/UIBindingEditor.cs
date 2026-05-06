
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Connect;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;

public class UIBindingEditor : EditorWindow
{
    static string directoryPath = "Assets/Resource/HotRes/Date/SO/UI/UIBindingSO/";

    static string obtainDirectoryPath = "Assets/Prefab/ui/UIRoot";

    static string LuaBindGeneratePath = "Assets/Resource/HotRes/Lua/UI/Binding";

    [MenuItem("Assets/UI/生成绑定配置", false, 100)]
    [Tooltip("配置文件生成在目录Assets/Resource/HotRes/Date/SO/UI/UIBindingSO/")]
    static void GenerateBindingConfig()
    {
        //确保UIPrefab的路径存在
        if (!AssetDatabase.IsValidFolder(obtainDirectoryPath))
        {
            EditorUtility.DisplayDialog("错误", $"请先创建文件路径 {obtainDirectoryPath} 并存入预制UI panel", "喵~");
            return;
        }

        List<GameObject> Prefabs = LoadAllAssets<GameObject>(obtainDirectoryPath);

        //确保生成SO的路径存在
        if (!AssetDatabase.IsValidFolder(directoryPath))
        {
            CheckAndCreateFolder();
            AssetDatabase.Refresh();
        }
        
        foreach (var prefab in Prefabs)
        {
            string fullAssetPath = directoryPath + $"/{prefab.name}.asset";
            //获取LuaBindingCollector
            LuaBindingCollector collector = AssetDatabase.LoadAssetAtPath<LuaBindingCollector>(fullAssetPath);
            if (collector == null)
            {
                //创建so
                collector = ScriptableObject.CreateInstance<LuaBindingCollector>();
                collector.name = collector.name + "Binding";
                collector.uiName = prefab.name;
                collector.bindings = new List<WidgetBinding>();
            }
            else
            {
                collector.bindings.Clear();
            }

            //遍历节点获取Component
            Transform[] allTransformsInPrefab = prefab.GetComponentsInChildren<Transform>(true);
            foreach (var chil in allTransformsInPrefab)
            {
                string path;
                if (chil == prefab.transform)
                {
                    path = "";   // 或 path = chil.name;
                }
                else
                {
                    path = GetRelativePath(prefab.transform, chil);
                }
                
                Component[] uiComponents = chil.GetComponents<Component>()
                    .Where(c => c != null && c.GetType().Namespace == "UnityEngine.UI" || c.GetType().Namespace == "TMPro")
                    .ToArray();

                foreach (Component comp in uiComponents)
                {
                    WidgetBinding bind = new WidgetBinding()
                    {
                        widgetName = $"{chil.name}_{comp.GetType().Name}",
                        widgetPath = path,
                        componentType = comp.GetType().Name,
                    };

                    collector.bindings.Add(bind);
                }
            }

            // 保存
            if (AssetDatabase.LoadAssetAtPath<LuaBindingCollector>(fullAssetPath) == null)
            {
                AssetDatabase.CreateAsset(collector, fullAssetPath);
            }
            else
            {
                // 如果已存在，需要编辑并标记为脏，或者直接覆盖
                EditorUtility.CopySerialized(collector, AssetDatabase.LoadAssetAtPath<LuaBindingCollector>(fullAssetPath));
            }

            EditorUtility.SetDirty(collector);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        GenerateLuaAutoBindFile();
        EditorUtility.DisplayDialog("成功", $"绑定配置已生成", "喵~");
    }
    /// <summary>
    /// 检测文件是否存在并创建
    /// </summary>
    /// <param name="parentPath">父级文件路径</param>
    /// <param name="folderName">目标文件名</param>
    /// <returns></returns>
    public static void CreateFolderIfNotExist(string parentPath, string folderName)
    {
        string path = parentPath + "/" + folderName;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parentPath, folderName);
        }
    }
    static void CheckAndCreateFolder()
    {
        // 一级一级来
        CreateFolderIfNotExist("Assets", "Resource");
        CreateFolderIfNotExist("Assets/Resource", "HotRes");
        CreateFolderIfNotExist("Assets/Resource/HotRes", "Date");
        CreateFolderIfNotExist("Assets/Resource/HotRes/Date", "SO");
        CreateFolderIfNotExist("Assets/Resource/HotRes/Date/SO", "UI");
        CreateFolderIfNotExist("Assets/Resource/HotRes/Date/SO/UI", "UIBindingSO");
    }
    /// <summary>
    /// 获取相对于根的路径
    /// </summary>
    /// <param name="rootTransform">根Transform</param>
    /// <param name="chilTransform">要获取的相对于根的路径的Transform</param>
    /// <returns></returns>
    public static string GetRelativePath(Transform rootTransform, Transform chilTransform)
    {
        string path = "";
        while (chilTransform != rootTransform && chilTransform != null)
        {
            path = (path == "") ? chilTransform.name : chilTransform.name + "/" + path;
            chilTransform = chilTransform.parent;
        }
        return path;
    }
    /// <summary>
    /// 根据绑定配置自动生成 Lua 自动绑定脚本
    /// </summary>
    public static void GenerateLuaAutoBindFile()
    {
        if (!AssetDatabase.IsValidFolder(LuaBindGeneratePath))
        {
            string parentDir = Path.GetDirectoryName(LuaBindGeneratePath);
            string folderName = Path.GetFileName(LuaBindGeneratePath);
            AssetDatabase.CreateFolder(parentDir, folderName);
        }

        List<ScriptableObject> configs = LoadAllAssets<ScriptableObject>(directoryPath);  
        foreach (LuaBindingCollector Collector in configs)
        {
            string uiName = Collector.uiName;
            string luaFilePath = LuaBindGeneratePath + "/" + uiName + "_AutoBind.lua";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"-- 此文件由 UIBindingEditor 自动生成，请勿手动修改");
            sb.AppendLine($"local M = {{}}");
            sb.AppendLine();
            sb.AppendLine($"function M:AutoBind(view)");
            foreach (var widget in Collector.bindings)
            {
                string varName = char.ToLower(widget.widgetName[0]) + widget.widgetName.Substring(1);
                sb.AppendLine($"self.{varName} = view:GetWidget(\"{widget.componentType}\")");
            }
            sb.AppendLine("end");
            sb.AppendLine();
            sb.AppendLine($"return M");

            File.WriteAllText(luaFilePath, sb.ToString(), System.Text.Encoding.UTF8);
        }

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 返回该目录下所有指定类型的资源
    /// </summary>
    /// <typeparam name="T">指定类型</typeparam>
    /// <param name="path">目标文件根目录</param>
    /// <returns></returns>
    public static List<T> LoadAllAssets<T>(string path) where T : Object
    {
        List<T> assets = new();
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { path });
        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null) assets.Add(asset);
        }
        return assets;
    }
}
