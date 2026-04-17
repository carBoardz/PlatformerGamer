using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MoveABToSA
{
    [MenuItem("将选中的热更资源添加到默认资源中")]
    public static void MoveResToSA()
    {
        Object[] selectObj = Selection.GetFiltered(typeof(object),SelectionMode.DeepAssets);
        if (selectObj.Length == 0)return;
        string ResInfo = null;
        foreach (var obj in selectObj)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            string fileName = assetPath.Substring(assetPath.LastIndexOf("/"));
            AssetDatabase.CopyAsset( assetPath , "Asset/StreamingAsset/ABRes/" + fileName);
            FileInfo curr = new FileInfo(assetPath);
            ResInfo += fileName + curr.Length + ABCompare.GetMD5(assetPath);
            ResInfo += "|";
        }
        //减最后一个字符“|”
    }
}
