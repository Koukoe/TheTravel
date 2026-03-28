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

    // --- 【新增】逻辑类缓存：防止每次 Push 都反射 new 一个 C# 对象 ---
    private Dictionary<string, BasePanel> panelCache = new Dictionary<string, BasePanel>();

    public Dictionary<string, GameObject> dict_uiobject = new Dictionary<string, GameObject>();
    public GameObject CanvasObj;

    private bool isTransitioning = false;

    public UIManager() { instance = this; }

    // --- 【修改】Push 现在直接传类名字符串 ---
    public BasePanel Push(string panelName)
    {
        if (isTransitioning) return null;

        // 1. 获取或创建【逻辑类】实例
        if (!panelCache.TryGetValue(panelName, out BasePanel basePanel))
        {
            // 通过字符串反射获取类型
            Type type = Type.GetType(panelName);
            if (type == null)
            {
                Debug.LogError($"[UI] 找不到名为 {panelName} 的脚本类！");
                return null;
            }
            basePanel = (BasePanel)Activator.CreateInstance(type);
            panelCache.Add(panelName, basePanel);
        }

        // 2. 禁用当前顶层 UI
        if (stack_ui.Count > 0 && !basePanel.IsSubPanel)
        {
            BasePanel top = stack_ui.Peek();
            top.OnDisable();
            // 注意：这里由于后面还要还给池子，现在只是暂时隐藏物理对象
            if (top.ActiveObj != null) top.ActiveObj.SetActive(false);
        }

        // 3. 获取【物理对象】 (对接你的 PoolManager)
        if (CanvasObj == null) CanvasObj = UIMethods.GetInstance().FindCanvas();

        bool isFirstCreated;
        // 调用 PoolManager 获取 GameObject
        GameObject ui_object = PoolManager.Global.Get(basePanel.uiType.Name, out isFirstCreated);

        if (ui_object == null) return null;

        ui_object.transform.SetParent(CanvasObj.transform, false);
        basePanel.ActiveObj = ui_object;

        // 4. 初始化
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

        // 5. 激活与延迟回调
        // PoolManager 已经 SetActive(true) 了，直接跑协程即可
        CanvasObj.GetComponent<MonoBehaviour>().StartCoroutine(DelayOnEnable(basePanel));

        return basePanel;
    }

    // --- 【配套修改】Close 逻辑 ---
    private void CloseTopPanel()
    {
        if (stack_ui.Count == 0) return;

        BasePanel topPanel = stack_ui.Pop();
        topPanel.OnDisable();

        // 归还到 PoolManager，不要物理销毁
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