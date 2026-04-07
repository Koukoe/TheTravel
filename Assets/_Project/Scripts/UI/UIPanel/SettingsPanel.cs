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
        });

        defaultBtn?.onClick.AddListener(() =>
        {
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
        Debug.Log("设置已应用并保存。");

        // 这里调用方法来刷新当前的设置效果
    }

    protected override GameObject DefaultFocused() => graphicsBtn != null ? graphicsBtn.gameObject : null;
}