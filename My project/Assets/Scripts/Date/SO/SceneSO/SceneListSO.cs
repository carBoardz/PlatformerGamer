
using UnityEngine;
[CreateAssetMenu(fileName = "SceneListConfig", menuName = "游戏配置/关卡列表")]
public class SceneListSO : ScriptableObject
{
    [Header(" 按顺序拖拽所有关卡，热更只改这个文件")]
    public SceneConfigSO[] levelList;
}