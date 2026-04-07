using UnityEngine;
using UnityEngine.UI;

public abstract class SettingSubPanel : MenuPanel
{
    [SerializeField] private Button defaultBtn;
    [SerializeField] private Button backBtn;

    protected DataSetting Temp => SettingsPanel.Temp;

    protected override void Awake()
    {
        base.Awake();

        backBtn?.onClick.AddListener(() => UIManager.Instance.Pop());
    }

    public override void OnOpen()
    {
        base.OnOpen();
        RefreshUI();
    }

    public abstract void RefreshUI();

    protected override GameObject DefaultFocused() => defaultBtn != null ? defaultBtn.gameObject : null;
}