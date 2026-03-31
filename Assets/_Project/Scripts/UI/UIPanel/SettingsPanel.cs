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
        });

        audioBtn?.onClick.AddListener(() =>
        {
        });

        languageBtn?.onClick.AddListener(() =>
        {
        });

        controlBtn?.onClick.AddListener(() =>
        {
        });

        applyBtn?.onClick.AddListener(() =>
        {
        });

        defaultBtn?.onClick.AddListener(() =>
        {
        });
    }

    protected override GameObject DefaultFocused() => backBtn != null ? backBtn.gameObject : null;
}