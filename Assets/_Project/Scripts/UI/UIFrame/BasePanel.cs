using UnityEngine;

public abstract class BasePanel : MonoBehaviour
{
    public bool hidePreviousPanel = true;

    public virtual void OnOpen() { }
    public virtual void OnClose() { }
    public virtual void OnSuspend() { }
    public virtual void OnResume() { }
}