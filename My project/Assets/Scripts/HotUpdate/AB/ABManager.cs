using MySinleton;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using XLua;

namespace Tool.MyAB
{
    /// <summary>
    /// AB包加载管理器（纯加载逻辑，不解耦下载）
    /// 职责：从persistent/streaming加载AB包，处理跨平台路径适配
    /// </summary>
    [XLua.LuaCallCSharp]
    public class ABManager : SingletonMono<ABManager>
    {
        AssetBundle _mainAB;
        AssetBundleManifest _mainManifest;
        bool _isLoadingMainManifest = false;
        public Dictionary<string, (AssetBundle ab, int refCount)> _abCache = new Dictionary<string, (AssetBundle ab, int refCount)>();

        private string _persistentPath; // 热更路径（persistentDataPath）
        private string _streamingPath;  // 默认路径（streamingAssetsPath）

        protected override void Awake()
        {
            base.Awake();
            InitPath();

            EventCenter.Instance.Register(
            "Csharp_Managers_Ready",
            new Action(OnCsharpManagersReady),
            owner: this,
            once: true
            );
        }

        void InitPath()
        {
            _persistentPath = Path.Combine(Application.persistentDataPath, "ABRes/");
            _streamingPath = Path.Combine(Application.streamingAssetsPath, "ABRes/");
            if (!Directory.Exists(_persistentPath))
            {
                Directory.CreateDirectory(_persistentPath);
            }
        }
        string MainABName
        {
            get
            {
#if UNITY_IOS
        return "IOS";
#elif UNITY_ANDROID
        return "Android";
#else
                return "PC";
#endif
            }
        }
        #region 同步加载
        /// <summary>
        /// 同步加载 不指定类型
        /// </summary>
        /// <param name="ABName">包路径</param>
        /// <param name="ResName">主包名</param>
        /// <returns></returns>
        public void LoadRes(string abName, string resName, System.Type type, UnityAction<object> callback)
        {
            if (string.IsNullOrEmpty(abName) || string.IsNullOrEmpty(resName) || callback == null)
            {
                Debug.LogError("同步加载参数错误：abName/resName/callback不能为空");
                callback?.Invoke(null);
                return;
            }

            if (Application.platform == RuntimePlatform.Android && !File.Exists(Path.Combine(_persistentPath, abName)))
            {
                StartCoroutine(LoadAndroidStreamingResCoroutine(abName, resName, type, callback));
                return;
            }

            object result = null;
            try
            {
                // 1. 加载主Manifest和依赖包
                GetMainManifest(abName);

                // 2. 加载目标AB包（优先热更路径）
                if (LoadSingleAB(abName, out AssetBundle ab))
                {
                    result = ab.LoadAsset(resName, type);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"同步加载异常：{ex.Message}");
                result = null;
            }
            finally
            {
                callback.Invoke(result);
            }
        }
        void GetMainManifest(string ABName)
        {
            if (_mainManifest != null) return;

            if (_isLoadingMainManifest)
            {
                while (_isLoadingMainManifest)
                    System.Threading.Thread.Sleep(10);
                return;
            }

            try
            {
                if (_mainAB == null)
                {
                    string mainABPath = GetABRealPath(MainABName);
                    if (string.IsNullOrEmpty(mainABPath))
                    {
                        Debug.LogError($"主AB包{MainABName}路径不存在");
                        return;
                    }
                    _mainAB = AssetBundle.LoadFromFile(mainABPath);
                    if (_mainAB == null)
                    {
                        Debug.LogError($"主AB包{MainABName}加载失败");
                        return;
                    }
                    _mainManifest = _mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    if (_mainManifest == null)
                    {
                        Debug.LogError("加载Manifest失败");
                        _mainAB.Unload(true);
                        _mainAB = null;
                        return;
                    }
                }

                string[] dependencies = _mainManifest.GetAllDependencies(ABName);
                if (dependencies != null && dependencies.Length > 0)
                    foreach (var dependence in dependencies)
                    {
                        LoadSingleAB(dependence, out _);
                    }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        bool LoadSingleAB(string abName, out AssetBundle ab)
        {
            ab = null;
            if (_abCache.TryGetValue(abName, out var abData))
            {
                int newCount = abData.refCount + 1;
                Debug.Log($"AB包{abName}引用计数+1，当前：{newCount}");
                _abCache[abName] = (abData.ab, newCount);
                ab = abData.ab;
                return true;
            }

            string abPath = GetABRealPath(abName);
            if (string.IsNullOrEmpty(abPath))
            {
                Debug.LogError($"AB包{abName}路径不存在（热更：{_persistentPath}/{abName}，默认：{_streamingPath}/{abName}）");
                return false;
            }
            else if (abPath.StartsWith("jar:file://"))
            {
                return false;
            }
            else
            {
                ab = AssetBundle.LoadFromFile(abPath);
                if (ab != null)
                {
                    Debug.Log($"AB包{abName}加载成功，引用计数初始化为1");
                    _abCache.Add(abName, (ab, 1));
                    return true;
                }
            }
            Debug.LogError($"AB包{abName}加载失败（路径：{abPath}）");
            return false;
        }
        /// <summary>
        /// 同步加载，方便 LuaLoader 调用
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="abName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public T LoadAssetSync<T>(string abName, string assetName) where T : UnityEngine.Object
        {
            if (_abCache.TryGetValue(abName, out var data) && data.ab != null)
            {
                return data.ab.LoadAsset<T>(assetName);
            }
            GetMainManifest(abName);
            if (LoadSingleAB(abName, out var ab))
            {
                return ab.LoadAsset<T>(assetName);
            }
            print($"加载{assetName}出错，返回null");
            return null;
        }
        #endregion
        #region 异步加载资源
        /// <summary>
        /// 异步加载 指定类型
        /// </summary>
        /// <param name="abName">包路径</param>
        /// <param name="resName">主包名</param>
        /// <param name="type">指定加载类型</param>
        /// <param name="callback">回调传出加载结果</param>
        public void LoadResAsync(string abName, string resName, System.Type type, UnityAction<object> callback)
        {
            if (string.IsNullOrEmpty(abName) || string.IsNullOrEmpty(resName) || type == null)
            {
                Debug.LogError($"AB包加载参数错误：abName={abName}, resName={resName}, type={type}");
                callback?.Invoke(null);
                return;
            }
            _ = LoadResTask(abName, resName, type, callback);
        }
        /// <summary>
        /// 异步的只加载 AB 包本身（如lua脚本包、config包）
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="callback"></param>
        public void LoadABOnlyAsync(string abName, UnityAction<bool> callback)
        {
            if (string.IsNullOrEmpty(abName))
            {
                Debug.LogError($"AB包名{abName}不能为null");
                callback?.Invoke(false);
                return;
            }
            _ = LoadResTask(abName, callback);
        }
        public void LoadTextAsync(string abName, string resName, UnityAction<string> callback)
        {
            if (string.IsNullOrEmpty(abName) || string.IsNullOrEmpty(resName))
            {
                Debug.LogError($"abName或者resName不能为空");
                return;
            }

            LoadResAsync(abName, resName, typeof(TextAsset), (obj) =>
            {
                string text = "";
                if (obj is TextAsset ta) text = ta.text;
                callback?.Invoke(text);
            });
        }
        public void LoadSceneConfigAsync(string abName, string soName, UnityAction<SceneConfigSO> callback)
        {
            LoadResAsync(abName, soName, typeof(SceneConfigSO), (obj) =>
            {
                callback?.Invoke(obj as SceneConfigSO);
            });
        }
        #region LoadResTask的各种重载
        async Task<object> LoadResTask(string abName, string resName, System.Type type, UnityAction<object> callback)
        {
            object result = null;
            try
            {
                //先并发异步加载依赖包
                if (_mainManifest == null)
                {
                    bool wtf = await LoadMainManifestAsync();
                    if (!wtf)
                    {
                        Debug.LogError("主Manifest加载失败");
                        callback?.Invoke(null);
                        return null;
                    }
                }

                string[] dependencies = _mainManifest.GetAllDependencies(abName);
                if (dependencies != null && dependencies.Length > 0)
                {
                    List<Task> depTasks = new List<Task>();
                    foreach (var dependence in dependencies)
                    {
                        depTasks.Add(LoadSingleABAsync(dependence));
                    }
                    await Task.WhenAll(depTasks);
                }

                //再异步加载目标包
                await LoadSingleABAsync(abName);

                if (_abCache.TryGetValue(abName, out var abData) && _abCache[abName].ab != null)
                {
                    var abRequest = abData.ab.LoadAssetAsync(resName, type);
                    await AsyncHealper.AwaitAsyncOperation(abRequest);
                    result = abRequest.asset;
                    if (result == null)
                    {
                        Debug.LogError($"AB包{abName}中未找到资源{resName}（类型：{type.Name}）");
                    }
                    return result;
                }
                else
                {
                    Debug.LogError($"AB包{abName}加载失败");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("异步加载异常" + ex);
                return null;
            }
            finally
            {
                callback?.Invoke(result);
            }
        }
        async Task LoadResTask(string abName, UnityAction<bool> callback)
        {
            bool success = false;
            try
            {
                if (_mainManifest == null)
                {
                    bool wtf = await LoadMainManifestAsync();
                    if (!wtf)
                    {
                        Debug.LogError("异步加载主Manifest资源出错");
                        callback?.Invoke(success);
                        return;
                    }
                }

                string[] abNameDependencies = _mainManifest.GetAllDependencies(abName);
                if (abNameDependencies != null && abNameDependencies.Length > 0)
                {
                    List<Task> depTasks = new List<Task>();
                    foreach (var dependence in abNameDependencies)
                    {
                        Debug.Log($"添加{dependence}至{abName}");
                        depTasks.Add(LoadSingleABAsync(dependence));
                    }
                    await Task.WhenAll(depTasks);
                }

                //再异步加载目标包
                await LoadSingleABAsync(abName);

                if (_abCache.ContainsKey(abName))
                {
                    success = true;
                    Debug.Log($"AB包 {abName} 预加载成功");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"AB包 {abName} 预加载失败：{ex.Message}");
            }
            finally
            {
                callback?.Invoke(success);
            }
        }
        #endregion
        //加载主包
        async Task<bool> LoadMainManifestAsync()
        {
            if (_mainManifest != null) return true;
            
            if (_isLoadingMainManifest)
            {
                while (_isLoadingMainManifest)
                    await Task.Delay(10);
                return _mainManifest != null;
            }

            try
            {
                _isLoadingMainManifest = true;
                if (_mainAB == null)
                {
                    var mainABRequest = AssetBundle.LoadFromFileAsync(GetABRealPath(MainABName));
                    await AsyncHealper.AwaitAsyncOperation(mainABRequest);
                    _mainAB = mainABRequest.assetBundle;
                }
                if (_mainAB == null)
                {
                    Debug.LogError($"主AB包{_mainAB}加载失败");
                    return false;
                }

                if (_mainManifest == null)
                {
                    var manifestRequest = _mainAB.LoadAssetAsync<AssetBundleManifest>("AssetBundleManifest");
                    await AsyncHealper.AwaitAsyncOperation(manifestRequest);
                    _mainManifest = manifestRequest.asset as AssetBundleManifest;
                }
                if (_mainManifest == null)
                {
                    Debug.LogError($"Manifest加载失败");
                    _mainAB.Unload(true);
                    _mainAB = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            finally
            {
                _isLoadingMainManifest = false;
            }
            return true;
        }
        async Task LoadSingleABAsync(string abName)
        {
            if (_abCache.TryGetValue(abName, out var abData))
            {
                int newCount = abData.refCount + 1;
                Debug.Log($"AB包{abName}引用计数+1，当前：{newCount}");
                _abCache[abName] = (abData.ab, newCount);
                return;
            }


            if (string.IsNullOrEmpty(abName))
            {
                Debug.LogError($"AB包{abName}路径不存在");
                return;
            }

            string abPath = GetABRealPath(abName);
            var abRequest = AssetBundle.LoadFromFileAsync(abPath);
            await AsyncHealper.AwaitAsyncOperation(abRequest);

            AssetBundle ab = abRequest.assetBundle;
            if (ab == null)
            {
                Debug.LogError($"AB包{abName}加载失败（路径：{abPath}）");
                return;
            }

            _abCache.Add(abName, (ab, 1));
            Debug.Log($"AB包{abName}加载成功，引用计数初始化为1");
        }
        #endregion
        IEnumerator LoadAndroidStreamingResCoroutine(string abName, string resName, System.Type type, UnityAction<object> callback)
        {
            string path = GetABRealPath(abName);
            if (!File.Exists(path))
            {
                callback.Invoke(null);
                yield break;
            }

            using (UnityWebRequest req = UnityWebRequestAssetBundle.GetAssetBundle(path))
            {
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"安卓加载{abName}失败：{req.error}");
                    callback.Invoke(null);
                    yield break;
                }

                AssetBundle ab = DownloadHandlerAssetBundle.GetContent(req);
                if (ab == null)
                {
                    Debug.LogError($"安卓包加载{ab}失败");
                    callback.Invoke(null);
                    yield break;
                }

                object result = ab.LoadAsset(resName);
                if (!_abCache.ContainsKey(abName))
                {
                    _abCache.Add(abName, (ab, 1));
                }
                callback.Invoke(result);
                yield break;
            }
        }
        //判断主包路径
        string GetABRealPath(string abName)
        {
            if (string.IsNullOrEmpty(abName))
            {
                Debug.LogError("AB包名称不能为空");
                return null;
            }

            string persistentPath = Path.Combine(_persistentPath, abName);
            if (File.Exists(persistentPath))
            {
                return persistentPath;
            }

            string streamingPath = string.Empty;

            // 移动端StreamingAssets是特殊协议，需额外处理（同步加载时）
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    // 安卓：jar协议路径，只能用UnityWebRequest加载
                    streamingPath = $"jar:file://{_streamingPath}/{abName}";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    // iOS：本地路径，直接加载
                    streamingPath = Path.Combine(_streamingPath, abName);
                    break;
                default:
                    // PC/编辑器：本地路径
                    streamingPath = Path.Combine(_streamingPath, abName);
                    break;
            }
            //验证路径是否有效
            if (!Application.isEditor && Application.platform != RuntimePlatform.Android)
            {
                if (!File.Exists(streamingPath))
                {
                    Debug.LogError($"AB包{abName}在streamingAssets路径不存在：{streamingPath}");
                    return null;
                }
            }
            return streamingPath;
        }
        /// <summary>
        /// 卸载AB包（核心方法，补充调用说明）
        /// </summary>
        /// <param name="abName">AB包名称</param>
        /// <param name="unloadAllLoadedObjects">是否卸载已加载的资源对象</param>
        public void UnloadAB(string abName, bool unloadAllLoadedObjects = false)
        {
            if (_abCache.TryGetValue(abName, out var abData))
            {
                int newRefCount = abData.refCount - 1;
                if (newRefCount <= 0)
                {
                    abData.ab.Unload(unloadAllLoadedObjects);
                    _abCache.Remove(abName);
                    Debug.Log($"AB包{abName}已卸载");
                }
                else
                {
                    _abCache[abName] = (abData.ab, newRefCount);
                    Debug.Log($"AB包{abName}引用计数-1，当前：{newRefCount}");
                }
            }
            else
            {
                Debug.LogWarning($"AB包{abName}未缓存");
            }
        }
        /// <summary>
        /// 清空所有AB包缓存（游戏退出/场景切换时调用）
        /// </summary>
        public void ClearAllABCache()
        {
            foreach (var abData in _abCache.Values)
            {
                abData.ab?.Unload(true);
            }
            _abCache.Clear();
            _mainManifest = null;
            _mainAB?.Unload(true);
            _mainAB = null;
            Debug.Log("所有AB包缓存已清空");
        }
        protected override void OnDestroy()
        {
            ClearAllABCache();
            base.OnDestroy();
        }
        /// <summary>
        /// 调试方法（打印指定ab包中的所有缓存资源）
        /// </summary>
        /// <param name="abName">指定ab包</param>
        public void DebugListAllAssets(string abName)
        {
            if (_abCache.TryGetValue(abName, out var data) && data.ab != null)
            {
                string[] assetNames = data.ab.GetAllAssetNames();
                Debug.Log($"AB包[{abName}]里的所有资源：");
                foreach (var name in assetNames)
                {
                    Debug.Log(" - " + name);
                }
            }
        }
        #region 事件委托
        void OnCsharpManagersReady()
        {
            LuaMgr.Instance.Global.Set("ABMgr", this);
            Debug.Log("ABMgr 注入Lua成功");
        }
        #endregion
    }
}
