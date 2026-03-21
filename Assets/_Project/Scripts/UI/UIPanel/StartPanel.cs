using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPanel : BasePanel
{
    private static string name = "StartPanel";
    private static string path = "UIPanel/StartPanel";
    public static readonly UIType uiType = new UIType(name, path);
    public StartPanel() : base(uiType)
    {

    }
    public override void Onstart()
    {
        base.Onstart();
        Debug.Log("StartPanel开始使用");
    }
    public override void OnEnable()
    {
        base.OnEnable();
        Debug.Log("StartPanel启用");
    }
    public override void OnDisable()
    {
        base.OnDisable();
        Debug.Log("StartPanel禁用");
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        Debug.Log("StartPanel销毁");
    }
}
