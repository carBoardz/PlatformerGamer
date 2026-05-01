using MySinleton;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tool.MyAB;
using UnityEngine;
using UnityEngine.UI;
using XLua;
using static UnityEditor.Experimental.GraphView.GraphView;

/// <summary>
/// 【单一职责】仅管理UI的打开/关闭/缓存，不碰AB，不解析配置
/// </summary>
public class UIManager : SingletonMono<UIManager>
{
    // 已打开的UI缓存
    private Dictionary<string, BaseView> _openedUI = new();
    private Dictionary<string, Queue<BaseView>> _uiPool = new();

    // UI根节点
    public Transform UIRoot_Normal { get; private set; }//正常
    public Transform UIRoot_Popup { get; private set; }//弹窗
    public Transform UIRoot_Top { get; private set; }//顶部提示

    public void Init()
    {
        _openedUI = new Dictionary<string, BaseView>();
        _uiPool = new Dictionary<string, Queue<BaseView>>();
        LoadUIFramework();
    }
    /// <summary>
    /// 加载基础UI框架
    /// </summary>
    void LoadUIFramework()
    {
        ABManager.Instance.LoadResAsync("ui_framework", "UIFramework", typeof(GameObject), (frameworkPrefab) =>
        {
            if (frameworkPrefab != null)
            {
                GameObject frameworkObj = GameObject.Instantiate((GameObject)frameworkPrefab);
                DontDestroyOnLoad(frameworkObj);

                // 获取分层UIRoot
                UIRoot_Normal = frameworkObj.transform.Find("UICanvas/UIRoot_Normal");
                UIRoot_Popup = frameworkObj.transform.Find("UICanvas/UIRoot_Popup");
                UIRoot_Top = frameworkObj.transform.Find("UICanvas/UIRoot_Top");
            }
        });
    }

    /// <summary>
    /// 异步打开并返回UI界面
    /// </summary>
    /// <param name="uiName">UI名字</param>
    /// <param name="layer">UI的层级</param>
    /// <param name="userData">用户的信息</param>
    /// <returns></returns>
    public async Task<BaseView> OpenUIAsync(string uiName, UILayer layer = UILayer.Normal, object userData = null)
    {
        var config = UIConfigManager.Instance.GetUIConfig(uiName);
        if (config == null)
        {
            Debug.LogError("UI配置不存在：" + uiName);
            return null;
        }

        BaseView view = await LoadViewAsync(config);
        if (view == null) Debug.LogError("获取View对象失败");

        // 挂载到对应层级
        Mount(uiName, view, layer);

        //从lua中获取返回的表
        var luaController = LuaMgr.Instance.RequireModule(config.controller);

        if (luaController.Get<LuaFunction>("New") != null)
        {
            var ctrlInst = luaController.Get<LuaFunction>("New").Call(view, userData)[0] as LuaTable;
            view.BindLuaController(ctrlInst,userData);
        }
        else
        {
            view.BindLuaController(luaController,userData);
        }

        return view;
    }
    /// <summary>
    /// 从对象池获取View对象，没有则从AB包中加载
    /// </summary>
    /// <param name="config">UIConfig信息</param>
    /// <returns></returns>
    public async Task<BaseView> LoadViewAsync(UIConfigItem config)
    {
        // 池中有则直接返回
        var _pool = GetFromPool(config.uiName);
        if (_pool != null) return _pool;

        var tcs = new TaskCompletionSource<BaseView>();
        // 缓存池没有 → 从AB包热更加载
        ABManager.Instance.LoadResAsync(config.abName, config.prefabName, typeof(GameObject), (Obj) =>
        {
            if (Obj != null)
            {
                GameObject prefab = Obj as GameObject;
                GameObject uiObj = GameObject.Instantiate(prefab);
                var targetUIView = uiObj.GetComponent<BaseView>();
                tcs.SetResult(targetUIView);
            }
            else
            {
                tcs.SetResult(null);
            }
        });
        
        return await tcs.Task;
    }
    /// <summary>
    /// 关闭UI
    /// </summary>
    public void CloseUI(string uiName)
    {
        if (_openedUI.TryGetValue(uiName, out var view))
        {
            view.gameObject.SetActive(false);
            view.DisposeView();
            _openedUI.Remove(uiName);
            ReturnToPool(uiName, view);
        }
    }
    /// <summary>
    /// 挂载到对应层级
    /// </summary>
    /// <param name="uiName">view的名字</param>
    /// <param name="view">要挂载的view</param>
    public void Mount(string uiName, BaseView view, UILayer layer = UILayer.Normal)
    {
        _openedUI[uiName] = view;
        view.UIName = uiName;
        Transform parent = GetUIRoot(layer);
        view.transform.SetParent(parent, false);
        view.gameObject.SetActive(true);
    }
    Transform GetUIRoot(UILayer uILayer)
    {
        return uILayer switch
        {
            UILayer.Normal => UIRoot_Normal,
            UILayer.Popup => UIRoot_Popup,
            UILayer.Top => UIRoot_Top,
            _ => UIRoot_Normal
        };
    }
    BaseView GetFromPool(string uiName)
    {
        if (string.IsNullOrEmpty(uiName))
            return null;
        if (_uiPool.TryGetValue(uiName, out var pool) && pool.Count > 0)
        {
            return pool.Dequeue();
        }
        return null;
    }
    void ReturnToPool(string uiName, BaseView view)
    {
        if (string.IsNullOrEmpty(uiName))
            return;
        if (!_uiPool.ContainsKey(uiName))
        {
            _uiPool[uiName] = new Queue<BaseView>();
        }
        _uiPool[uiName].Enqueue(view);
    }
}
public enum UILayer
{
    Normal,
    Popup,
    Top
}
