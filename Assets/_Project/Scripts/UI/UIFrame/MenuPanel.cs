using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;
using System;

// 既然它是面板，必须挂在物体上，且通常需要 CanvasGroup 控制透明度
[RequireComponent(typeof(CanvasGroup))]
public abstract class MenuPanel : BasePanel
{
    protected CanvasGroup cg;
    private GameObject lastSelectedObject; // 记录上次选中的物体，用于恢复焦点

    protected virtual void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    // --- 生命周期回调 (由 UIManager 调用) ---

    public override void OnOpen()
    {
        // 绑定返回/取消事件
        if (InputManager.Instance != null)
            InputManager.Instance.UIActions.Cancel.performed += OnCancelPressed;

        // 默认显示鼠标（如果你是纯手柄游戏可以去掉）
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 设置初始焦点
        GameObject target = lastSelectedObject != null ? lastSelectedObject : GetFirstSelectable();
        SetFocus(target);
    }

    public override void OnClose()
    {
        // 解绑事件防止内存泄漏
        if (InputManager.Instance != null)
            InputManager.Instance.UIActions.Cancel.performed -= OnCancelPressed;

        ClearFocus(false); // 关闭时不需要保留焦点记录
    }

    public override void OnDisfocus()
    {
        // 变为半透明表示失去焦点
        if (cg != null) cg.alpha = 0.8f;

        // 记录当前选中的位置，并清除当前焦点
        ClearFocus(true);
    }

    public override void OnResume()
    {
        // 重新绑定事件
        if (InputManager.Instance != null)
            InputManager.Instance.UIActions.Cancel.performed += OnCancelPressed;

        // 恢复透明度
        if (cg != null) cg.alpha = 1f;

        // 恢复之前记录的焦点
        SetFocus(lastSelectedObject);
    }

    // --- 抽象方法：交给具体的面板实现 ---

    /// <summary> 返回该面板第一个被选中的按钮对象 </summary>
    protected abstract GameObject GetFirstSelectable();

    // --- 焦点逻辑控制 ---

    private void SetFocus(GameObject obj)
    {
        if (obj == null) obj = GetFirstSelectable();
        if (obj == null) return;

        // 使用内置的 StartCoroutine，不再需要 runner
        StartCoroutine(ExecuteDelaySetFocus(0.05f, obj));
    }

    private IEnumerator ExecuteDelaySetFocus(float delayTime, GameObject obj)
    {
        // 使用 Realtime 确保即便游戏暂停时 UI 也能正常工作
        yield return new WaitForSecondsRealtime(delayTime);
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(obj);
        }
    }

    private void ClearFocus(bool rememberCurrent)
    {
        if (EventSystem.current == null) return;

        if (rememberCurrent)
        {
            // 只有当玩家真的选中了某个物体时才记录
            lastSelectedObject = EventSystem.current.currentSelectedGameObject;
        }
        else
        {
            lastSelectedObject = null;
        }

        EventSystem.current.SetSelectedGameObject(null);
    }

    // --- 事件响应 ---

    protected virtual void OnCancelPressed(InputAction.CallbackContext context)
    {
        // 调用 UIManager 弹出当前面板
        UIManager.Instance.Pop();
    }
}