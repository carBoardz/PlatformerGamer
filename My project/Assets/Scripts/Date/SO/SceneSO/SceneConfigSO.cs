using UnityEngine;
[CreateAssetMenu(menuName = "创建SO/SceneLoadEventS0")]
public class SceneConfigSO: ScriptableObject
{
    public SceneType type;
    [Header("场景对应的AB包名")]
    public string SceneName;

    [Header("场景对象")]
    public Object Scene;
}
public enum SceneType
{
    Location, Menu
}