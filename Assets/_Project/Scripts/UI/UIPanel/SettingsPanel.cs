using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MenuPanel
{
    [SerializeField] private Button graphicsBtn;
    [SerializeField] private Button audioBtn;
    [SerializeField] private Button languageBtn;
    [SerializeField] private Button controlBtn;
    [SerializeField] private Button applyBtn;
    [SerializeField] private Button defaultBtn;
    [SerializeField] private Button backBtn;

    private static DataSetting _temp;
    public static DataSetting Temp => _temp;
    
    private readonly (int width, int height)[] resolutionPresets = new (int, int)[]
    {
        (1280, 720),
        (1920, 1080),
        (2560, 1440),
        (3840, 2160)
    };//分辨率预设列表（宽, 高）

    protected override void Awake()
    {
        base.Awake();

        backBtn?.onClick.AddListener(() => UIManager.Instance.Pop());

        graphicsBtn?.onClick.AddListener(() =>
        {
            UIManager.Instance.Push("GraphicsPanel");
        });

        audioBtn?.onClick.AddListener(() =>
        {
            UIManager.Instance.Push("AudioPanel");
        });

        languageBtn?.onClick.AddListener(() =>
        {
        });

        controlBtn?.onClick.AddListener(() =>
        {
            UIManager.Instance.Push("ControlPanel");
        });

        applyBtn?.onClick.AddListener(() =>
        {
            var panel = UIManager.Instance.Push<ConfirmPanel>("ConfirmPanel");
            panel.Setup(onConfirm: OnApplyClicked, title: "", content: "");
        });

        defaultBtn?.onClick.AddListener(() =>
        {
            var panel = UIManager.Instance.Push<ConfirmPanel>("ConfirmPanel");
            panel.Setup(onConfirm: OnDefaultClicked, title: "", content: "");
        });
    }

    public override void OnOpen()
    {
        base.OnOpen();
        _temp = DataSettingSystem.Get();
    }

    public override void OnClose()
    {
        base.OnClose();
        _temp = null;
    }

    private void OnDefaultClicked()
    {
        DataSettingSystem.Reset();
        _temp = DataSettingSystem.Get();
    }

    private void OnApplyClicked()
    {
        DataSettingSystem.Set(_temp);
        DataSettingSystem.Save();
        MenuManager.Instance.ApplySettings(_temp);
        Debug.Log("设置已应用并保存");
    }

    protected override GameObject DefaultFocused() => graphicsBtn != null ? graphicsBtn.gameObject : null;
}