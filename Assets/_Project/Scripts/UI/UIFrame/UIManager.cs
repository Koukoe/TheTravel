using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIManager
{
    private static UIManager instance;
    public static UIManager GetInstance()
    {
        if (instance == null) instance = new UIManager();
        return instance;
    }

    public Stack<BasePanel> stack_ui = new Stack<BasePanel>();
    public Dictionary<string, GameObject> dict_uiobject = new Dictionary<string, GameObject>();
    public GameObject CanvasObj;

    // 状态锁：防止在切换动画或处理输入时重复触发
    private bool isTransitioning = false;

    public UIManager()
    {
        instance = this;
    }

    private GameObject GetSingleObject(UIType uIType)
    {
        if (CanvasObj == null)
        {
            CanvasObj = UIMethods.GetInstance().FindCanvas();
        }

        GameObject gameObject = PoolManager.Global.Get(uIType.Name);

        if (gameObject != null)
        {
            gameObject.transform.SetParent(CanvasObj.transform, false);
        }
        else
        {
            Debug.LogError($"PoolManager中未找到名为 {uIType.Name} 的配置！");
        }

        return gameObject;
    }

    public void Push(BasePanel basePanel)
    {
        if (isTransitioning) return;

        // 1. 禁用当前顶层 UI
        if (stack_ui.Count > 0)
        {
            BasePanel top = stack_ui.Peek();
            top.OnDisable();
            top.ActiveObj.SetActive(false);
        }

        // 2. 获取并设置新 UI
        GameObject ui_object = GetSingleObject(basePanel.uiType);
        basePanel.ActiveObj = ui_object;

        if (!dict_uiobject.ContainsKey(basePanel.uiType.Name))
        {
            dict_uiobject.Add(basePanel.uiType.Name, ui_object);
        }

        stack_ui.Push(basePanel);

        // 3. 激活新 UI（同样使用延迟，防止吃掉触发 Push 的那个按键）
        ui_object.SetActive(true);
        // 借用 Canvas 上的任意脚本开启协程
        CanvasObj.GetComponent<MonoBehaviour>().StartCoroutine(DelayOnEnable(basePanel));
    }

    public void Pop(bool isAll)
    {
        if (stack_ui.Count <= 0 || isTransitioning) return;

        if (isAll)
        {
            while (stack_ui.Count > 0) CloseTopPanel();
        }
        else
        {
            CloseTopPanel();

            // 恢复下层 UI
            if (stack_ui.Count > 0)
            {
                BasePanel nextPanel = stack_ui.Peek();
                // 核心修复：延迟激活下层 UI，避开当前帧的输入残留
                CanvasObj.GetComponent<MonoBehaviour>().StartCoroutine(DelayRestore(nextPanel));
            }
        }
    }

    private void CloseTopPanel()
    {
        BasePanel topPanel = stack_ui.Pop();
        topPanel.OnDisable();

        PoolManager.Release(topPanel.ActiveObj);

        if (dict_uiobject.ContainsKey(topPanel.uiType.Name))
        {
            dict_uiobject.Remove(topPanel.uiType.Name);
        }
    }

    // --- 延迟处理逻辑 ---

    private IEnumerator DelayOnEnable(BasePanel panel)
    {
        isTransitioning = true;
        yield return new WaitForSecondsRealtime(0.001f); // 0.1秒足以让按键状态重置
        panel.OnEnable();
        isTransitioning = false;
    }

    private IEnumerator DelayRestore(BasePanel panel)
    {
        isTransitioning = true;
        // 等待一小段时间，确保 Input System 已经处理完当前帧的“按下”事件
        yield return new WaitForSecondsRealtime(0.001f);

        if (panel != null && panel.ActiveObj != null)
        {
            panel.ActiveObj.SetActive(true);
            panel.OnEnable();
        }
        isTransitioning = false;
    }
}