using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Runtime.Serialization;

public abstract class MenuPanel : BasePanel
{
    protected MenuPanel(UIType type) : base(type) { }

    public CanvasGroup cg;
    private GameObject selectedObject;

    public override void OnEnable()
    {
        base.OnEnable();

        if (!isCreated)
        {
            Init();
            cg = ActiveObj.GetComponent<CanvasGroup>();
            isCreated = true;
        }

        if (InputReader.Instance != null)
            InputReader.Instance.UIActions.Cancel.performed += OnCancelPressed;

        // 默认显示鼠标
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (selectedObject == null) SetDefaultFocus();
        SetFocus();
    }

    public override void OnDisable(bool remainFocus = true)
    {
        if (InputReader.Instance != null)
            InputReader.Instance.UIActions.Cancel.performed -= OnCancelPressed;

        ClearFocus(remainFocus);
    }
    protected abstract void Init();

    /// <summary> 返回该面板第一个被选中的按钮对象 </summary>
    protected abstract GameObject GetFirstSelectable();

    private void SetDefaultFocus()
    {
        selectedObject = GetFirstSelectable();
    }

    private void SetFocus()
    {
        if (selectedObject != null)
        {
            var runner = ActiveObj.GetComponent<MonoBehaviour>();
            if (runner != null)
            {
                runner.StartCoroutine(ExecuteDelayCoroutine(0.05f, () =>
                {
                    if (EventSystem.current != null)
                        EventSystem.current.SetSelectedGameObject(selectedObject);
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

    protected virtual void OnCancelPressed(InputAction.CallbackContext context)
    {
        UIManager.GetInstance().Pop(false);
    }

    public override void ParentDisfocus()
    {
        if (cg != null) cg.alpha = 0.8f;
        ClearFocus(true);
    }

    public override void Resume()
    {
        if (cg != null) cg.alpha = 1f;
        SetFocus();
    }

    public void ClearFocus(bool remainFocus)
    {
        if (EventSystem.current != null && remainFocus)
        {
            // 记录下当前玩家选中的那个按钮
            selectedObject = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }
}