using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MainPanel : MenuPanel
{
    [SerializeField] private Button saveBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button backBtn;

    protected override void Awake()
    {
        base.Awake();

        // 绑定按钮监听事件
        settingsBtn?.onClick.AddListener(OnSettingsClicked);
        backBtn?.onClick.AddListener(OnBackClicked);
    }

    protected override GameObject DefaultFocused() => saveBtn != null ? saveBtn.gameObject : null;

    public override void OnOpen()
    {
        base.OnOpen();
        InputManager.Instance.UIActions.Cancel.performed += OnCancelPressed;
    }

    public override void OnSuspend()
    {
        base.OnSuspend();
        InputManager.Instance.UIActions.Cancel.performed -= OnCancelPressed;
    }

    public override void OnResume()
    {
        base.OnResume();
        InputManager.Instance.UIActions.Cancel.performed += OnCancelPressed;
    }

    public override void OnClose()
    {
        base.OnClose();
        InputManager.Instance.UIActions.Cancel.performed -= OnCancelPressed;
    }

    #region
    private void OnSettingsClicked()
    {
        if (UIManager.Instance.IsTransitioning) return;
        UIManager.Instance.Push("SettingsPanel");
    }

    private void OnBackClicked()
    {
        if (UIManager.Instance.IsTransitioning) return;

        UIManager.Instance.Pop();
        InputManager.Instance.EnablePlayerInput();
    }

    private void OnCancelPressed(InputAction.CallbackContext context)
    {
        OnBackClicked();
    }
    #endregion
}