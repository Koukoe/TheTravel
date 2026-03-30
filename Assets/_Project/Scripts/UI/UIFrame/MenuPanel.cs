using UnityEngine;
using UnityEngine.EventSystems;
using System;

public abstract class MenuPanel : BasePanel
{
    private GameObject lastFocused;

    protected override void Awake() { }

    public override void OnOpen()
    {
        SetFocus(DefaultFocused());
    }

    public override void OnResume()
    {
        // 没有则选默认
        SetFocus(lastFocused ?? DefaultFocused());
    }

    public override void Suspend(Action onAllFinished = null)
    {
        // 记录当前焦点
        if (EventSystem.current != null)
        {
            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current != null && current.transform.IsChildOf(transform)) lastFocused = current;
        }

        ReleaseFocus();
        base.Suspend();
    }

    public override void Close(Action onAllFinished = null)
    {
        lastFocused = null;
        ReleaseFocus();
        base.Close();
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