using UnityEngine;
using UnityEngine.InputSystem;

public class InteractTipWorld : BasePanel
{
    private Transform _target;
    private Vector3 _offset;
    [SerializeField] private RectTransform _rect;
    [SerializeField] private CanvasGroup _canvasGroup;

    public void Bind(IInteractable interactable)
    {
        _target = interactable.InteractTransform;
        _offset = interactable.TipOffset;

        // 初始位置同步，防止第一帧闪现
        UpdatePosition();
    }

    private void LateUpdate()
    {
        // 父物体被销毁，自动回收
        if (_target == null)
        {
            UIManager.Instance.Hide(this);
            return;
        }

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        // 世界坐标转屏幕坐标
        Vector3 worldPos = _target.position + _offset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // 背后检查
        if (screenPos.z < 0)
        {
            _canvasGroup.alpha = 0;  // 隐藏
            return;
        }

        // 设置位置
        _canvasGroup.alpha = 1;
        _rect.position = screenPos;
    }

    public void SetFocus(bool s)
    {

    }

}