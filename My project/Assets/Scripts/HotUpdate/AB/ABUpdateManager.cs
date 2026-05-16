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
        string _remoteComparePathTemp => "ABcompareTempInfo.bytes";
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
        /// <param name="fileName">要下载的文件名</param>
        /// <param name="progressCallback">单文件下载进度回调（已下载字节/总字节/进度百分比）</param>
        /// <returns>是否下载成功</returns>
        bool DownLoadFild(string fileName, UnityAction<long, long, float> progressCallback = null)
        {
            string fileUrl = $"{_serverBaseUrl}{fileName}";
            string savePath = Path.Combine(_persistentABPath, fileName);
            try
            {
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
                if (File.Exists(savePath)) File.Delete(savePath);//删除可能因为网络波动而存在的损坏文件
                Debug.Log(fileName + "下载出错" + e.Status + e.Message);
                return false;
            }
        }
        public async Task<bool> DownLoadCompareFile()
        {
            bool isSuccess = false; int retryCount = 5;
            while (!isSuccess && retryCount > 0)
            {
                string wrongPath = Path.Combine(_persistentABPath, _remoteComparePathTemp);
                if (File.Exists(wrongPath)) File.Delete(wrongPath);

                await Task.Run(() =>
                {
                    isSuccess = DownLoadFild(_remoteComparePathTemp);
                });

                --retryCount;

                if (!isSuccess)
                {
                    if (File.Exists(wrongPath)) File.Delete(wrongPath);
                    Debug.LogWarning($"下载对比文件失败，剩余重试次数：{retryCount}");
                    await Task.Delay(1000); // 重试前等待1秒，避免频繁请求
                }
            }
            if (isSuccess)
                Debug.Log("<color=green>对比文件下载成功！</color>");
            return isSuccess;
        }
        /// <summary>
        /// 下载待下载列表的资源
        /// </summary>
        /// <param name="overCallback">下载完成回调</param>
        /// <param name="DownloadProgress">下载进度回调</param>
        public async Task<bool> DownLoadABFile(UnityAction<long,long,float> progressCallback = null)
        {
            //检测对比本地资源，添加到待下载列表
            if (downLoadList.Count == 0)
            {
                Debug.Log("[DownLoadABFile] 所有AB包已是最新，无需下载");
                return false;
            }
            //下载待下载列表中的资源
            int totalFileCount = downLoadList.Count;
            int waitToDownLoadCount = totalFileCount;

            int completedFileCount = 0; // 已完成文件数
            var failedFiles = new List<string>();

            //
            for (int i = 0; i < waitToDownLoadCount; i++)
            {
                int index = i;
                string fileName = downLoadList[index];
                int retryCount = 5;//重新下载次数
                bool fileSuccess = false;

                while (!fileSuccess && retryCount > 0)
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            Debug.Log($"[DownLoadABFile] 当前文件资源下载：{fileName}");
                            fileSuccess = DownLoadFild(fileName, (downloadedBytes, totalBytes, DownLoadProgress) =>
                            {
                                progressCallback?.Invoke(downloadedBytes, totalBytes, DownLoadProgress);
                            });
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[DownLoadABFile] 下载 {fileName} 发生未处理异常: {ex}");
                        fileSuccess = false;
                    }

                    if (fileSuccess)
                    {
                        completedFileCount++;
                        Debug.Log($"[DownLoadABFile] {fileName}下载完成，已完成{completedFileCount}/{totalFileCount}");
                        //HaveBeenDownLoaded.Add(downLoadList[i]);
                        continue;
                    }
                    else
                    {
                        retryCount--;
                        if (retryCount > 0)
                        {
                            Debug.LogWarning($"[DownLoadABFile] {fileName} 下载失败，剩余重试 {retryCount} 次");
                            await Task.Delay(500); // 退避
                        }
                        else
                        {
                            Debug.LogError($"[DownLoadABFile] {fileName} 下载最终失败，已重试耗尽");
                            failedFiles.Add(fileName);
                        }
                    }
                }
            }

            if (failedFiles.Count > 0)
            {
                Debug.LogError($"[DownLoadABFile] 以下文件下载失败: {string.Join(", ", failedFiles)}");
                return false;
            }

            Debug.Log("[DownLoadABFile] 所有文件下载完毕");
            return true;
        }
        #endregion
        #region Parse解析
        public async Task<bool> CheckUpdate(UnityAction<long,long,float> progressCallback = null)
        {
            string toBeCompared = null;
            string formalComparePath = Path.Combine(_persistentABPath, _remoteComparePath);
            if (File.Exists(formalComparePath))
            {
                toBeCompared = formalComparePath;
            }
            else
            {
                string localComparePath = Path.Combine(_streamingABPath, _localComparePath);
                toBeCompared = localComparePath;
                if (Application.platform == RuntimePlatform.Android)
                {
                    localComparePath = $"jar:file://{localComparePath}"; // 安卓jar路径
                    toBeCompared = localComparePath;
                }
            }
            
            string tempComparePath = Path.Combine(_persistentABPath, _remoteComparePathTemp);

            return await UpdateFile(tempComparePath, toBeCompared, progressCallback);
        }
        /// <summary>
        /// 更新资源
        /// </summary>
        /// <param name="remoteComparePath">下载的最新的对比文件</param>
        /// <param name="toBeCompared">待对比的文件</param>
        /// <param name="callback">下载成功回调</param>
        /// <param name="progressCallback">下载进度回调</param>
        /// <returns></returns>
        async Task<bool> UpdateFile(string remoteComparePath, string toBeCompared, UnityAction<long, long, float> progressCallback = null)
        {
            Debug.Log("[UpdateFile] 进入方法");
            var tcs = new TaskCompletionSource<bool>();

            await ReadCompareFileContent(remoteComparePath, async (success, remoteContent) =>
            {
                if (success && !string.IsNullOrEmpty(remoteContent))
                {
                    bool isRemoteParseSuccess = ParseCompareContent(remoteContent, remoteABInfo);
                    if (!isRemoteParseSuccess)
                    {
                        tcs.SetResult(false);
                        return;
                    }

                    await ReadCompareFileContent(toBeCompared, async (success, localContent) =>
                    {
                        if (success && !string.IsNullOrEmpty(localContent))
                        {
                            bool isLocalParseSuccess = ParseCompareContent(localContent, localABInfo);
                            if (!isLocalParseSuccess)
                            {
                                tcs.SetResult(false);
                                return;
                            } 
                            
                            //添加资源到下载列表
                            downLoadList.Clear();
                            foreach (string name in remoteABInfo.Keys)
                            {
                                if (!localABInfo.ContainsKey(name))
                                {
                                    Debug.Log($"[UpdateFile] 添加资源{name}到待下载列表");
                                    downLoadList.Add(name);
                                    totalBytes += remoteABInfo[name].size;
                                }
                                else if (localABInfo[name].md5 != remoteABInfo[name].md5)
                                {
                                    Debug.Log($"[UpdateFile] 资源{name}发生变化需要更新");
                                    downLoadList.Add(name);
                                    localABInfo.Remove(name);
                                    totalBytes += remoteABInfo[name].size;
                                }
                            }
                            
                            //下载更新列表文件
                            bool DownLoadSuccess = await DownLoadABFile((downloadedBytes, totalBytes, DownLoadProgress) =>
                            {
                                progressCallback?.Invoke(downloadedBytes, totalBytes, DownLoadProgress);
                            });

                            if (DownLoadSuccess)
                            {
                                string persistentCompareFile = Path.Combine(_persistentABPath, _remoteComparePath);
                                // 加日志 1
                                Debug.Log($"[UpdateFile] 准备写入对比文件: {persistentCompareFile}，内容长度: {remoteContent?.Length ?? -1}");

                                File.WriteAllText(persistentCompareFile, remoteContent);

                                // 加日志 2
                                Debug.Log($"[UpdateFile] 对比文件写入完成，验证存在: {File.Exists(persistentCompareFile)}");

                                CleanObsoleteRes();

                                // 加日志 3
                                Debug.Log($"[UpdateFile] 过时资源已删除");
                                tcs.SetResult(true);
                                return;
                            }
                            else
                            {
                                Debug.LogWarning("[UpdateFile] DownLoadSuccess 为 false，未更新对比文件");
                                tcs.SetResult(false);
                                return;
                            }
                        }
                        else
                        {
                            Debug.LogError("[UpdateFile] 未检测到本地对比文件");
                            tcs.SetResult(false);
                            return;
                        }
                    });
                }
                else
                {
                    Debug.Log("[UpdateFile] 未检测到远端对比文件");
                    tcs.SetResult(false);
                    return;
                }
            });
            return await tcs.Task;
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
            if (IsValidSingleton)
            {
                base.OnDestroy();
            }
        }
    }
}