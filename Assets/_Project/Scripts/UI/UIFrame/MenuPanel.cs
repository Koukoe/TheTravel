using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.InputSystem;

public abstract class MenuPanel : BasePanel
{
    private GameObject lastFocused;

    protected override void Awake() { }

    public override void OnOpen()
    {
        base.OnOpen();
        SetFocus(DefaultFocused());
        InputManager.Instance.UIActions.Cancel.performed += OnCancelPressed;
    }

    public override void OnClose()
    {
        base.OnClose();
        InputManager.Instance.UIActions.Cancel.performed -= OnCancelPressed;
    }

    public override void OnResume()
    {
        base.OnResume();
        // 没有则选默认
        SetFocus(lastFocused ?? DefaultFocused());
        InputManager.Instance.UIActions.Cancel.performed += OnCancelPressed;
    }

    public override void OnSuspend()
    {
        base.OnSuspend();
        InputManager.Instance.UIActions.Cancel.performed -= OnCancelPressed;
    }

    public override void Close(Action onAllFinished = null)
    {
        lastFocused = null;
        ReleaseFocus();
        base.Close(onAllFinished);
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
        base.Suspend(onAllFinished);
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

    private void OnCancelPressed(InputAction.CallbackContext context)
    {
        OnBackClicked();
    }

    protected virtual void OnBackClicked()
    {
        if (UIManager.Instance.IsTransitioning) return;

        UIManager.Instance.Pop();
    }
}