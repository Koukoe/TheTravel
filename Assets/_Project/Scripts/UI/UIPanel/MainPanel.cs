using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Runtime.CompilerServices;

public class MainPanel : MenuPanel
{
    [SerializeField] private Button saveBtn;
    [SerializeField] private Button loadBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button aboutBtn;
    [SerializeField] private Button backBtn;

    protected override void Awake()
    {
        base.Awake();

        // 绑定按钮监听事件
        saveBtn?.onClick.AddListener(() => OnArchivesClicked());
        loadBtn?.onClick.AddListener(() => OnArchivesClicked(false));
        settingsBtn?.onClick.AddListener(OnSettingsClicked);
        aboutBtn?.onClick.AddListener(OnAboutClicked);
        backBtn?.onClick.AddListener(OnBackClicked);
    }

    protected override GameObject DefaultFocused() => saveBtn != null ? saveBtn.gameObject : null;

    private void OnArchivesClicked(bool isSave = true)
    {
        if (UIManager.Instance.IsTransitioning) return;
        changeStyle(-1);
        if (UIManager.Instance.Push("ArchivesPanel") is ArchivesPanel p) p.Init();
    }

    private void OnSettingsClicked()
    {
        if (UIManager.Instance.IsTransitioning) return;
        changeStyle(1);
        UIManager.Instance.Push("SettingsPanel");
    }

    private void OnAboutClicked()
    {
        if (UIManager.Instance.IsTransitioning) return;
        changeStyle(1);
        UIManager.Instance.Push("AboutPanel");
        Debug.Log("About Clicked");
    }

    protected override void OnBackClicked()
    {
        base.OnBackClicked();
        EffectManager.Instance.SetBackgroundBlur(false);
        InputManager.Instance.SwitchPlayerMode(true);
    }
}