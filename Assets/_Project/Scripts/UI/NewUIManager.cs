using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI 配置")]
    [SerializeField] private GameObject canvasObj; // 直接在编辑器里拖入 Canvas
    public GameObject CanvasObj => canvasObj;

    // UI 栈逻辑数据
    private Stack<BasePanel> stack_ui = new Stack<BasePanel>();
    private Dictionary<string, BasePanel> panelCache = new Dictionary<string, BasePanel>();
    private Dictionary<string, GameObject> dict_uiobject = new Dictionary<string, GameObject>();

    private bool isTransitioning = false;

    #region 生命周期
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 自动兜底查找 Canvas
            if (canvasObj == null)
                canvasObj = GameObject.FindObjectOfType<Canvas>()?.gameObject;
        }
        else { Destroy(gameObject); }
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.PlayerActions.Menu.performed += OnMenuPressed;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.PlayerActions.Menu.performed -= OnMenuPressed;
        }
    }
    #endregion

    #region 输入响应逻辑
    private void OnMenuPressed(InputAction.CallbackContext context)
    {
        Push("MainPanel");
        InputManager.Instance.EnableUIInput();
    }

    public void NewGame()
    {
        InputManager.Instance.EnableAllInput();
        Push("StartPanel");
    }
    #endregion

    #region UI 核心逻辑 (原 C# 类逻辑)
    public BasePanel Push(string panelName)
    {
        if (isTransitioning) return null;

        // 1. 直接从对象池/资源路径获取物理对象
        bool isFirstCreated;
        GameObject ui_object = PoolManager.Global.Get(panelName, out isFirstCreated);
        if (ui_object == null) return null;

        // 2. 获取物体上挂载的脚本（取代反射）
        BasePanel basePanel = ui_object.GetComponent<BasePanel>();
        if (basePanel == null)
        {
            Debug.LogError($"[UI] 预制体 {panelName} 上没有挂载 BasePanel 脚本！");
            return null;
        }

        // 设置父级并记录引用
        ui_object.transform.SetParent(canvasObj.transform, false);
        basePanel.ActiveObj = ui_object;

        // 3. 处理当前顶层 UI 的失焦逻辑（保持不变）
        if (stack_ui.Count > 0)
        {
            BasePanel top = stack_ui.Peek();
            if (!basePanel.IsSubPanel)
            {
                top.OnDisable();
                if (top.ActiveObj != null) top.ActiveObj.SetActive(false);
            }
            else
            {
                top.ParentDisfocus();
            }
        }

        // 4. 压栈并启用
        stack_ui.Push(basePanel);
        StartCoroutine(DelayOnEnable(basePanel));

        return basePanel;
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
            if (stack_ui.Count > 0)
            {
                BasePanel nextPanel = stack_ui.Peek();
                StartCoroutine(DelayRestore(nextPanel));
            }
        }
    }

    private void CloseTopPanel(bool needDisableEffect = true)
    {
        if (stack_ui.Count == 0) return;

        BasePanel topPanel = stack_ui.Pop();
        topPanel.OnDisable(needDisableEffect);

        PoolManager.Release(topPanel.ActiveObj);

        string name = topPanel.GetType().Name;
        dict_uiobject.Remove(name);
    }
    #endregion

    #region 内部协程
    private IEnumerator DelayOnEnable(BasePanel panel)
    {
        isTransitioning = true;
        yield return null; // 延迟一帧，确保物理对象初始化完成
        panel.OnEnable();
        isTransitioning = false;
    }

    private IEnumerator DelayRestore(BasePanel panel)
    {
        isTransitioning = true;
        yield return null;

        if (panel != null && panel.ActiveObj != null)
        {
            panel.ActiveObj.SetActive(true);
            panel.Resume();
        }
        isTransitioning = false;
    }
    #endregion
}