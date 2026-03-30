using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject canvasObj;
    public GameObject CanvasObj => canvasObj;

    private Stack<BasePanel> stack_ui = new Stack<BasePanel>();

    [SerializeField] private bool isTransitioning = false;
    public bool IsTransitioning => isTransitioning;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (canvasObj == null)
                canvasObj = GameObject.FindObjectOfType<Canvas>()?.gameObject;
        }
        else { Destroy(gameObject); }
    }

    public BasePanel Peek() => stack_ui.Count > 0 ? stack_ui.Peek() : null;
    public int Count => stack_ui.Count;

    /// <summary>
    /// 打开新的面板，挂起旧的面板
    /// </summary>
    public BasePanel Push(string panelName)
    {
        if (isTransitioning) return null;

        // 获取对象
        GameObject ui_object = PoolManager.Global.Get(panelName);
        if (ui_object == null) return null;

        ui_object.transform.SetParent(canvasObj.transform, false);  // 设置为 Canvas 的子物体
        ui_object.transform.SetAsLastSibling();  // 移动到最新一位

        // 处理旧面板
        BasePanel basePanel = ui_object.GetComponent<BasePanel>();
        if (stack_ui.Count > 0) stack_ui.Peek().Suspend();

        stack_ui.Push(basePanel);
        isTransitioning = true;

        // Open 回调
        basePanel.Open(() => isTransitioning = false);

        return basePanel;
    }

    /// <summary>
    /// 关闭当前顶层面板，恢复上一个面板
    /// </summary>
    public void Pop()
    {
        if (stack_ui.Count <= 0 || isTransitioning) return;

        isTransitioning = true;
        BasePanel topPanel = stack_ui.Pop();

        topPanel.Close(() =>
        {
            PoolManager.Release(topPanel.gameObject);

            // 尝试恢复底下的面板
            if (stack_ui.Count > 0)
            {
                stack_ui.Peek().Resume(() => isTransitioning = false);
            }
            else
            {
                isTransitioning = false;
            }
        });
    }

    /// <summary>
    /// 清空所有面板
    /// </summary>
    public void PopAll()
    {
        // 清空不需要等待动画，直接物理切断
        isTransitioning = true;
        while (stack_ui.Count > 0)
        {
            BasePanel panel = stack_ui.Pop();
            panel.Abort();  // 清理协程
            panel.gameObject.SetActive(false);
            PoolManager.Release(panel.gameObject);
        }
        isTransitioning = false;
    }
}