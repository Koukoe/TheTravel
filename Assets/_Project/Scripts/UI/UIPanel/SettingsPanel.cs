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

    protected override GameObject DefaultFocused() => graphicsBtn != null ? graphicsBtn.gameObject : null;
}