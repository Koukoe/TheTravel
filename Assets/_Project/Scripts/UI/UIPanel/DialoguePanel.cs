using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEditor;

public class DialoguePanel : BasePanel
{
    public override void OnOpen()
    {
        base.OnOpen();
        // 绑定输入
        InputManager.Instance.DialogueActions.Submit.performed += Next;

        // 为什么MenuManager写的是只有玩家输入才能打开MainMenu...
        InputManager.Instance.DialogueActions.Cancel.performed += Menu;

        // 切换至对话输入模式，禁用玩家移动操作
        InputManager.Instance.EnableDialogueInput();

        // 对话面板打开时清空焦点，避免沿用上一个UI焦点造成误触发
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        InputManager.Instance.DialogueActions.Submit.performed -= Next;
        InputManager.Instance.DialogueActions.Cancel.performed -= Menu;

        // 恢复玩家输入模式
        InputManager.Instance.EnablePlayerInput();
    }

    private void Next(InputAction.CallbackContext context)
    {
        // 确保转场时不会触发
        if (UIManager.Instance.IsTransitioning) return;

        // 当前句有选项时不响应 Next
        if (DialogueManager.Instance.HasCurrentOptions()) return;

        DialogueManager.Instance.DialogueNext();

    }

    // 为什么MenuManager写的是只有玩家输入才能打开MainMenu...
    public void Menu(InputAction.CallbackContext context)
    {
        // 对话选项按钮没被模糊，有点搞，，，
        EffectManager.Instance.SetBackgroundBlur(true);
        UIManager.Instance.Push("MainPanel");
        InputManager.Instance.EnableUIInput();
    }

}