using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ABCompare
{
    static string ABResPath = "\\Resource\\ABRes\\";
    [MenuItem("AB包工具/创建对比文件")]
    public static void CreatABCompareFile()
    {
        //获取文件夹信息
        DirectoryInfo directory = new DirectoryInfo(Application.dataPath + ABResPath);
        FileInfo[] fileInfos = directory.GetFiles();
        string abCompareInfo = "";
        foreach (FileInfo fileInfo in fileInfos)
        {
            if (fileInfo.Extension == "")
            {
                abCompareInfo += fileInfo.Name + "" + fileInfo.Length + "" + GetMD5(fileInfo.FullName);
                abCompareInfo += "|";
            }
        }
        abCompareInfo = abCompareInfo.Substring(0,ABResPath.Length - 1);
        File.WriteAllText(Application.dataPath + ABResPath + "ABcompareInfo.txt", abCompareInfo);
        AssetDatabase.Refresh();
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
}
