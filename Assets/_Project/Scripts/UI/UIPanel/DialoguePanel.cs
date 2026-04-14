using UnityEngine;
using UnityEngine.InputSystem;

public class DialoguePanel : BasePanel
{
    public override void OnOpen()
    {
        base.OnOpen();
        // 绑定输入
        InputManager.Instance.PlayerActions.Interact.performed += Next;
    }

    public override void OnClose()
    {
        base.OnClose();
        InputManager.Instance.PlayerActions.Interact.performed -= Next;
    }

    private void Next(InputAction.CallbackContext context)
    {
        // 确保转场时不会触发
        if (UIManager.Instance.IsTransitioning) return;

        // 当前句有选项时，等待玩家点击选项按钮，不响应 Next
        if (DialogueManager.Instance.HasCurrentOptions()) return;

        DialogueManager.Instance.DialogueNext();

    }
}