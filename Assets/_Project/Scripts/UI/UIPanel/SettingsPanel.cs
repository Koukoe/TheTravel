using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MenuPanel
{
    [SerializeField] private Button backBtn;
    [SerializeField] private Button audioBtn;

    protected override void Awake()
    {
        base.Awake();

        backBtn?.onClick.AddListener(() =>
        {
            UIManager.Instance.Pop();
        });

        audioBtn?.onClick.AddListener(() =>
        {
            UIManager.Instance.Push("AudioPanel");
        });
    }

    protected override GameObject DefaultFocused() => backBtn != null ? backBtn.gameObject : null;
}