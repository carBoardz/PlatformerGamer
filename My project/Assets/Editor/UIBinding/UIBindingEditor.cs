
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;

public class UIBindingEditor : EditorWindow
{
    static string directoryPath = "Assets/Resource/HotRes/Date/SO/UI";

    static string OutputDirectoryPath = "Assets/Prefab/ui/UIRoot";

    [MenuItem("Assets/UI/生成绑定配置", false, 100)]
    [Tooltip("配置文件生成在目录Assets/Resource/HotRes/Date/SO/UI/")]
    static void GenerateBindingConfig()
    {
        //确保UIPrefab的路径存在
        if (!AssetDatabase.IsValidFolder(OutputDirectoryPath))
        {
            EditorUtility.DisplayDialog("错误", $"请先创建文件路径 {OutputDirectoryPath} 并存入预制UI panel", "喵~");
            return;
        }

        List<GameObject> Prefabs = new();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { OutputDirectoryPath });
        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if(prefab != null) Prefabs.Add(prefab);
        }

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
                if (chil == prefab.transform) continue;

                string path = GetRelativePath(prefab.transform, chil);

                Component[] uiComponents = chil.GetComponents<Component>()
                    .Where(c => c != null && c.GetType().Namespace == "UnityEngine.UI")
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
}
