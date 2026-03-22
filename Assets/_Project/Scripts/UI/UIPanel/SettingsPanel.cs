using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;  // 添加这一行，解决错误1

public class SettingsPanel : BasePanel
{
    public static readonly UIType uiType = new UIType("SettingsPanel");

    private Button backBtn;
    private Button[] allButtons;

    public SettingsPanel() : base(uiType) { }

    public override void OnEnable()
    {
        base.OnEnable();

        if (!isCreated)
        {
            // 获取所有按钮（用于键盘/手柄导航）
            allButtons = ActiveObj.GetComponentsInChildren<Button>();

            // 配置返回按钮
            backBtn = UIMethods.GetInstance().FindObjectInChild<Button>(ActiveObj, "Back");
            if (backBtn != null)
            {
                Debug.Log("找到返回按钮，添加监听");

                // 配置按钮视觉效果（鼠标悬停、键盘/手柄选中、按下）
                ConfigureButtonVisuals(backBtn);

                // 添加点击事件（鼠标、键盘、手柄都会触发）
                backBtn.onClick.RemoveAllListeners();
                backBtn.onClick.AddListener(() =>
                {
                    Debug.Log("返回按钮被点击");
                    OnBackButtonClicked();
                });
                
            }

            // 配置所有按钮的导航（键盘/手柄需要）
            ConfigureNavigation();

            isCreated = true;
        }

        // 订阅输入事件（键盘/手柄）
        SubscribeInputEvents();

        // 设置默认选中的按钮（键盘/手柄需要）
        SetDefaultSelectedButton();

        // 显示鼠标光标（如果需要）
        ShowMouseCursor(true);
    }

    public override void OnDisable()
    {
        // 取消订阅输入事件
        UnsubscribeInputEvents();

        base.OnDisable();
    }

    // ==================== 配置方法 ====================

    /// <summary>
    /// 配置按钮的视觉效果（鼠标和手柄都生效）
    /// </summary>
    private void ConfigureButtonVisuals(Button button)
    {
        // 通过 Color Tint
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;                    // 普通状态
        colors.highlightedColor = new Color(1, 0.9f, 0.5f);  // 鼠标悬停：淡黄色
        colors.selectedColor = new Color(0.5f, 1, 0.5f);     // 键盘/手柄选中：淡绿色
        colors.pressedColor = new Color(1, 0.5f, 0.5f);      // 按下：淡红色
        colors.fadeDuration = 0.1f;                          // 过渡时间
        button.colors = colors;

        // 添加轮廓效果（键盘/手柄选中时更明显）
        Outline outline = button.gameObject.GetComponent<Outline>();
        if (outline == null)
            outline = button.gameObject.AddComponent<Outline>();
        outline.effectColor = Color.green;
        outline.effectDistance = new Vector2(3, 3);
        outline.enabled = false;

        // 添加事件触发器
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        // 选中时显示轮廓
        EventTrigger.Entry selectEntry = new EventTrigger.Entry();
        selectEntry.eventID = EventTriggerType.Select;
        selectEntry.callback.AddListener((data) => outline.enabled = true);
        trigger.triggers.Add(selectEntry);

        // 取消选中时隐藏轮廓
        EventTrigger.Entry deselectEntry = new EventTrigger.Entry();
        deselectEntry.eventID = EventTriggerType.Deselect;
        deselectEntry.callback.AddListener((data) => outline.enabled = false);
        trigger.triggers.Add(deselectEntry);

        // 鼠标悬停时显示轮廓
        EventTrigger.Entry hoverEntry = new EventTrigger.Entry();
        hoverEntry.eventID = EventTriggerType.PointerEnter;
        hoverEntry.callback.AddListener((data) =>
        {
            if (EventSystem.current.currentSelectedGameObject != button.gameObject)
                outline.enabled = true;
        });
        trigger.triggers.Add(hoverEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) =>
        {
            if (EventSystem.current.currentSelectedGameObject != button.gameObject)
                outline.enabled = false;
        });
        trigger.triggers.Add(exitEntry);
    }

    /// <summary>
    /// 配置键盘/手柄导航
    /// </summary>
    private void ConfigureNavigation()
    {
        if (allButtons == null || allButtons.Length == 0) return;

        // 自动导航
        foreach (var button in allButtons)
        {
            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Automatic;
            button.navigation = nav;
        }
    }

    /// <summary>
    /// 设置默认选中的按钮（键盘/手柄需要）
    /// </summary>
    private void SetDefaultSelectedButton()
    {
        if (EventSystem.current == null)
        {
            Debug.LogWarning("EventSystem not found! Creating one...");
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            // 如果使用新版 Input System
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<InputSystemUIInputModule>();
#endif
        }

        // 优先选中返回按钮
        if (backBtn != null)
        {
            EventSystem.current.SetSelectedGameObject(backBtn.gameObject);
        }
        // 如果没有返回按钮，选中第一个按钮
        else if (allButtons != null && allButtons.Length > 0)
        {
            EventSystem.current.SetSelectedGameObject(allButtons[0].gameObject);
        }
    }

    /// <summary>
    /// 订阅键盘/手柄输入事件
    /// </summary>
    private void SubscribeInputEvents()
    {
        // 修复错误2和3：只检查 InputReader.Instance，UIActions 是结构体不能检查 null
        if (InputReader.Instance == null)
        {
            Debug.LogError("InputReader.Instance is null! Make sure InputReader exists in scene.");
            return;
        }

        // 直接使用 UIActions，它不会是 null
        InputReader.Instance.UIActions.Submit.performed += OnSubmit;
        InputReader.Instance.UIActions.Navigate.performed += OnNavigate;
        InputReader.Instance.UIActions.Cancel.performed += OnCancel;

        Debug.Log("ettings 已订阅键盘/手柄输入事件");
    }

    /// <summary>
    /// 取消订阅输入事件
    /// </summary>
    private void UnsubscribeInputEvents()
    {
        if (InputReader.Instance == null)
            return;

        InputReader.Instance.UIActions.Submit.performed -= OnSubmit;
        InputReader.Instance.UIActions.Navigate.performed -= OnNavigate;
        InputReader.Instance.UIActions.Cancel.performed -= OnCancel;
    }

    // ==================== 输入事件处理方法 ====================

    /// <summary>
    /// 提交按钮（键盘：Enter/Space，手柄：A键）
    /// </summary>
    private void OnSubmit(InputAction.CallbackContext context)
    {
        Debug.Log("键盘/手柄：确认按钮被按下");

        // 获取当前选中的按钮
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            Button selectedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            if (selectedButton != null && selectedButton.interactable)
            {
                Debug.Log($"执行按钮: {selectedButton.name}");
                selectedButton.onClick.Invoke();
            }
        }
    }

    /// <summary>
    /// 导航（键盘：方向键/WASD，手柄：左摇杆/方向键）
    /// </summary>
    private void OnNavigate(InputAction.CallbackContext context)
    {
        // EventSystem 会自动处理导航
        Vector2 navigationValue = context.ReadValue<Vector2>();
        if (navigationValue.magnitude > 0.5f)
        {
            // 播放导航音效（可选）
            Debug.Log($"键盘/手柄：导航方向 {navigationValue}");
        }
    }

    /// <summary>
    /// 取消按钮（键盘：ESC，手柄：B键）
    /// </summary>
    private void OnCancel(InputAction.CallbackContext context)
    {
        Debug.Log("键盘/手柄：取消按钮被按下，返回上一页");
        OnBackButtonClicked();
    }

    // ==================== 业务逻辑方法 ====================

    /// <summary>
    /// 返回按钮点击逻辑
    /// </summary>
    private void OnBackButtonClicked()
    {
        // 清除选中状态（避免关闭后按钮还亮着）
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // 关闭当前面板
        UIManager.GetInstance().Pop(false);
    }

    /// <summary>
    /// 显示/隐藏鼠标光标
    /// </summary>
    private void ShowMouseCursor(bool show)
    {
        Cursor.visible = show;

        if (show)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}