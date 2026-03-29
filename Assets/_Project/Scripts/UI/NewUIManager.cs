using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    // 单例模式
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
        // 确保 InputManager 已初始化
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

        // 1. 获取或创建面板实例 (反射)
        if (!panelCache.TryGetValue(panelName, out BasePanel basePanel))
        {
            Type type = Type.GetType(panelName);
            if (type == null)
            {
                Debug.LogError($"[UI] 找不到名为 {panelName} 的脚本类！");
                return null;
            }
            basePanel = (BasePanel)Activator.CreateInstance(type);
            panelCache.Add(panelName, basePanel);
        }

        // 2. 处理当前顶层 UI 的失焦
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

        // 3. 从对象池获取物理物体
        bool isFirstCreated;
        GameObject ui_object = PoolManager.Global.Get(basePanel.uiType.Name, out isFirstCreated);
        if (ui_object == null) return null;

        ui_object.transform.SetParent(canvasObj.transform, false);
        basePanel.ActiveObj = ui_object;

        if (!dict_uiobject.ContainsKey(panelName))
        {
            dict_uiobject.Add(panelName, ui_object);
        }

        stack_ui.Push(basePanel);

        // 4. 执行启用逻辑 (直接使用 this 发起协程)
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