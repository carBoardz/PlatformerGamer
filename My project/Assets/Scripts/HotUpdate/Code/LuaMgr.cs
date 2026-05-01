using MySinleton;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Tool.MyAB;
using UnityEngine;
using XLua;

public class LuaMgr : SingletonMono<LuaMgr>
{
    LuaEnv luaEnv = null;
    string LuaMainName => "LuaMain.lua";
    string _streamingPath => Path.Combine(Application.streamingAssetsPath, "ABRes");
    string _persistentPath => Path.Combine(Application.persistentDataPath, "ABRes");

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }
    public void Initialize()
    {
        if (luaEnv != null)
        {
            return;
        }

        try
        {
            luaEnv = new LuaEnv();
            luaEnv.AddLoader(MyCustomLoader);

            luaEnv.Global.Set("EventCenter", EventCenter.Instance);//特殊处理，先把EventCenter注入到lua环境中
            
            Debug.Log("<color=green>Lua 环境就绪</color>");//只要 new LuaEnv() 执行完成且不抛异常 就代表 Lua 环境就已经就绪
            Debug.Log("EventCenter 注入Lua成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lua 环境初始化失败，EventCenter注入失败：" + e.Message);
        }
    }
    public LuaTable Global 
    {
        get
        {
            return luaEnv.Global;
        }
    }
    public void DoString(string str)
    {
        luaEnv.DoString($"require('{str}')");
    }
    public LuaTable RequireModule(string luaPath)
    {
        // 执行require并返回Lua脚本return的表（Controller/Model）
        return luaEnv.DoString($"return require('{luaPath}')")[0] as LuaTable;
    }

    byte[] MyCustomLoader(ref string path)//自定义加载逻辑
    {
        if (ABManager.Instance != null)
        {
            var ta = ABManager.Instance.LoadAssetSync<TextAsset>("luaassets", path + ".lua");
            if (ta != null)
            {
                Debug.Log($"缓存中成功加载AB包: [{path}]，长度: {ta.bytes.Length}");
                return ta.bytes;
            }
            else
            {
                Debug.LogError($"AB包[{path}]加载失败");
            }
        }

        string fileName = $"{path}.lua";
        string persistentFilePath = Path.Combine(_persistentPath, fileName);
        if (File.Exists(persistentFilePath))
        {
            return File.ReadAllBytes(persistentFilePath);
        }
        else
        {
            string streamingFilePath = Path.Combine(_streamingPath, fileName);
            if (File.Exists(streamingFilePath))
            {
                return File.ReadAllBytes(streamingFilePath);
            }
            else
            {
                Debug.LogError($"本地资源未有{fileName}脚本");
            }
        }
        return null;
    }
}
