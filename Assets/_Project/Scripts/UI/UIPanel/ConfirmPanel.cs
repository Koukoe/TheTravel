using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ConfirmPanel : MenuPanel
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private Button confirmBtn;
    [SerializeField] private Button cancelBtn;

    private Action _onConfirm;
    private Action _onCancel;

    protected override void Awake()
    {
        base.Awake();

        confirmBtn?.onClick.AddListener(() =>
        {
            _onConfirm?.Invoke();
            UIManager.Instance.Pop();
        });

        cancelBtn?.onClick.AddListener(() =>
        {
            _onCancel?.Invoke();
            UIManager.Instance.Pop();
        });
    }

    public void Setup(Action onConfirm, Action onCancel = null, string title = null, string content = null)
    {
        if (titleText != null) titleText.text = title;
        if (contentText != null) contentText.text = content;

        _onConfirm = onConfirm;
        _onCancel = onCancel;
    }

    protected override GameObject DefaultFocused() => confirmBtn != null ? confirmBtn.gameObject : null;

    public override void Close(Action onAllFinished = null)
    {
        _onConfirm = null;
        _onCancel = null;
        base.Close(onAllFinished);
    }
}