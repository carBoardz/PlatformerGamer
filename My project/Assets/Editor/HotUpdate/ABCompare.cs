using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ABCompare
{
    static string ABResPath = "ABOutput";
    static string OutputPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "ABOutput\\PC");
    const string FileName = "ABcompareTempInfo.bytes";
    [MenuItem("AB包工具/创建对比文件")]
    public static void CreatABCompareFile()
    {
        //获取文件夹信息
        List<DirectoryInfo> directories = GetAllDirectories(ABResPath);
        StringBuilder abCompareInfo = new StringBuilder();
        foreach (var directory in directories)
        {
            FileInfo[] fileInfos = directory.GetFiles();
            foreach (FileInfo fileInfo in fileInfos)
            {
                if (string.IsNullOrEmpty(fileInfo.Extension) || fileInfo.Extension == ".bytes")
                {
                    string md5 = GetMD5(fileInfo.FullName);
                    abCompareInfo.Append($"{fileInfo.Name}|{fileInfo.Length}|{md5}|");
                }
            }
        }
        string res = abCompareInfo.ToString().TrimEnd('|');
        string savePath = Path.Combine(OutputPath, FileName);

        File.WriteAllText(savePath, res);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("成功", "对比文件生成完毕！", "喵！");
    }
    public static string GetMD5(string filePath)
    {
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] md5Info = md5.ComputeHash(fileStream);
            fileStream.Close();
            StringBuilder sb = new StringBuilder();
            int n = md5Info.Length;
            for (int i = 0; i < n; i++)
            {
                sb.Append(md5Info[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
    static List<DirectoryInfo> GetAllDirectories(string rootDir)
    {
        List<DirectoryInfo> dirs = new List<DirectoryInfo>();
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string fullPath = Path.Combine(projectRoot, rootDir);

        DirectoryInfo root = new DirectoryInfo(fullPath);
        dirs.Add(root);
        dirs.AddRange(root.GetDirectories("*", SearchOption.AllDirectories));
        return dirs;
    }
}
