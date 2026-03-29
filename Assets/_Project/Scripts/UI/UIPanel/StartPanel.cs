using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class StartPanel : BasePanel
{
    // 第一个是路径，第二个是 PoolManager 里的 Name
    public static readonly UIType uiType = new UIType("StartPanel");

    public StartPanel() : base(uiType) { }


    public override void OnEnable()
    {
        if (!isCreated) isCreated = true;
        InputManager.Instance.AllActions.AnyKey.performed += OnAnyKey;
    }

    public override void OnDisable(bool a)
    {
        InputManager.Instance.AllActions.AnyKey.performed -= OnAnyKey;
    }

    private void OnAnyKey(InputAction.CallbackContext context)
    {
        UIManager.GetInstance().Pop(false);
        InputManager.Instance.EnablePlayerInput();
    }
}