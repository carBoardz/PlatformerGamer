using MySinleton;
using System.Collections.Generic;
using Tool.MyAB;
using UnityEngine;
using UnityEngine.UI;
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
    /// 统一打开UI
    /// </summary>
    public void OpenUI(string uiName, UILayer layer = UILayer.Normal)
    {
        // 1. 从【配置管理器】拿配置
        var config = UIConfigManager.Instance.GetUIConfig(uiName);
        if (config == null)
        {
            Debug.LogError("UI配置不存在：" + uiName);
            return;
        }

        // 2. 从缓存池中加载UIView
        BaseView targetUIView = GetFromPool(uiName);
        if (targetUIView == null)
        {
            // 3. 缓存池没有 → 从AB包热更加载
            ABManager.Instance.LoadResAsync(config.abName, config.prefabName, typeof(GameObject), (Obj) =>
            {
                GameObject prefab = Obj as GameObject;
                GameObject uiObj = GameObject.Instantiate(prefab);
                targetUIView = uiObj.GetComponent<BaseView>();
            });
        }

        // 4. 把view挂载到对应的节点上
        _openedUI[uiName] = targetUIView;
        Transform parent = GetUIRoot(layer);
        targetUIView.transform.SetParent(parent, false);
        targetUIView.gameObject.SetActive(true);

        // 5. 反射创建控制器，绑定MVC（热更核心）
        var controller = System.Activator.CreateInstance(System.Type.GetType(config.controller)) as BaseController;
        var model = System.Activator.CreateInstance(System.Type.GetType(config.controller.Replace("Controller", "Model"))) as BaseModel; 
        controller.Bind(model, targetUIView);
    }

    /// <summary>
    /// 关闭UI
    /// </summary>
    public void CloseUI(string uiName)
    {
        if (_openedUI.TryGetValue(uiName, out var view))
        {
            view.gameObject.SetActive(false);
            _openedUI.Remove(uiName);
            ReturnToPool(uiName, view);
        }
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
