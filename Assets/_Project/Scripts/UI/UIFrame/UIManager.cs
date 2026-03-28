using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;

public class UIManager
{
    private static UIManager instance;
    public static UIManager GetInstance()
    {
        if (instance == null) instance = new UIManager();
        return instance;
    }

    public Stack<BasePanel> stack_ui = new Stack<BasePanel>();

    private Dictionary<string, BasePanel> panelCache = new Dictionary<string, BasePanel>();

    public Dictionary<string, GameObject> dict_uiobject = new Dictionary<string, GameObject>();
    public GameObject CanvasObj;

    private bool isTransitioning = false;

    public UIManager() { instance = this; }

    public BasePanel Push(string panelName)
    {
        if (isTransitioning) return null;

        // 通过字符串反射获取类型与其对象
        if (!panelCache.TryGetValue(panelName, out BasePanel basePanel))
        {

            Type type = Type.GetType(panelName);
            if (type == null)
            {
                UnityEngine.Debug.LogError($"[UI] 找不到名为 {panelName} 的脚本类！");
                return null;
            }
            basePanel = (BasePanel)Activator.CreateInstance(type);
            panelCache.Add(panelName, basePanel);
        }

        // 禁用当前顶层 UI
        if (stack_ui.Count > 0)
        {
            BasePanel top = stack_ui.Peek();
            if (!basePanel.IsSubPanel)
            {
                top.OnDisable();
                // 压栈
                if (top.ActiveObj != null) top.ActiveObj.SetActive(false);
            }
            else
            {
                UnityEngine.Debug.Log("xixi");
                top.ParentDisfocus();
            }
        }

        // 获取物理对象
        if (CanvasObj == null) CanvasObj = UIMethods.GetInstance().FindCanvas();

        bool isFirstCreated;
        GameObject ui_object = PoolManager.Global.Get(basePanel.uiType.Name, out isFirstCreated);

        if (ui_object == null) return null;

        ui_object.transform.SetParent(CanvasObj.transform, false);
        basePanel.ActiveObj = ui_object;

        // 初始化
        if (isFirstCreated)
        {
            // 如果是池子第一次实例化该物体，可以在这里调用一次性初始化
            // basePanel.OnFirstInit(); 
        }

        if (!dict_uiobject.ContainsKey(panelName))
        {
            dict_uiobject.Add(panelName, ui_object);
        }

        stack_ui.Push(basePanel);

        CanvasObj.GetComponent<MonoBehaviour>().StartCoroutine(DelayOnEnable(basePanel));

        return basePanel;
    }

    private void CloseTopPanel(bool a = true)
    {
        if (stack_ui.Count == 0) return;

        BasePanel topPanel = stack_ui.Pop();
        topPanel.OnDisable(a);

        PoolManager.Release(topPanel.ActiveObj);

        // 清理字典映射
        string name = topPanel.GetType().Name;
        if (dict_uiobject.ContainsKey(name))
        {
            dict_uiobject.Remove(name);
        }
    }

    public void Pop(bool isAll)
    {
        if (stack_ui.Count <= 0 || isTransitioning) return;

        if (isAll)
        {
            while (stack_ui.Count > 0) CloseTopPanel(false);
        }
        else
        {
            CloseTopPanel();

            // 恢复下层 UI
            if (stack_ui.Count > 0)
            {
                BasePanel nextPanel = stack_ui.Peek();
                CanvasObj.GetComponent<MonoBehaviour>().StartCoroutine(DelayRestore(nextPanel));

            }
        }
    }


    // 延迟处理下一级面板出现

    private IEnumerator DelayOnEnable(BasePanel panel)
    {
        isTransitioning = true;
        yield return new WaitForSecondsRealtime(0f);
        panel.OnEnable();
        isTransitioning = false;
    }


    // 延迟处理面板恢复
    private IEnumerator DelayRestore(BasePanel panel)
    {
        isTransitioning = true;
        yield return new WaitForSecondsRealtime(0f);

        if (panel != null && panel.ActiveObj != null)
        {
            panel.ActiveObj.SetActive(true);
            panel.Resume();
        }
        isTransitioning = false;
    }
}