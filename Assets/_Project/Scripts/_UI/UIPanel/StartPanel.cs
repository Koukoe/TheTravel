using UnityEngine;
using UnityEngine.InputSystem;

public class StartPanel : BasePanel
{
    public override void OnOpen()
    {
        base.OnOpen();
        // 绑定输入
        InputManager.Instance.AllActions.AnyKey.performed += OnAnyKey;
    }

    public override void OnClose()
    {
        base.OnClose();
        InputManager.Instance.AllActions.AnyKey.performed -= OnAnyKey;
    }

    private void OnAnyKey(InputAction.CallbackContext context)
    {
        // 确保转场时不会触发
        if (UIManager.Instance.IsTransitioning) return;

        UIManager.Instance.Pop();
        InputManager.Instance.SwitchPlayerMode(true);
    }
}