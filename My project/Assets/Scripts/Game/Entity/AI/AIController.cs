using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : AIControllerBase
{
    protected void Awake()
    {
        aiHotLogic = new AIHotLogic();
        aiHotLogic.Init(this);
        EventCenter.Instance.Register("LuaEnv_Ready", new Action(OnLuaReady), owner: this);
    }
    void OnLuaReady()
    {
        if (LuaMgr.Instance.Global == null) Debug.LogError("LuaMgr未被实例化");
        LuaMgr.Instance.Global.Set("AIController",this);
        Debug.Log("AICtrl 注入Lua成功");
    }
    protected override void OnUpdate()
    {
        base.OnUpdate();
        aiControlleStateMechine.OnUpdate();
        aiHotLogic.Update(Time.deltaTime);
    }
    private void OnDisable()
    {
        EventCenter.Instance.UnRegister(owner: this);
    }
}
