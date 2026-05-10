using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using static UnityEngine.GridBrushBase;
using MySinleton;

namespace Tool.MyAB
{
    /// <summary>
    /// AB包更新管理器（纯下载逻辑，解耦）
    /// 职责：下载对比文件 → 对比版本 → 下载需要更新的AB包到persistentPath
    /// </summary>
    public class ABUpdateManager : SingletonMono<ABUpdateManager>
    {
        //远端资源
        Dictionary<string, ABInfo> remoteABInfo = new Dictionary<string, ABInfo>();
        //本地资源
        Dictionary<string, ABInfo> localABInfo = new Dictionary<string, ABInfo>();
        //待下载列表
        List<string> downLoadList = new List<string>();
        public long totalBytes;
        public long downloadedBytes;

        // 服务器基础地址
        private string _serverBaseUrl = "http://localhost/ABRes/";
        // 本地热更目录
        string _persistentABPath;
        string _streamingABPath;

        string _localComparePath => "ABcompareInfo.bytes";
        string _remoteComparePath => "ABcompareInfo.bytes";
        //主线程回调队列
        readonly Queue<Action> _mainThreadActions = new Queue<Action>();

        protected override void Awake()
        {
            base.Awake();
            _persistentABPath = Path.Combine(Application.persistentDataPath, "ABRes");
            _streamingABPath = Path.Combine(Application.streamingAssetsPath, "ABRes");
            // 初始化热更目录
            if (!Directory.Exists(_persistentABPath))
            {
                Directory.CreateDirectory(_persistentABPath);
                Debug.Log($"初始化热更目录：{_persistentABPath}");
            }
        }

        #region DownLoad 下载
        /// <summary>
        /// 下载单个文件（新增进度回调）
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="progressCallback">单文件下载进度回调（已下载字节/总字节/进度百分比）</param>
        /// <returns>是否下载成功</returns>
        bool DownLoadFild(string fileName, UnityAction<long, long, float> progressCallback = null)
        {
            try
            {
                string fileUrl = $"{_serverBaseUrl}{fileName}";
                string savePath = Path.Combine(_persistentABPath, fileName);

                HttpWebRequest req = HttpWebRequest.Create(new Uri(fileUrl)) as HttpWebRequest;
                req.Method = "GET";
                req.Timeout = 3000;
                req.UserAgent = "Unity ABUpdate";
                using (HttpWebResponse res = req.GetResponse() as HttpWebResponse)
                {
                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        long fileTotalBytes = res.ContentLength;

                        using (Stream downlodeStream = res.GetResponseStream())
                        using (FileStream fileStream = File.Create(savePath))
                        {
                            byte[] buffer = new byte[8192];
                            int contentLength = 0;
                            while ((contentLength = downlodeStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fileStream.Write(buffer, 0, contentLength);
                                //获取下载进度
                                downloadedBytes += contentLength;
                                float DownLoadProgress = totalBytes <= 0 ? 0 : (float)downloadedBytes / totalBytes;

                                if (progressCallback != null)
                                {
                                    lock (_mainThreadActions)
                                    {
                                        _mainThreadActions.Enqueue(() =>
                                        {
                                            progressCallback.Invoke(downloadedBytes, totalBytes, DownLoadProgress);
                                        });
                                    }
                                }

                                Thread.Sleep(1);// 让出线程，避免卡死
                            }
                        }
                        // 下载完成，进度100%
                        if (progressCallback != null)
                        {
                            lock (_mainThreadActions)
                            {
                                _mainThreadActions.Enqueue(() =>
                                {
                                    progressCallback.Invoke(totalBytes, totalBytes, 1f);
                                });
                            }
                        }
                        return true;
                    }
                    else
                    {
                        print(fileName + "下载失败" + res.StatusCode);
                        return false;
                    }
                }

            }
            catch (WebException e)
            {
                Debug.Log(fileName + "下载出错" + e.Status + e.Message);
                return false;
            }
        }
        public async void DownLoadCompareFile(UnityAction<bool> overCallback = null)
        {
            bool isSuccess = false; int retryCount = 5;
            while (!isSuccess && retryCount > 0)
            {
                await Task.Run(() =>
                {
                    isSuccess = DownLoadFild(_remoteComparePath);
                });
                --retryCount;

                if (!isSuccess)
                {
                    Debug.LogWarning($"下载对比文件失败，剩余重试次数：{retryCount}");
                    await Task.Delay(1000); // 重试前等待1秒，避免频繁请求
                }
            }
            if (isSuccess)
                Debug.Log("<color=green>对比文件下载成功！</color>");
            overCallback?.Invoke(isSuccess);
        }
        /// <summary>
        /// 下载待下载列表的资源
        /// </summary>
        /// <param name="overCallback">下载完成回调</param>
        /// <param name="DownloadProgress">下载进度回调</param>
        public async void DownLoadABFile(UnityAction<bool> overCallback = null, UnityAction<long,long,float> progressCallback = null)
        {
            //检测对比本地资源，添加到待下载列表
            if (downLoadList.Count == 0)
            {
                Debug.Log("所有AB包已是最新，无需下载");
                overCallback?.Invoke(true);
                return;
            }
            //下载待下载列表中的资源
            int totalFileCount = downLoadList.Count;
            int waitToDownLoadCount = totalFileCount;
            bool isFileSuccess = false; bool allSuccess = true;

            int completedFileCount = 0; // 已完成文件数
            int retryCount = 5;//重新下载次数
            List<string> HaveBeenDownLoaded = new List<string>();

            while (waitToDownLoadCount > 0 && retryCount > 0)
            {
                isFileSuccess = false;
                for (int i = 0; i < waitToDownLoadCount; i++)
                {
                    await Task.Run(() =>
                    {
                        isFileSuccess = DownLoadFild(downLoadList[i], (downloadedBytes, totalBytes, DownLoadProgress) =>
                        {
                            progressCallback?.Invoke(downloadedBytes, totalBytes, DownLoadProgress);
                        });
                    });
                    if (isFileSuccess)
                    {
                        HaveBeenDownLoaded.Add(downLoadList[i]);
                        continue;
                    }
                    // 主线程更新完成数
                    lock (_mainThreadActions)
                    {
                        _mainThreadActions.Enqueue(() =>
                        {
                            if (isFileSuccess)
                            {
                                completedFileCount++;
                                Debug.Log($"{downLoadList[i]}下载完成，已完成{completedFileCount}/{totalFileCount}");
                            }
                            else
                            {
                                allSuccess = false;
                            }
                        });
                    }
                    await Task.Delay(200);
                }
                int size1 = HaveBeenDownLoaded.Count;
                for (int i = 0; i < size1; i++)
                {
                    downLoadList.Remove(HaveBeenDownLoaded[i]);
                }
                waitToDownLoadCount = downLoadList.Count;
                retryCount--;
            }
            if (waitToDownLoadCount != 0)
            {
                Debug.Log("有待下载资源未下载完成");
            }
            overCallback?.Invoke(allSuccess);
        }
        #endregion
        #region Parse解析
        public async Task CheckUpdate(UnityAction<bool> callback = null, UnityAction<long,long,float> progressCallback = null)
        {
            string localComparePath = Path.Combine(_streamingABPath, _localComparePath);
            if (Application.platform == RuntimePlatform.Android)
            {
                localComparePath = $"jar:file://{localComparePath}"; // 安卓jar路径
            }
            string remoteComparePath = Path.Combine(_persistentABPath, _remoteComparePath);

            await ReadCompareFileContent(remoteComparePath, async (success, remoteContent) =>
            {
                if (success && !string.IsNullOrEmpty(remoteContent))
                {
                    bool isRemoteParseSuccess = ParseCompareContent(remoteContent, remoteABInfo);
                    if (!isRemoteParseSuccess)
                    {
                        callback?.Invoke(false);
                        return;
                    }

                    await ReadCompareFileContent(localComparePath, (success, localContent) =>
                    {
                        if (success && !string.IsNullOrEmpty(localContent))
                        {
                            bool isLocalParseSuccess = ParseCompareContent(localContent, localABInfo);
                            if (!isLocalParseSuccess)
                            {
                                callback?.Invoke(false);
                                return;
                            }
                            //添加资源到下载列表
                            downLoadList.Clear();
                            foreach (string name in remoteABInfo.Keys)
                            {
                                if (!localABInfo.ContainsKey(name))
                                {
                                    downLoadList.Add(name);
                                    totalBytes += remoteABInfo[name].size;
                                }
                                else if (localABInfo[name].md5 != remoteABInfo[name].md5)
                                {
                                    downLoadList.Add(name);
                                    localABInfo.Remove(name);
                                    totalBytes += remoteABInfo[name].size;
                                }
                            }
                            CleanObsoleteRes();
                            //下载更新列表文件
                            DownLoadABFile((DownLoadSuccess) =>
                            {
                                if (DownLoadSuccess)
                                {
                                    //下载成功，更新对比文件
                                    File.WriteAllText(localComparePath, remoteContent);
                                }
                                callback?.Invoke(DownLoadSuccess);
                            }, (downloadedBytes, totalBytes, DownLoadProgress) =>
                            {
                                progressCallback?.Invoke(downloadedBytes, totalBytes, DownLoadProgress);
                            });
                        }
                        else
                        {
                            Debug.LogError("未检测到远端对比文件");
                            callback?.Invoke(false);
                        }
                    });
                }
                else
                {
                    Debug.Log("未检测到远端对比文件,直接加载本地资源");
                    await ReadCompareFileContent(localComparePath, (success, localContent) =>
                    {
                        if (success)
                        {
                            bool isLocalParseSuccess = ParseCompareContent(localContent, localABInfo);
                            if (isLocalParseSuccess)
                            {
                                callback?.Invoke(true);
                                return;
                            }
                        }
                        else
                        {
                            Debug.LogError("加载本地资源出错");
                            callback?.Invoke(false);
                        }
                    });
                }
            });
        }
        /// <summary>
        /// 解析对比文件内容为ABInfo字典（通用方法，只解析一次）
        /// </summary>
        /// <param name="compareContent">对比文件文本内容</param>
        /// <param name="abInfoDict">输出的字典</param>
        /// <returns>是否解析成功</returns>
        bool ParseCompareContent(string compareContent, Dictionary<string, ABInfo> abInfoDict)
        {
            abInfoDict.Clear();
            if (string.IsNullOrEmpty(compareContent))
            {
                Debug.LogError("对比文件内容为空");
                return false;
            }
            try
            {
                string[] str = compareContent.Split('|'); int n = str.Length;
                for (int i = 0; i < n; i += 3)
                {
                    string name = str[i];
                    string length = str[i + 1];
                    string md5 = str[i + 2];
                    abInfoDict.Add(name, new ABInfo(name, length, md5));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("对比文件解析出错" + ex);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 读取对比文件内容
        /// </summary>
        /// <param name="filePath">文件路径（本地路径/jar路径）</param>
        /// <param name="callback">回调：是否成功 + 文件内容</param>
        async Task<bool> ReadCompareFileContent(string filePath, UnityAction<bool, string> callback)
        {
            string content = string.Empty;
            bool isSuccess = false;

            try
            {
                // 安卓streamingAssets：用UnityWebRequest读取
                if (Application.platform == RuntimePlatform.Android && filePath.StartsWith("jar:file://"))
                {
                    using (UnityWebRequest req = UnityWebRequest.Get(filePath))
                    {
                        await AsyncHealper.AwaitWebRequest(req);
                        if (req.result == UnityWebRequest.Result.Success)
                        {
                            content = req.downloadHandler.text;
                            isSuccess = true;
                        }
                        else
                        {
                            Debug.LogError($"安卓读取对比文件失败：{req.error}");
                        }
                    }
                }
                // 其他平台/本地文件：直接读取
                else
                {
                    if (File.Exists(filePath))
                    {
                        content = File.ReadAllText(filePath);
                        isSuccess = true;
                    }
                    else
                    {
                        Debug.LogError($"对比文件不存在：{filePath}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"读取对比文件异常：{e.Message}");
            }

            callback?.Invoke(isSuccess, content);
            return isSuccess;
        }
        /// <summary>
        /// 移除过时资源
        /// </summary>
        void CleanObsoleteRes()
        {
            if (!Directory.Exists(_persistentABPath)) return;
            foreach (var fileName in Directory.GetFiles(_persistentABPath))
            {
                string resName = Path.GetFileName(fileName);
                if (resName == _localComparePath) continue;
                if (!remoteABInfo.ContainsKey(resName))
                {
                    try
                    {
                        File.Delete(fileName);
                        Debug.Log($"清理过时资源：{resName}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"清理{resName}失败：{e.Message}");
                    }
                }
            }
        }
        #endregion
        class ABInfo
        {
            public string name;
            public long size;
            public string md5;

            public ABInfo(string name, string size, string md5)
            {
                this.name = name;
                this.size = long.Parse(size);
                this.md5 = md5;
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}