using System;
using System.Collections.Generic;
using UnityEngine;

public enum CanvasRender
{
    SYS2D = 0,
    SYS3D,
    GAMECAMERA,
    GAMEWORLD,
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject sys2dCanvasObj;
    [SerializeField] private GameObject sys3dCanvasObj;
    [SerializeField] private GameObject gameCameraCanvasObj;
    [SerializeField] private GameObject gameWorldCanvasObj;

    public GameObject CanvasObj(CanvasRender m) => m switch
    {
        CanvasRender.SYS2D => sys2dCanvasObj,
        CanvasRender.SYS3D => sys3dCanvasObj,
        CanvasRender.GAMECAMERA => gameCameraCanvasObj,
        CanvasRender.GAMEWORLD => gameWorldCanvasObj,
        _ => sys2dCanvasObj
    };

    public Stack<BasePanel> _singleStack = new Stack<BasePanel>();
    public List<BasePanel> _singleList = new List<BasePanel>();
    public Dictionary<string, BasePanel> _register = new Dictionary<string, BasePanel>();
    public Queue<string> _msgQueue = new Queue<string>();

    [SerializeField] private bool isTransitioning = false;
    public bool IsTransitioning => isTransitioning;

    [SerializeField] private bool isQueueProcessing = false;

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


    public BasePanel Peek() => _singleStack.Count > 0 ? _singleStack.Peek() : null;
    public int Count => _singleStack.Count;

    /// <summary>
    /// 在栈中打开新的面板，挂起旧的面板
    /// </summary>
    public BasePanel Push(string panelName)
    {
        if (isTransitioning && closingPanel != null && closingPanel.name.Contains(panelName))
        {
            BasePanel recoveredPanel = closingPanel;
            recoveredPanel.Abort(); // 停止当前的 Close 协程
            closingPanel = null;    // 清除离场标记

            recoveredPanel.transform.SetAsLastSibling();
            _singleStack.Push(recoveredPanel); // 重新入栈
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
        if (_singleStack.Count > 0) _singleStack.Peek().Suspend();

        _singleStack.Push(basePanel);
        isTransitioning = true;

        // Open 回调
        basePanel.Open(() => isTransitioning = false);

        return basePanel;
    }

    /// <summary>
    /// 关闭栈当前顶层面板，恢复上一个面板
    /// </summary>
    public void Pop()
    {
        if (_singleStack.Count <= 0 || isTransitioning) return;

        isTransitioning = true;
        BasePanel topPanel = _singleStack.Pop();
        closingPanel = topPanel;

        topPanel.Close(() =>
        {
            Debug.Log($"[UI] Close回调触发. 当前关闭的Panel: {topPanel.name}, 标记的closingPanel: {(closingPanel != null ? closingPanel.name : "null")}");
            if (closingPanel == topPanel)
            {
                PoolManager.Release(topPanel.gameObject);
                closingPanel = null;

                if (_singleStack.Count > 0)
                {
                    _singleStack.Peek().Resume(() => isTransitioning = false);
                }
                else
                {
                    isTransitioning = false;
                }
            }
        });
    }

    /// <summary>
    /// 清空栈中所有面板
    /// </summary>
    public void PopAll()
    {
        // 清空不需要等待动画，直接物理切断
        isTransitioning = true;
        closingPanel = null;
        while (_singleStack.Count > 0)
        {
            BasePanel panel = _singleStack.Pop();
            panel.Abort();  // 清理协程
            panel.gameObject.SetActive(false);
            PoolManager.Release(panel.gameObject);
        }
        isTransitioning = false;
    }


    /// <summary>
    /// 打开并登记一个无层级面板
    /// </summary>
    public BasePanel Show(string panelName)
    {
        // 如果已经登记并显示了，直接返回
        if (_register.TryGetValue(panelName, out var panel)) return panel;

        GameObject obj = PoolManager.Global.Get(panelName);
        BasePanel newPanel = obj.GetComponent<BasePanel>();

        newPanel.transform.SetParent(CanvasObj(newPanel.CanvasRenderMode).transform, false);
        _register.Add(panelName, newPanel);

        newPanel.Open();
        return newPanel;
    }

    /// <summary>
    /// 关闭并注销一个无层级面板
    /// </summary>
    public void Hide(string panelName)
    {
        if (_register.TryGetValue(panelName, out var panel))
        {
            panel.Close(() =>
            {
                PoolManager.Release(panel.gameObject);
                _register.Remove(panelName);
            });
        }
    }


    /// <summary>
    /// 初始化列表面板
    /// </summary>
    public void InitList(params string[] panelNames)
    {
        ClearList();
        foreach (var name in panelNames)
        {
            GameObject obj = PoolManager.Global.Get(name);
            BasePanel panel = obj.GetComponent<BasePanel>();
            panel.transform.SetParent(CanvasObj(panel.CanvasRenderMode).transform, false);
            panel.gameObject.SetActive(false);
            _singleList.Add(panel);
        }
    }

    /// <summary>
    /// 切换列表面板的页面
    /// </summary>
    public void SwitchPage(int index)
    {
        if (index < 0 || index >= _singleList.Count) return;

        for (int i = 0; i < _singleList.Count; i++)
        {
            if (i == index)
            {
                if (!_singleList[i].gameObject.activeSelf) _singleList[i].Open();
            }
            else
            {
                if (_singleList[i].gameObject.activeSelf) _singleList[i].Close();
            }
        }
    }

    /// <summary>
    /// 清理列表面板
    /// </summary>
    public void ClearList()
    {
        foreach (var p in _singleList) PoolManager.Release(p.gameObject);
        _singleList.Clear();
    }
}