using MySinleton;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tool.MyAB;
using UnityEngine;
using UnityEngine.UI;
using XLua;

public class LoadingManager : SingletonMono<LoadingManager>
{
    const string LoadingUIName = "StartGameLoadingPanelController";
    BaseView _loadingView;
    LuaTable _loadingLuaController;

    TaskCompletionSource<bool> _showTcs; // 用于等待显示动画完成
    TaskCompletionSource<bool> _hideTcs; // 用于等待隐藏动画完成

    public async Task ShowAsync(string loadingText = "Check for resource updates...")
    {
        if (_loadingView == null)
        {
            _loadingView = await UIManager.Instance.OpenUIAsync(LoadingUIName, UILayer.Top);
            if (_loadingView == null) return;
            _loadingLuaController = _loadingView._luaController;
        }
        else
        {
            _loadingView.gameObject.SetActive(true);
        }

        _loadingLuaController?.Get<LuaFunction>("OnShow")?.Call(_loadingLuaController, loadingText);
        _showTcs = new();
        await _showTcs.Task;
    }

    public void UpdateProgress(long downloadedBytes, long totalBytes, float DownLoadProgress, string msg = null)
    {
        _loadingLuaController?.Get<LuaFunction>("UpdateProgress")?.Call(_loadingLuaController, downloadedBytes, totalBytes, DownLoadProgress, msg);
    }
    public async Task HideAsync(string loadingText = "Initialization complete...")
    {
        _loadingLuaController?.Get<LuaFunction>("OnHide")?.Call(_loadingLuaController, loadingText);
        _hideTcs = new();
        await _hideTcs.Task;

        // 真正的关闭交给 UIManager 回收/隐藏
        UIManager.Instance.CloseUI(LoadingUIName);
        _loadingView = null;
        _loadingLuaController = null;
    }
    /// <summary>
    /// 供 Lua 动画事件调用
    /// </summary>
    public void OnShowAnimFinished()
    {
        _showTcs?.TrySetResult(true);
    }
    public void OnHideAnimFinished()
    {
        _hideTcs?.TrySetResult(true);
    }
}
//[其他业务]  →  LoadingService(静态工具类)
//                    ↓ 调用
//              UIManager.OpenUIAsync("LoadingPanel")  ← 复用框架
//                    ↓ 返回
//              BaseView (内部持有 LuaController)
//                    ↓ 调用 Lua 方法
//              LoadingPanel 的 Lua Controller (OnShow, UpdateProgress, OnHide...)