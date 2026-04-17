using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEditor;

public class DialoguePanel : BasePanel
{
    private bool sawStackPanelWhileDialogueActive = false;  // 用于解决对话中打开其他面板后输入变化导致无法继续对话的问题

    public override void OnOpen()
    {
        base.OnOpen();
        // 绑定输入
        InputManager.Instance.PlayerDialogueActions.Next.performed += Next;

        // 为什么MenuManager写的是只有玩家输入才能打开MainPanel...
        // 宝宝你的问题解决了

        // 切换至对话输入模式，禁用玩家移动操作
        InputManager.Instance.SwitchPlayerMode(false);

        // 对话面板打开时清空焦点，避免沿用上一个UI焦点造成误触发
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        sawStackPanelWhileDialogueActive = false;
    }

    public override void OnClose()
    {
        base.OnClose();
        InputManager.Instance.PlayerDialogueActions.Next.performed -= Next;

        // 恢复玩家输入模式
        InputManager.Instance.SwitchPlayerMode(true);
        sawStackPanelWhileDialogueActive = false;
    }

    // 用于解决对话中打开其他面板后输入变化导致无法继续对话的问题
    private void LateUpdate()
    {
        if (!gameObject.activeInHierarchy || UIManager.Instance == null)
        {
            return;
        }

        bool hasStackPanel = UIManager.Instance.Count > 0;
        if (hasStackPanel)
        {
            sawStackPanelWhileDialogueActive = true;
            return;
        }

        // 栈面板(如MainPanel)关闭后，恢复对话输入图，保证对话可继续
        if (sawStackPanelWhileDialogueActive)
        {
            InputManager.Instance.SwitchPlayerMode(false);
            sawStackPanelWhileDialogueActive = false;
        }
    }

    private void Next(InputAction.CallbackContext context)
    {
        // 确保转场时不会触发
        if (UIManager.Instance.IsTransitioning) return;

        // 当前句有选项时不响应 Next
        if (DialogueManager.Instance.HasCurrentOptions()) return;

        DialogueManager.Instance.DialogueNext();

    }

    // 为什么MenuManager写的是只有玩家输入才能打开MainPanel...
    public void Menu(InputAction.CallbackContext context)
    {
        // 对话选项按钮没被模糊，有点搞，，，
        EffectManager.Instance.SetBackgroundBlur(true);
        UIManager.Instance.Push("MainPanel");
        InputManager.Instance.SwitchUIMode();
    }

}