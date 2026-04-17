using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling.Memory.Experimental;

public class Lua_ConfigCopyEditor : Editor
{
    private static string LuaSourceRoot => Path.Combine(Application.dataPath, "Resource/HotRes/Lua");
    private static string ConfigSourceRoot => Path.Combine(Application.dataPath, "Resource/HotRes/Date");
    private static string LuaOutputRoot => Path.Combine(Application.dataPath, "Resource/HotRes/bytes/LuaBytes");
    private static string ConfigOutputRoot => Path.Combine(Application.dataPath, "Resource/HotRes/bytes/ConfigBytes");
    private const string LuaBundleName = "luaassets";
    private const string ConfigBundleName = "configassets";
    #region 编辑器设置
    [MenuItem("XLua/自动生成bytes后缀的Lua")]
    public static void PackLuaToBytes()
    {
        if (!Directory.Exists(LuaSourceRoot))
        {
            Debug.LogError($"Lua 源目录不存在：{LuaSourceRoot}");
            return;
        }

        if (!Directory.Exists(LuaOutputRoot))
        {
            Directory.CreateDirectory(LuaOutputRoot);
        }
        else
        {
            Directory.Delete(LuaOutputRoot, true);
            File.Delete(LuaOutputRoot + ".meta");
            AssetDatabase.Refresh();
        }

        List<string> allLuaFiles = new List<string>();
        CollectLuaFiles(LuaSourceRoot, allLuaFiles);
        Debug.Log($"找到 {allLuaFiles.Count} 个 Lua 文件");

        List<string> outputFilePaths = new List<string>();
        foreach (var sourcePath in allLuaFiles)
        {
            string outputPath = ConvertAndCopyLuaFile(sourcePath);
            if (!string.IsNullOrEmpty(outputPath))
            {
                outputFilePaths.Add(outputPath);
            }
        }
        AssetDatabase.Refresh();
        System.Threading.Thread.Sleep(50);

        // 统一设置 AB 包名
        int bundleCount = 0;
        foreach (string unityRelativePath in outputFilePaths)
        {
            AssetImporter importer = AssetImporter.GetAtPath(unityRelativePath);
            if (importer != null)
            {
                importer.assetBundleName = LuaBundleName;
                importer.assetBundleVariant = "";
                bundleCount++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"完成！共转换 {outputFilePaths.Count} 个文件，{bundleCount} 个文件已设置 AB 包名：{LuaBundleName}");
    }

    [MenuItem("XLua/自动生成bytes后缀的Config")]
    public static void PackConfigToBytes()
    {
        if (!Directory.Exists(ConfigSourceRoot))
        {
            Debug.LogError($"Config 源目录不存在：{ConfigSourceRoot}");
            return;
        }

        if (!Directory.Exists(ConfigOutputRoot))
        {
            Directory.CreateDirectory(ConfigOutputRoot);
        }
        else
        {
            Directory.Delete(ConfigOutputRoot,true);
            string metaPath = ConfigOutputRoot + ".meta";
            if (File.Exists(metaPath)) File.Delete(metaPath);
            AssetDatabase.Refresh();
        }

        List<string> allBytesFiles = new();
        CollectConfigFiles(ConfigSourceRoot, allBytesFiles);
        Debug.Log($"找到 {allBytesFiles.Count} 个 Config 文件");

        List<string> outputFilePaths = new List<string>();
        foreach (var configFile in allBytesFiles)
        {
            string outputPath = ConvertAndCopyConfigFile(configFile);
            if (!string.IsNullOrEmpty(outputPath))
            {
                outputFilePaths.Add(outputPath);
            }
        }
        AssetDatabase.Refresh();

        //给所有转换为bytes的config的AB包设置成ConfigBundleName
        int bundleCount = 0;
        foreach (var unityRelativePath in outputFilePaths)
        {
            AssetImporter importer = AssetImporter.GetAtPath(unityRelativePath);
            if (importer != null)
            {
                importer.assetBundleName = ConfigBundleName;
                importer.assetBundleVariant = "";
                //importer.assetBundleAssetName = "playerdate";
                bundleCount++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"完成！共转换 {outputFilePaths.Count} 个文件，{bundleCount} 个文件已设置 AB 包名：{ConfigBundleName}");
    }

    [MenuItem("XLua/清空 Lua AB 包设置")]
    public static void ClearLuaBundleName()
    {
        if (!Directory.Exists(LuaOutputRoot)) return;

        string[] files = Directory.GetFiles(LuaOutputRoot, "*.bytes", SearchOption.AllDirectories);
        foreach (string filePath in files)
        {
            string assetPath = "Assets" + filePath.Substring(Application.dataPath.Length);
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null)
            {
                importer.assetBundleName = "";
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Lua AB 包设置已清空");
    }
    #endregion

    /// <summary>
    /// 获取目标路径下的所有Lua脚本
    /// </summary>
    /// <param name="rootDir">目标路径</param>
    /// <param name="result">结果</param>
    static void CollectLuaFiles(string rootDir, List<string> result)
    {
        if (!Directory.Exists(rootDir))
        {
            Debug.LogError($"不存在{rootDir}根目录");
            return;
        }

        string[] files = Directory.GetFiles(rootDir, "*.lua");
        result.AddRange(files);

        string[] subDirs = Directory.GetDirectories(rootDir);
        foreach (var subDir in subDirs)
        {
            CollectLuaFiles(subDir,result);
        }
    }
    /// <summary>
    /// 获取目标路径下的所有Config
    /// </summary>
    /// <param name="rootDir">目标路径</param>
    /// <param name="result">结果</param>
    static void CollectConfigFiles(string rootDir, List<string> result)
    {
        if (!Directory.Exists(rootDir))
        {
            Debug.LogError($"不存在{rootDir}根目录");
            return;
        }

        string[] files = Directory.GetFiles(rootDir, "*.json", SearchOption.AllDirectories);
        result.AddRange(files);

        string[] subDirs = Directory.GetDirectories(rootDir);
        foreach (var subDir in subDirs)
        {
            CollectConfigFiles(subDir,result);
        }
    }
    /// <summary>
    /// 转换单个 Lua 文件为 .bytes 并复制到输出目录（保留目录结构）
    /// </summary>
    static string ConvertAndCopyLuaFile(string sourcePath)
    {
        try
        {
            // 生成输出路径：xxx.lua -> xxx.lua.bytes
            string relativePath = sourcePath.Substring(LuaSourceRoot.Length + 1);
            string outputFileName = Path.GetFileNameWithoutExtension(sourcePath) + ".lua.bytes";

            string outputSubDir = Path.GetDirectoryName(relativePath);
            string outputDir = string.IsNullOrEmpty(outputSubDir)? LuaOutputRoot : Path.Combine(LuaOutputRoot, outputSubDir);

            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            string outputPath = Path.Combine(outputDir, outputFileName);
            string unityRelativePath = outputPath.Replace(Application.dataPath, "Assets");
            unityRelativePath = unityRelativePath.Replace("\\","/");

            byte[] luaByte = File.ReadAllBytes(sourcePath);
            string luaContent = Encoding.UTF8.GetString(luaByte);//读取
            File.WriteAllText(outputPath, luaContent, new UTF8Encoding(false));//写入

            AssetDatabase.ImportAsset(unityRelativePath, ImportAssetOptions.ForceUpdate);//导入到unity中

            return unityRelativePath;
        }
        catch (Exception ex)
        {
            Debug.LogError($"转换文件失败：{sourcePath}\n{ex.Message}");
            return null;
        }
    }
    /// <summary>
    /// 转换单个 Config 文件为 .bytes 并复制到输出目录（保留目录结构）
    /// </summary>
    static string ConvertAndCopyConfigFile(string sourcePath)
    {
        try
        {
            string relativePath = sourcePath.Substring(ConfigSourceRoot.Length + 1);
            string outputFileName = Path.GetFileNameWithoutExtension(sourcePath) + ".json.bytes";

            string outputSubDir = Path.GetDirectoryName(relativePath);
            string outputDir = string.IsNullOrEmpty(outputSubDir) ? ConfigOutputRoot : Path.Combine(ConfigOutputRoot, outputSubDir);//判断并返回是否是子目录

            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            string outputPath = Path.Combine(outputDir, outputFileName);
            string unityRelativePath = outputPath.Replace(Application.dataPath, "Assets");
            unityRelativePath = unityRelativePath.Replace("\\","/");

            byte[] jsonBytes = File.ReadAllBytes(sourcePath);
            string jsonContent = Encoding.UTF8.GetString(jsonBytes);//读取
            File.WriteAllText(outputPath, jsonContent, new UTF8Encoding(false));//写入

            AssetDatabase.ImportAsset(unityRelativePath, ImportAssetOptions.ForceUpdate);

            return unityRelativePath;
        }
        catch (Exception e)
        {
            Debug.LogError($"转换文件失败：{sourcePath}\n{e.Message}");
            return null;
        }
    }
}
