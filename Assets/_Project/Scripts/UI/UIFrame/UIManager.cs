using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum CanvasRender
{
    OVERLAY = 0,
    CAMERA,
    WORLD,
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject overlayCanvasObj;
    [SerializeField] private GameObject cameraCanvasObj;
    [SerializeField] private GameObject worldCanvasObj;

    [SerializeField] private Volume blurVolume;

    public GameObject CanvasObj(CanvasRender m = CanvasRender.OVERLAY) =>
    m == CanvasRender.OVERLAY ? overlayCanvasObj : (m == CanvasRender.CAMERA ? cameraCanvasObj : worldCanvasObj);

    public Stack<BasePanel> stack_ui = new Stack<BasePanel>();
    public List<BasePanel> list_ui = new List<BasePanel>();  // 先预留吧，看看有没有用

    [SerializeField] private bool isTransitioning = false;
    public bool IsTransitioning => isTransitioning;

    private BasePanel closingPanel = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // if (canvasObj == null)
            //     canvasObj = GameObject.FindObjectOfType<Canvas>()?.gameObject; 三个还是自己拖吧
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
        if (isTransitioning && closingPanel != null && closingPanel.name.Contains(panelName))
        {
            BasePanel recoveredPanel = closingPanel;
            recoveredPanel.Abort(); // 停止当前的 Close 协程
            closingPanel = null;    // 清除离场标记

            recoveredPanel.transform.SetAsLastSibling();
            stack_ui.Push(recoveredPanel); // 重新入栈
            recoveredPanel.Open(() => isTransitioning = false); // 重新开始 Open 动画
            return recoveredPanel;
        }

        // 处理其他面板的转场，保持锁定
        if (isTransitioning) return null;

        // 获取对象
        GameObject ui_object = PoolManager.Global.Get(panelName);
        if (ui_object == null) return null;

        BasePanel basePanel = ui_object.GetComponent<BasePanel>();
        if (basePanel == null) return null;

        basePanel.transform.SetParent(CanvasObj(basePanel.CanvasRenderMode).transform, false);  // 设置为 Canvas 的子物体
        basePanel.transform.SetAsLastSibling();  // 移动到最新一位

        // 处理旧面板
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
        closingPanel = topPanel;

        topPanel.Close(() =>
        {
            Debug.Log($"[UI] Close回调触发. 当前关闭的Panel: {topPanel.name}, 标记的closingPanel: {(closingPanel != null ? closingPanel.name : "null")}");
            if (closingPanel == topPanel)
            {
                PoolManager.Release(topPanel.gameObject);
                closingPanel = null;

                if (stack_ui.Count > 0)
                {
                    stack_ui.Peek().Resume(() => isTransitioning = false);
                }
                else
                {
                    isTransitioning = false;
                }
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
        closingPanel = null;
        while (stack_ui.Count > 0)
        {
            BasePanel panel = stack_ui.Pop();
            panel.Abort();  // 清理协程
            panel.gameObject.SetActive(false);
            PoolManager.Release(panel.gameObject);
        }
        isTransitioning = false;
    }



    public void SetBackgroundBlur(bool enable)
    {
        float target = enable ? 1f : 0f;

        StopAllCoroutines();
        StartCoroutine(FadeBlur(target));
    }

    private System.Collections.IEnumerator FadeBlur(float targetWeight)
    {
        float startWeight = blurVolume.weight;
        float time = 0;
        while (time < 0.5f)
        {
            time += Time.deltaTime;
            blurVolume.weight = Mathf.Lerp(startWeight, targetWeight, time / 0.5f);
            yield return null;
        }
        blurVolume.weight = targetWeight;
    }
}