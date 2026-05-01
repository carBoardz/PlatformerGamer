using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Tool.MyAB;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using XLua;

public class BaseView : MonoBehaviour
{
    private Dictionary<string, Component> _widgets = new Dictionary<string, Component>();
    public LuaTable _luaController { get; private set; }
    public string UIName { get; set; }
    public void BindLuaController(LuaTable controller, object userData = null)
    {
        _luaController = controller;
        
        var luafunction = _luaController.Get<LuaFunction>("OnInit");
        if (luafunction != null)
            luafunction?.Call(_luaController, this, userData);
        else
            Debug.LogError($"Lua Controller for {gameObject.name} has no OnInit method!");
    }
    /// <summary>
    /// 绑定UI元素
    /// </summary>
    protected void BindView()
    {
        _luaController.Get<LuaFunction>("OnButtonClick")?.Call(_luaController);
    }
    void CollectWidgets()
    {
        _widgets.Clear();

    }
    /// <summary>
    /// 刷新View视图
    /// </summary>
    public void RefreshView()
    {
        _luaController.Get<LuaFunction>("RefreshView")?.Call(_luaController);
    }
    public void OnButtonClick(string btnName)
    {
        if (_luaController == null) return;
        _luaController.Get<LuaFunction>("OnButtonClick")?.Call(_luaController, btnName);
    }
    public void CloseSelf()
    {
        UIManager.Instance.CloseUI(UIName);
    }
    /// <summary>
    /// 释放View视图
    /// </summary>
    public void DisposeView()
    {
        _luaController.Get<LuaFunction>("DisposeView")?.Call(_luaController);
        _luaController?.Dispose();
        _luaController = null;
    }
}
