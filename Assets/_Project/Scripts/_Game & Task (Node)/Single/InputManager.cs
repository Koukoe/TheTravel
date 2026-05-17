using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Range(0.01f, 1f)] public float mouseLookSensitivity = 0.1f;
    [Range(10f, 500f)] public float gamepadLookSensitivity = 200f;
    [Range(1f, 3f)] public float gamepadLookCurve = 2.0f;  // 手柄二次曲线

    public float mouseZoomSensitivity = 0.05f;
    public float gamepadZoomSensitivity = 10f;

    private GameInput _controls;

    public enum InputMode { UI, All, PlayerDyn, PlayerDia }
    private InputMode _currentMode = InputMode.UI;
    private InputMode _lastMode = InputMode.UI;

    private void Awake()
    {
        Instance = this;
        _controls = new GameInput();
    }

    private void OnEnable() => _controls?.Enable();

    private void OnDisable() => _controls?.Disable();


    public void SetLookSensitivity(float s)
    {
        mouseLookSensitivity = 0.1f * s;
        gamepadLookSensitivity = 10f * s;
    }
    public void SetZoomSensitivity(float s)
    {
        mouseZoomSensitivity = 0.01f * s;
        gamepadZoomSensitivity = 1f * s;
    }

    public void SaveMode()
    {
        _lastMode = _currentMode;
    }

    public void RestoreMode()
    {
        switch (_lastMode)
        {
            case InputMode.UI: SwitchUIMode(); break;
            case InputMode.All: SwitchAllMode(); break;
            case InputMode.PlayerDyn: SwitchPlayerMode(true); break;
            case InputMode.PlayerDia: SwitchPlayerMode(false); break;
        }
    }

    /// <summary>
    /// 切换为面板操作：取消
    /// <paramref name="isMenu"/> 是 Sys（菜单）面板，关闭PlayerSta；是 Game 面板，则无视
    /// </summary>
    public void SwitchUIMode(bool isSys = true)  // 其他操作在Eventsystem里，只是没必要再分一个
    {
        _currentMode = InputMode.UI;
        UnityEngine.Debug.Log("切换至 UI Map");
        _controls.PlayerDyn.Disable();
        _controls.PlayerDia.Disable();
        _controls.All.Disable();
        if (isSys) { SetPlayerStaticMode(false); }
        else { SetPlayerStaticMode(true); }
        _controls.UI.Enable();
    }

    /// <summary>
    /// 切换为 AnyKey
    /// </summary>
    public void SwitchAllMode()
    {
        _currentMode = InputMode.All;
        UnityEngine.Debug.Log("切换至 All Map");
        _controls.PlayerDyn.Disable();
        _controls.PlayerDia.Disable();
        _controls.PlayerSta.Disable();
        _controls.UI.Disable();
        _controls.All.Enable();
    }

    /// <summary>
    /// 切换为玩家操作
    /// <paramref name="isDyn"/> 动态操作：移动、交互 ； 对话操作。二者互斥。一定会开启静态操作
    /// </summary>
    public void SwitchPlayerMode(bool isDyn = true)
    {
        _currentMode = isDyn ? InputMode.PlayerDyn : InputMode.PlayerDia;
        _controls.UI.Disable();
        _controls.All.Disable();

        if (isDyn)
        {
            UnityEngine.Debug.Log("切换至 Player Dynamic Map");
            _controls.PlayerDia.Disable();
            _controls.PlayerDyn.Enable();
        }
        else
        {
            UnityEngine.Debug.Log("切换至 Player Dialogue Map");
            _controls.PlayerDyn.Disable();
            _controls.PlayerDia.Enable();
        }

        SetPlayerStaticMode(true);
    }

    /// <summary>
    /// 启用与禁用玩家静态操作：打开菜单、图鉴
    /// </summary>
    private void SetPlayerStaticMode(bool enable = true)
    {
        if (enable)
        {
            UnityEngine.Debug.Log("开启 Player Static Map");
            _controls.PlayerSta.Enable();
        }
        else
        {
            UnityEngine.Debug.Log("关闭 Player Static Map");
            _controls.PlayerSta.Disable();
        }
    }

    // Update 获取数值
    public Vector2 GetMove() => _controls.PlayerDyn.Move.ReadValue<Vector2>();

    public Vector2 GetLook()
    {
        Vector2 rawLook = _controls.PlayerDyn.Look.ReadValue<Vector2>();

        // 检测手柄操作
        if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
        {
            // 处理手柄曲线
            rawLook.x = Mathf.Sign(rawLook.x) * Mathf.Pow(Mathf.Abs(rawLook.x), gamepadLookCurve);
            rawLook.y = Mathf.Sign(rawLook.y) * Mathf.Pow(Mathf.Abs(rawLook.y), gamepadLookCurve);

            // 乘以感度并补偿时间
            return rawLook * (gamepadLookSensitivity * Time.deltaTime);
        }
        else
        {
            // 鼠标直接应用感度
            return rawLook * mouseLookSensitivity;
        }
    }

    public float GetZoom()
    {
        float rawZoom = _controls.PlayerDyn.Zoom.ReadValue<float>();

        if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
        {
            return rawZoom * gamepadZoomSensitivity * Time.deltaTime;
        }
        else
        {
            return rawZoom * mouseZoomSensitivity;
        }
    }

    public GameInput.PlayerDynActions PlayerDynamicActions => _controls.PlayerDyn;
    public GameInput.PlayerDiaActions PlayerDialogueActions => _controls.PlayerDia;
    public GameInput.PlayerStaActions PlayerStaticActions => _controls.PlayerSta;
    public GameInput.UIActions UIActions => _controls.UI;
    public GameInput.AllActions AllActions => _controls.All;
}