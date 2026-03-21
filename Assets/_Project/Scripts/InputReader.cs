using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    public static InputReader Instance { get; private set; }

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
        else
        {
            Destroy(gameObject);
        }
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
    /// 切换玩家输入模式。
    /// </summary>
    public void EnablePlayerInput()
    {
        _controls.UI.Disable();
        _controls.Player.Enable();
    }

    /// <summary>
    /// 切换UI输入模式。
    /// </summary>
    public void EnableUIInput()
    {
        _controls.Player.Disable();
        _controls.UI.Enable();
    }

    // 获取移动向量
    public Vector2 GetMove() => _controls.Player.Move.ReadValue<Vector2>();

    public GameInput.PlayerActions PlayerActions => _controls.Player;
    public GameInput.UIActions UIActions => _controls.UI;
}