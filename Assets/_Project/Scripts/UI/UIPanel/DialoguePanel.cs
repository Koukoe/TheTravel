using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class DialoguePanel : BasePanel
{
    public override void OnOpen()
    {
        base.OnOpen();
        // 绑定输入
        InputManager.Instance.PlayerDialogueActions.Next.performed += Next;

        // 为什么MenuManager写的是只有玩家输入才能打开MainPanel...
        // 宝宝你的问题解决了
        // 可以可以

        // 切换至对话输入模式，禁用玩家移动操作
        InputManager.Instance.SwitchPlayerMode(false);

        // 对话面板打开时清空焦点，避免沿用上一个UI焦点造成误触发
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public override void OnResume()
    {
        base.OnResume();
        // 从其他面板恢复时切回对话输入
        InputManager.Instance.SwitchPlayerMode(false);
    }

    public override void OnClose()
    {
        base.OnClose();
        InputManager.Instance.PlayerDialogueActions.Next.performed -= Next;

        // 恢复玩家输入模式
        InputManager.Instance.SwitchPlayerMode(true);
    }

    private void Next(InputAction.CallbackContext context)
    {
        // 确保转场时不会触发
        if (UIManager.Instance.IsTransitioning) return;

        DialogueManager.Instance.DialogueNext();

    }

    public void Menu(InputAction.CallbackContext context)
    {
        EffectManager.Instance.SetBackgroundBlur(true);
        UIManager.Instance.Push("MainPanel");
        InputManager.Instance.SwitchUIMode();
    }

}
