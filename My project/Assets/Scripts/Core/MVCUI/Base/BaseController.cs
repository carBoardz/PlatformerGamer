using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseController
{
    public BaseModel _model;
    public BaseView _view;
    public void Bind(BaseModel model, BaseView view)
    {
        _model = model;
        _view = view;
        _model.InitModel();
        _view.InitView(GetViewABName(), GetViewResName());
        _view.RefreshView();
    }

    public abstract string GetViewABName();
    public abstract string GetViewResName();
    public abstract string GetInitViewDate();
    /// <summary>
    /// 处理View交互事件
    /// </summary>
    public abstract void HandleViewEvent(string eventName, object data);

    /// <summary>
    /// 释放Controller资源
    /// </summary>
    public abstract void DisposeController();
}
