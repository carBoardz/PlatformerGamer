using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PreloadAssetSO", menuName = "创建SO/单个AB包下载SO配置")]
public class PreloadAssetSO : ScriptableObject
{
    [Header(" 需要对比的文件")]
    public string ABName;
    public bool isNeedToDownLoadFromRemote;
    public string DownLoadPath = "http://26.166.242.49:8000/ABRes/";
    public int Order = 0;
    public string Discript;
}
