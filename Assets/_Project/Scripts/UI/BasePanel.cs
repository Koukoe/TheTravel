using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePanel
{
    public UIType uiType;
    public GameObject ActiveObj;
    public BasePanel(UIType uiType)
    {
        uiType = uiType;
    }
    public virtual void Onstart()
    {
        Debug.Log("BasePanel开始使用");
    }
    public virtual void OnEnable()
    {
        
    }
    public virtual void OnDisable()
    {
        
    }
    public virtual void OnDestroy()
    {
        
    }
}
