using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public abstract class MenuPanel : BasePanel
{
    protected MenuPanel(UIType type) : base(type) { }

    public override void OnEnable()
    {
        base.OnEnable();

        if (!isCreated)
        {
            Init();
            isCreated = true;
        }

        if (InputReader.Instance != null)
            InputReader.Instance.UIActions.Cancel.performed += OnCancelPressed;

        // 默认显示鼠标
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SetDefaultFocus();
    }

    public override void OnDisable()
    {
        if (InputReader.Instance != null)
            InputReader.Instance.UIActions.Cancel.performed -= OnCancelPressed;

        // 关闭面板时彻底清空焦点，防止按键穿透到下一层
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        base.OnDisable();
    }
    protected abstract void Init();

    /// <summary> 返回该面板第一个被选中的按钮对象 </summary>
    protected abstract GameObject GetFirstSelectable();

    protected virtual void OnCancelPressed(InputAction.CallbackContext context)
    {
        UIManager.GetInstance().Pop(false);
    }

    private void SetDefaultFocus()
    {
        GameObject first = GetFirstSelectable();
        if (first != null)
        {
            var runner = ActiveObj.GetComponent<MonoBehaviour>();
            if (runner != null)
            {
                runner.StartCoroutine(ExecuteDelayCoroutine(0.1f, () =>
                {
                    if (EventSystem.current != null)
                        EventSystem.current.SetSelectedGameObject(first);
                }));
            }
        }
    }

    // 延迟协程
    private IEnumerator ExecuteDelayCoroutine(float delayTime, Action callback)
    {
        yield return new WaitForSecondsRealtime(delayTime);
        callback?.Invoke();
    }
}