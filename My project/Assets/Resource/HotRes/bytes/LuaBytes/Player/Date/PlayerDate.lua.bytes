Config.PlayerDate = {}

print("PlayerDate.lua 执行了，EventCenter =", EventCenter)
--读取json数据表获取玩家数据
EventCenter:RegisterLua("LuaEnv_Ready", function()
    print("Lua 收到事件，开始加载资源")
    --local abMgr = CS.Tool.MyAB.ABManager.Instance
    print("ABMgr 实例：", abMgr)
    
    local abName = "configassets"
    local resName = "PlayerDate.json"
    print("实际加载资源名：" .. resName)
    
    abMgr:LoadTextAsync(abName, resName, function(res)
        print("资源加载结果：", res)
        if res == nil then return end
        local data = Json.decode(res)
        print("解析后的 data：", data)
        Config.PlayerDate[data.Id] = data
        
        PlayerCtrl.HotLogic.MaxHp = data.MaxHp
        PlayerCtrl.HotLogic.Hp = data.MaxHp
        
        print("Hp =", PlayerCtrl.HotLogic.Hp)
    end)
end)