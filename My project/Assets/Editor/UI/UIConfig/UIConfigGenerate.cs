using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

public class UIConfigGenerate : EditorWindow
{
    const string PrefabDir = "Assets/Prefab/ui/UIRoot/";
    const string OutPutDir = "Assets/Resource/HotRes/Date/SO/UI/UIConfigSO/";
    const string assetName = "/UISOConfigs.asset";
    [MenuItem("Assets/UI/生成UI配置信息", false, 100)]
    public static void GenerateUIConfig()
    {
        //检查路径是否存在
        if (!AssetDatabase.IsValidFolder(PrefabDir))
        {
            EditorUtility.DisplayDialog("错误", $"请先创建文件路径 {PrefabDir} 并存入预制UI panel", "喵~");
            return;
        }

        //获取PrefabDir下的所有GameObject
        List<GameObject> Prefabs = new();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabDir });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) Prefabs.Add(prefab);
        }

        //检查路径是否存在
        if (!AssetDatabase.IsValidFolder(OutPutDir))
        {
            CheckAndCreateFolder();
            AssetDatabase.Refresh();
        }

        string fullAssetPath = OutPutDir + assetName;
        UISOConfigs UISOConfigsCollector = AssetDatabase.LoadAssetAtPath<UISOConfigs>(fullAssetPath);

        if (UISOConfigsCollector == null)
        {
            UISOConfigsCollector = ScriptableObject.CreateInstance<UISOConfigs>();
            UISOConfigsCollector.allUIConfigs = new();
        }
        else
        {
            UISOConfigsCollector.allUIConfigs.Clear();
        }

        //添加
        foreach (var prefab in Prefabs)
        {
            string panelName = prefab.name;

            string assetPath = AssetDatabase.GetAssetPath(prefab);
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            string abName = importer?.assetBundleName ?? "";

            UIConfigItem newItem = new()
            {
                uiName = panelName,
                abName = abName,
                controller = $"UI.{panelName}Controller",
                bindingConfig = $"{panelName}Binding"
            };
            UISOConfigsCollector.allUIConfigs.Add(newItem);
        }

        if (AssetDatabase.LoadAssetAtPath<LuaBindingCollector>(fullAssetPath) == null)
        {
            AssetDatabase.CreateAsset(UISOConfigsCollector, fullAssetPath);
        }
        else
        {
            // 如果已存在，需要编辑并标记为脏，或者直接覆盖
            EditorUtility.CopySerialized(UISOConfigsCollector, AssetDatabase.LoadAssetAtPath<LuaBindingCollector>(fullAssetPath));
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("成功", $"uiPanel配置已生成", "喵~");
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
        CreateFolderIfNotExist("Assets/Resource/HotRes/Date/SO/UI", "UIConfigSO");
    }
}