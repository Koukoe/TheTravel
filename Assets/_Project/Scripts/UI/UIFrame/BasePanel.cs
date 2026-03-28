using UnityEngine;

public class BasePanel
{
    public UIType uiType;
    public GameObject ActiveObj;
    public bool isCreated = false;

    public virtual bool IsSubPanel => false;

    public BasePanel(UIType uiType)
    {
        this.uiType = uiType;
        this.isCreated = false;
    }
    public virtual void OnEnable() { }
    public virtual void OnDisable() { }
}