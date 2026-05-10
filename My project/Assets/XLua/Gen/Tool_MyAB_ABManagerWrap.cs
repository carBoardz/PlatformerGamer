#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using XLua;
using System.Collections.Generic;


namespace XLua.CSObjectWrap
{
    using Utils = XLua.Utils;
    public class ToolMyABABManagerWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(Tool.MyAB.ABManager);
			Utils.BeginObjectRegister(type, L, translator, 0, 8, 1, 1);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "LoadRes", _m_LoadRes);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "LoadResAsync", _m_LoadResAsync);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "LoadABOnlyAsync", _m_LoadABOnlyAsync);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "LoadTextAsync", _m_LoadTextAsync);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "LoadSceneConfigAsync", _m_LoadSceneConfigAsync);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "UnloadAB", _m_UnloadAB);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ClearAllABCache", _m_ClearAllABCache);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "DebugListAllAssets", _m_DebugListAllAssets);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "_abCache", _g_get__abCache);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "_abCache", _s_set__abCache);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 1, 0, 0);
			
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					var gen_ret = new Tool.MyAB.ABManager();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to Tool.MyAB.ABManager constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LoadRes(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                Tool.MyAB.ABManager gen_to_be_invoked = (Tool.MyAB.ABManager)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _abName = LuaAPI.lua_tostring(L, 2);
                    string _resName = LuaAPI.lua_tostring(L, 3);
                    System.Type _type = (System.Type)translator.GetObject(L, 4, typeof(System.Type));
                    UnityEngine.Events.UnityAction<object> _callback = translator.GetDelegate<UnityEngine.Events.UnityAction<object>>(L, 5);
                    
                    gen_to_be_invoked.LoadRes( _abName, _resName, _type, _callback );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LoadResAsync(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                Tool.MyAB.ABManager gen_to_be_invoked = (Tool.MyAB.ABManager)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _abName = LuaAPI.lua_tostring(L, 2);
                    string _resName = LuaAPI.lua_tostring(L, 3);
                    System.Type _type = (System.Type)translator.GetObject(L, 4, typeof(System.Type));
                    UnityEngine.Events.UnityAction<object> _callback = translator.GetDelegate<UnityEngine.Events.UnityAction<object>>(L, 5);
                    
                    gen_to_be_invoked.LoadResAsync( _abName, _resName, _type, _callback );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LoadABOnlyAsync(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                Tool.MyAB.ABManager gen_to_be_invoked = (Tool.MyAB.ABManager)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _abName = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Events.UnityAction<bool> _callback = translator.GetDelegate<UnityEngine.Events.UnityAction<bool>>(L, 3);
                    
                    gen_to_be_invoked.LoadABOnlyAsync( _abName, _callback );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LoadTextAsync(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                Tool.MyAB.ABManager gen_to_be_invoked = (Tool.MyAB.ABManager)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _abName = LuaAPI.lua_tostring(L, 2);
                    string _resName = LuaAPI.lua_tostring(L, 3);
                    UnityEngine.Events.UnityAction<string> _callback = translator.GetDelegate<UnityEngine.Events.UnityAction<string>>(L, 4);
                    
                    gen_to_be_invoked.LoadTextAsync( _abName, _resName, _callback );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LoadSceneConfigAsync(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                Tool.MyAB.ABManager gen_to_be_invoked = (Tool.MyAB.ABManager)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _abName = LuaAPI.lua_tostring(L, 2);
                    string _soName = LuaAPI.lua_tostring(L, 3);
                    UnityEngine.Events.UnityAction<SceneConfigSO> _callback = translator.GetDelegate<UnityEngine.Events.UnityAction<SceneConfigSO>>(L, 4);
                    
                    gen_to_be_invoked.LoadSceneConfigAsync( _abName, _soName, _callback );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_UnloadAB(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                Tool.MyAB.ABManager gen_to_be_invoked = (Tool.MyAB.ABManager)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)) 
                {
                    string _abName = LuaAPI.lua_tostring(L, 2);
                    bool _unloadAllLoadedObjects = LuaAPI.lua_toboolean(L, 3);
                    
                    gen_to_be_invoked.UnloadAB( _abName, _unloadAllLoadedObjects );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)) 
                {
                    string _abName = LuaAPI.lua_tostring(L, 2);
                    
                    gen_to_be_invoked.UnloadAB( _abName );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to Tool.MyAB.ABManager.UnloadAB!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ClearAllABCache(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                Tool.MyAB.ABManager gen_to_be_invoked = (Tool.MyAB.ABManager)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.ClearAllABCache(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_DebugListAllAssets(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                Tool.MyAB.ABManager gen_to_be_invoked = (Tool.MyAB.ABManager)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _abName = LuaAPI.lua_tostring(L, 2);
                    
                    gen_to_be_invoked.DebugListAllAssets( _abName );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get__abCache(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                Tool.MyAB.ABManager gen_to_be_invoked = (Tool.MyAB.ABManager)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked._abCache);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set__abCache(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                Tool.MyAB.ABManager gen_to_be_invoked = (Tool.MyAB.ABManager)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked._abCache = (System.Collections.Generic.Dictionary<string, System.ValueTuple<UnityEngine.AssetBundle, int>>)translator.GetObject(L, 2, typeof(System.Collections.Generic.Dictionary<string, System.ValueTuple<UnityEngine.AssetBundle, int>>));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}
