using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class UpLoadAB : MonoBehaviour
{

    [MenuItem("AB包工具/上传热更文件")]
    public static void UpLoadFile()
    {
        DirectoryInfo dir = new DirectoryInfo(Application.dataPath);
        FileInfo[] fileInfos = dir.GetFiles();
        int n = fileInfos.Length;
        foreach (var fileInfo in fileInfos)
        {
            if (fileInfo.Extension == "" || fileInfo.Extension == ".txt")
            {
                UpLoadRes(fileInfo.FullName,fileInfo.Name);
            }
        }
    }
    public async static void UpLoadRes(string FullName, string fileName)
    {
        await Task.Run(() =>
        {
            try
            {
                HttpWebRequest req = HttpWebRequest.Create(new Uri("http://26.166.242.49:8000/" + "/AB/PC/" + fileName)) as HttpWebRequest;
                req.Method = "GET";
                HttpWebResponse res = req.GetResponse() as HttpWebResponse;
                Stream targetStream1 = res.GetResponseStream();
                using (FileStream upLoadStream = new FileStream(FullName, FileMode.Open))
                {
                    byte[] buffer = new byte[4096];
                    int contentLength = upLoadStream.Read(buffer, 0, buffer.Length);
                    while (contentLength != 0)
                    {
                        targetStream1.Write(buffer, 0, contentLength);
                        contentLength = upLoadStream.Read(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        });
    }
}
