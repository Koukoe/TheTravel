using UnityEngine;
using UnityEngine.UI;

public class AudioPanel : MenuPanel
{
    [SerializeField] private Button backBtn;

    protected override void Awake()
    {
        base.Awake();

        backBtn?.onClick.AddListener(() =>
        {
            UIManager.Instance.Pop();
        });

    }

    protected override GameObject DefaultFocused() => backBtn != null ? backBtn.gameObject : null;

    private void OnVolumeChanged(float value)
    {
    }

    public override void OnOpen()
    {
        base.OnOpen();

    }
}