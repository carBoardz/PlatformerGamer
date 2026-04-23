using System.Collections;
using System.Collections.Generic;
using Tool.MyAB;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public abstract class BaseView : MonoBehaviour
{
    /// <summary>
    /// 初始化View（加载AB包内的UI预制体）
    /// </summary>
    /// <param name="abName">UI所在AB包名称</param>
    /// <param name="resName">UI预制体名称</param>
    public virtual void InitView(string abName,string resName)
    {
        ABManager.Instance.LoadResAsync(abName, resName, typeof(object), (obj) =>
        {
            if (obj != null)
            {
                GameObject uiPrefab = obj as GameObject;
                GameObject uiObj = Instantiate(uiPrefab, transform);
            }
        });
    }

    /// <summary>
    /// 绑定UI元素
    /// </summary>
    protected abstract void BindView();
    /// <summary>
    /// 刷新View视图
    /// </summary>
    public abstract void RefreshView();
    /// <summary>
    /// 释放View视图
    /// </summary>
    public abstract void DisposeView();
}
