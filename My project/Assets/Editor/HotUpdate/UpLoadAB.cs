using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class UpLoadAB
{
    static string GeneratePath = "ABOutput";
    private static readonly HttpClient httpClient = new HttpClient()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    [MenuItem("AB包工具/上传热更文件 - 将Asset同级的ABOutput里面的内容上传至远端资源服务器")]
    public static async void UpLoadFile()
    {
        List<DirectoryInfo> dirs = GetAllDirectories(GeneratePath);
        List<Task> uploadTasks = new List<Task>();

        // 限制同时上传的连接数，避免占用过多资源
        SemaphoreSlim semaphore = new SemaphoreSlim(4);

        foreach (var dir in dirs)
        {
            FileInfo[] fileInfos = dir.GetFiles();
            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.Extension == "" || fileInfo.Extension == ".bytes")
                {
                    var task = UpLoadRes(fileInfo.FullName, fileInfo.Name, semaphore);
                    uploadTasks.Add(task);
                }
            }
        }
        await Task.WhenAll(uploadTasks);
        EditorUtility.DisplayDialog("成功", "文件上传远端完毕！", "喵！");
    }
    public static async Task UpLoadRes(string FullName, string fileName, SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();
        string url = "http://localhost/ABRes/" + fileName;
        string localHash = ComputeFileSha256(FullName);

        bool success = false;
        Exception caughtException = null;

        //检测资源是否需要上传或更新
        bool needUpload = true;
        try
        {
            using (var headRequest = new HttpRequestMessage(HttpMethod.Head, url))
            using (var headResponse = await httpClient.SendAsync(headRequest))
            {
                if (headResponse.IsSuccessStatusCode)
                {
                    // 远端存在该文件，尝试获取其哈希（优先用 ETag，其次用 Content-MD5）
                    string remoteHash = GetHashFromResponse(headResponse);
                    if (!string.IsNullOrEmpty(remoteHash) && remoteHash.Equals(localHash, StringComparison.OrdinalIgnoreCase))
                    {
                        needUpload = false;
                        Debug.Log($"跳过（远端内容相同）：{fileName}");
                    }
                    else
                    {
                        Debug.Log($"内容不同，即将上传：{fileName}");
                    }
                }
                else
                {
                    Debug.Log($"远端无此文件，准备上传：{fileName}");
                }
            }
        }
        catch (HttpRequestException)
        {
            // 如果 HEAD 失败（比如 404 或服务器不支持），认为需要上传
            Debug.Log($"无法获取远端文件信息，尝试直接上传：{fileName}");
        }

        try
        {
            if (needUpload)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                        req.Method = "PUT";
                        // 可选：添加 Headers，例如带上本地哈希供下次比较
                        // req.Headers["If-None-Match"] = $"\"{localHash}\"";

                        using (Stream targetStream = req.GetRequestStream())
                        using (FileStream sourceStream = new FileStream(FullName, FileMode.Open, FileAccess.Read))
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead;
                            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                targetStream.Write(buffer, 0, bytesRead);
                            }
                        }

                        using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
                        {
                            if (res.StatusCode != HttpStatusCode.OK && res.StatusCode != HttpStatusCode.Created)
                            {
                                throw new Exception($"上传失败，状态码: {res.StatusCode}");
                            }
                            Debug.Log($"上传成功：{fileName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"上传文件异常 {fileName}：{ex.Message}");
                        // 如果需要，可以在这里决定是否要抛出，或只记录
                    }
                });
            }
            else
            {
                Debug.Log($"跳过（内容相同）：{fileName}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"处理文件异常 {fileName}：{ex.Message}");
        }
        finally
        {
            semaphore.Release();
        }
    }
    static string ComputeFileSha256(string filePath)
    {
        using (var stream = File.OpenRead(filePath))
        using (var sha = SHA256.Create())
        {
            byte[] hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
    private static string GetHashFromResponse(HttpResponseMessage response)
    {
        // ETag 通常带双引号，如 "abc123"，去掉引号
        if (response.Headers.ETag != null && !string.IsNullOrEmpty(response.Headers.ETag.Tag))
        {
            return response.Headers.ETag.Tag.Trim('\"');
        }

        if (response.Content.Headers.ContentMD5 != null)
        {
            return Convert.ToBase64String(response.Content.Headers.ContentMD5);
        }

        return null;
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