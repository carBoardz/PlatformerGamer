using Tool.MyAB;

public static class GameRestart
{
    public static void Restart()
    {
        // 1. 清空 AB 包缓存（关键！）
        ABManager.Instance.ClearAllABCache();

        // 2. 清空 UI 管理器（关闭所有UI，清空对象池）
        UIManager.Instance.ClearAll();

        // 3. 清空配置缓存
        UIConfigManager.Instance.ClearCache();

        // 4. 清空事件中心
        EventCenter.Instance.Clear();

        // 5. 如果使用了 Lua，重置 Lua 虚拟机
        LuaMgr.Instance.Dispose();

        // 6. 重新加载入口场景
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}