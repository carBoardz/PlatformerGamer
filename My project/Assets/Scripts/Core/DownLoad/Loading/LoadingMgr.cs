using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingMgr : MonoBehaviour
{
    public PreloadAssetListSO ABSOList;


    const string downLoadText = "DownLoad Resource";
    const string initText = "Initialize Resources";

    public Scrollbar scrollbar;
    public void Init()
    {
        scrollbar = Component.FindObjectOfType<Scrollbar>();
        
    }
    public void ScrollbarValueChange()
    {
        
    }
}
