using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "PreloadAssetSO", menuName = "游戏配置/AB包下载列表SO配置")]
public class PreloadAssetListSO : ScriptableObject
{
    public List<PreloadAssetSO> DownLoadList;
}
