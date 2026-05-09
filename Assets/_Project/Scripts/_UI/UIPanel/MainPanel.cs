using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;

public class MainPanel : MenuPanel
{
    [SerializeField] private Button saveBtn;
    [SerializeField] private Button loadBtn;
    [SerializeField] private Button newBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button aboutBtn;
    [SerializeField] private Button backBtn;
    [SerializeField] private Button exitBtn;

    protected override void Awake()
    {
        base.Awake();

        // 绑定按钮监听事件
        saveBtn?.onClick.AddListener(() => OnArchivesClicked());
        loadBtn?.onClick.AddListener(() => OnArchivesClicked(false));
        exitBtn?.onClick.AddListener(OnNewGameClicked);
        settingsBtn?.onClick.AddListener(OnSettingsClicked);
        aboutBtn?.onClick.AddListener(OnAboutClicked);
        backBtn?.onClick.AddListener(OnBackClicked);
        exitBtn?.onClick.AddListener(OnExitClicked);
    }

    protected override GameObject DefaultFocused() => saveBtn != null && saveBtn.interactable ? saveBtn.gameObject : loadBtn.gameObject;

    public override void OnOpen()
    {
        base.OnOpen();
    }

    public override void OnClose()
    {
        base.OnClose();
        EffectManager.Instance.SetBackgroundBlur(false);
    }

    private void OnArchivesClicked(bool isSave = true)
    {
        if (UIManager.Instance.IsTransitioning) return;
        changeStyle(-1);
        if (UIManager.Instance.Push("ArchivesPanel") is ArchivesPanel p) p.Init(isSave);
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

    public override void OnBackClicked()
    {
        base.OnBackClicked();
        EffectManager.Instance.SetBackgroundBlur(false);
    }

    public void OnExitClicked()
    {
        var panel = UIManager.Instance.Push<ConfirmPanel>("ConfirmPanel");
        panel.Setup(onConfirm: Quit, title: "", content: "");
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnNewGameClicked()
    {
        var panel = UIManager.Instance.Push<ConfirmPanel>("ConfirmPanel");
        panel.Setup(onConfirm: () => GameFlowManager.Instance.NewGame().Forget(), title: "", content: ""); ;
    }
}