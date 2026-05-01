
using UnityEngine;
[CreateAssetMenu(fileName = "SceneListConfig", menuName = "创建SO/关卡列表SO")]
public class SceneListSO : ScriptableObject
{
    [Header(" 按顺序拖拽所有关卡，热更只改这个文件")]
    public SceneConfigSO[] levelList;
}