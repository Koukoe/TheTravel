using System.Diagnostics;
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
        }
        else { Destroy(gameObject); }
    }

    private void OnEnable()
    {
        if (_controls != null) SwitchUIMode();
    }

    private void OnDisable() => _controls?.Disable();


    /// <summary>
    /// 切换为面板操作：取消
    /// <paramref name="isMenu"/> 是 Sys（菜单）面板，关闭PlayerSta；是 Game 面板，则无视
    /// </summary>
    public void SwitchUIMode(bool isSys = true)  // 其他操作在Eventsystem里，只是没必要再分一个
    {
        UnityEngine.Debug.Log("切换至 UI Map");
        _controls.PlayerDyn.Disable();
        _controls.PlayerDia.Disable();
        _controls.All.Disable();
        if (isSys) { SetPlayerStaticMode(false); }
        _controls.UI.Enable();
    }

    /// <summary>
    /// 切换为 AnyKey
    /// </summary>
    public void SwitchAllMode()
    {
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

    // 获取移动向量
    public Vector2 GetMove() => _controls.PlayerDyn.Move.ReadValue<Vector2>();

    public GameInput.PlayerDynActions PlayerDynamicActions => _controls.PlayerDyn;
    public GameInput.PlayerDiaActions PlayerDialogueActions => _controls.PlayerDia;
    public GameInput.PlayerStaActions PlayerStaticActions => _controls.PlayerSta;
    public GameInput.UIActions UIActions => _controls.UI;
    public GameInput.AllActions AllActions => _controls.All;
}