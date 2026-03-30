using UnityEngine;
using UnityEngine.EventSystems;

public abstract class MenuPanel : BasePanel
{
    private GameObject lastFocused;

    public override void OnOpen()
    {
        SetFocus(DefaultFocused());
    }

    public override void OnResume()
    {
        // 没有则选默认
        SetFocus(lastFocused ?? DefaultFocused());
    }

    public override void OnSuspend()
    {
        // 记录当前焦点
        if (EventSystem.current != null)
        {
            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current != null && current.transform.IsChildOf(transform)) lastFocused = current;
        }

        ReleaseFocus();
    }

    public override void OnClose()
    {
        ReleaseFocus();
        lastFocused = null;
    }


    protected abstract GameObject DefaultFocused();

    public void SetFocus(GameObject tar)
    {
        if (tar == null || EventSystem.current == null) return;

        EventSystem.current.SetSelectedGameObject(tar);
    }

    private void ReleaseFocus()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}