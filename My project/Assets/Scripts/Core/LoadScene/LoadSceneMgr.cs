using MySinleton;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tool.MyAB;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLua;

public class LoadSceneMgr : SingletonMono<LoadSceneMgr>
{
    public SceneConfigSO currentScene;
    public int currentIndex = -1;
    SceneListSO _SceneListConfig;

    const string abName = "configassets";
    const string ResName = "SceneListConfig";

    protected override void Awake()
    {
        base.Awake();
        EventCenter.Instance.Register("LuaEnv_Ready", new Action(InitLevelList), owner: this);
    }
    /// <summary>
    /// 加载关卡列表
    /// </summary>
    public void InitLevelList()
    {
        ABManager.Instance.LoadResAsync(
            abName,
            ResName,
            typeof(SceneListSO),
            (obj) =>
            {
                _SceneListConfig = obj as SceneListSO;
                Debug.Log("关卡列表加载完成，总关卡数：" + _SceneListConfig.levelList.Length);
            }
        );
    }
    /// <summary>
    /// 通过索引来加载指定的场景关卡
    /// </summary>
    /// <param name="index"></param>
    public async Task LoadSceneByIndex(int index)
    {
        if (_SceneListConfig == null || index < 0 || index >= _SceneListConfig.levelList.Length)
        {
            Debug.LogError($"关卡索引{index} 无效！");
            return;
        }
        try
        {
            UIManager.Instance.ClearAll();

            //显示过渡
            //await LoadingManager.Instance.ShowAsync("Warping to next sector...");//要改

            currentIndex = index;
            currentScene = _SceneListConfig.levelList[currentIndex];
            
             SceneManager.LoadSceneAsync(currentScene.SceneName);
        }
        catch (Exception ex)
        {
            Debug.LogError($"场景index{index}加载错误");
        }
    }
    /// <summary>
    /// 加载下一关（通关调用，给Lua用）
    /// </summary>
    public void NextLevel()
    {
        LoadSceneByIndex(currentIndex + 1);
    }
}
