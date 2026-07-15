using Unity.VisualScripting;
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
    protected override void Awake()
    {
        base.Awake();

        backBtn?.onClick.AddListener(OnBackClicked);

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
            Debug.Log("Control Clicked");
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
        _temp = DataSettingSystem.GetDeep();
    }

    public override void OnClose()
    {
        base.OnClose();
        _temp = null;
    }

    private void OnDefaultClicked()
    {
        DataSettingSystem.Reset();
        _temp = DataSettingSystem.GetDeep();
    }

    private void OnApplyClicked()
    {
        if (DataSetting.IsDataSettingSame(_temp, DataSettingSystem.GetShallow())) return;
        DataSettingSystem.Set(_temp);
        DataSettingSystem.Save();
        MenuManager.Instance.ApplySettings(_temp);
        Debug.Log("设置已应用并保存");
    }

    public override void OnBackClicked()
    {
        if (UIManager.Instance.IsTransitioning) return;

        if (DataSetting.IsDataSettingSame(_temp, DataSettingSystem.GetShallow()))
        {
            if (isCancelClosable) UIManager.Instance.Pop();
        }
        else
        {
            var panel = UIManager.Instance.Push<ConfirmPanel>("ConfirmPanel");
            panel.Setup(onConfirm: base.OnBackClicked, title: "", content: "");
        }
    }

    protected override GameObject DefaultFocused() => graphicsBtn != null ? graphicsBtn.gameObject : null;
}