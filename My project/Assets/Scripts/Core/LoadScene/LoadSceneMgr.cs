using MySinleton;
using System.Collections;
using System.Collections.Generic;
using Tool.MyAB;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneMgr : SingletonMono<LoadSceneMgr>
{
    public SceneConfigSO currentScene;
    public int currentIndex = -1;
    SceneListSO _SceneListConfig;

    /// <summary>
    /// 加载关卡列表
    /// </summary>
    public void InitLevelList()
    {
        ABManager.Instance.LoadResAsync(
            "configassets",
            "LevelListConfig",
            typeof(SceneListSO),
            (obj) =>
            {
                _SceneListConfig = obj as SceneListSO;
                Debug.Log("关卡列表加载完成，总关卡数：" + _SceneListConfig.levelList.Length);
            });
    }
    /// <summary>
    /// 通过索引来加载指定的场景关卡
    /// </summary>
    /// <param name="index"></param>
    public void LoadSceneByIndex(int index)
    {
        if (_SceneListConfig == null)
        {
            Debug.LogError("关卡列表还未初始化！");
            return;
        }
        if (index < 0 || index >= _SceneListConfig.levelList.Length)
        {
            Debug.LogError($"关卡索引{index} 超出范围！");
            return;
        }
        if (currentIndex == index)
            return;
        currentIndex = index;
        currentScene = _SceneListConfig.levelList[currentIndex];

        SceneManager.LoadSceneAsync(currentScene.SceneName);
    }
    /// <summary>
    /// 加载下一关（通关调用，给Lua用）
    /// </summary>
    public void NextLevel()
    {
        LoadSceneByIndex(currentIndex + 1);
    }
}
