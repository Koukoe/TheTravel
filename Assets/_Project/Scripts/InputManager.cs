using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private GameInput _controls;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _controls = new GameInput();

            EnableUIInput();
        }
        else { Destroy(gameObject); }
    }

    private void OnEnable()
    {
        if (_controls != null)
        {
            _controls.Enable();
        }
    }

    private void OnDisable()
    {
        if (_controls != null)
        {
            _controls.Disable();
        }
    }

    /// <summary>
    /// 玩家操作：移动、交互
    /// </summary>
    public void EnablePlayerInput()
    {
        _controls.UI.Disable();
        _controls.Dialogue.Disable();
        _controls.All.Disable();
        _controls.Player.Enable();
    }

    /// <summary>
    /// 面板操作：取消 
    /// </summary>
    public void EnableUIInput()  // 其他操作在Eventsystem里，只是没必要再分一个
    {
        _controls.Player.Disable();
        _controls.Dialogue.Disable();
        _controls.All.Disable();
        _controls.UI.Enable();
    }

    /// <summary>
    /// 对话操作
    /// </summary>
    public void EnableDialogueInput()
    {
        _controls.Player.Disable();
        _controls.UI.Disable();
        _controls.All.Disable();
        _controls.Dialogue.Enable();
    }

    /// <summary>
    /// AnyKey
    /// </summary>
    public void EnableAllInput()
    {
        _controls.Player.Disable();
        _controls.UI.Disable();
        _controls.All.Enable();
    }

    // 获取移动向量
    public Vector2 GetMove() => _controls.Player.Move.ReadValue<Vector2>();

    public GameInput.PlayerActions PlayerActions => _controls.Player;
    public GameInput.UIActions UIActions => _controls.UI;
    public GameInput.DialogueActions DialogueActions => _controls.Dialogue;
    public GameInput.AllActions AllActions => _controls.All;
}