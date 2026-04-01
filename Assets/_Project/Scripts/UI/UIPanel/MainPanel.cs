using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Runtime.CompilerServices;

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

    private void OnSettingsClicked()
    {
        if (UIManager.Instance.IsTransitioning) return;
        UIManager.Instance.Push("SettingsPanel");
    }

    protected override void OnBackClicked()
    {
        base.OnBackClicked();
        UIManager.Instance.SetBackgroundBlur(false);
        InputManager.Instance.EnablePlayerInput();
    }
}