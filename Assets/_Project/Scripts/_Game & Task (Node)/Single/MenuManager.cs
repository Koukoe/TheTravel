using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(1)]
public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    private readonly (int width, int height)[] resolutionPresets = new (int, int)[]
    {
        (1280, 720),
        (1920, 1080),
        (2560, 1440),
        (3840, 2160)
    };
    float[] sensitivityValues = { 0.5f, 1.0f, 1.5f, 2.0f, 2.5f };
    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        Debug.Log("订阅 Menu");
        InputManager.Instance.PlayerStaticActions.Menu.performed += OnMenu;
        if (InputManager.Instance != null)
        {

        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.PlayerStaticActions.Menu.performed -= OnMenu;
        }
    }

    public void SetEnabled(bool value = true) => this.enabled = value;


    public void OnMenu(InputAction.CallbackContext context) => Menu();

    public void Menu()
    {
        EffectManager.Instance.SetBackgroundBlur(true);
        if (UIManager.Instance.IsTransitioning) return;
        if (InputManager.Instance.PlayerDialogueActions.enabled)
        {
            UIManager.Instance.Push("MainBrokenPanel");
        }
        else { UIManager.Instance.Push("MainPanel"); }
        InputManager.Instance.SwitchUIMode(true);
    }

    public void ApplySettings(DataSetting d)
    {
        // 应用音量
        var a = AudioManager.Instance;
        a.SetGroupVolume("MasterVol", d.masterVolumeIndex / 4f);
        a.SetGroupVolume("MusicVol", d.musicVolumeIndex / 4f);
        a.SetGroupVolume("SFXVol", d.sfxVolumeIndex / 4f);
        a.SetGroupVolume("AmbVol", d.ambVolumeIndex / 4f);

        // 应用分辨率
        int idx = Mathf.Clamp(d.resolutionIndex, 0, resolutionPresets.Length - 1);
        var res = resolutionPresets[idx];
        Screen.SetResolution(res.width, res.height, d.isFullScreen == 1);

        // 应用画质
        QualitySettings.SetQualityLevel(d.qualityLevel, true);
        QualitySettings.vSyncCount = d.vSync ? 1 : 0;

        // 应用鼠标灵敏度
        float[] sensitivityValues = { 0.5f, 1.0f, 1.5f, 2.0f, 2.5f };
        float sens = sensitivityValues[Mathf.Clamp(d.sensitivityIndex, 0, sensitivityValues.Length - 1)];
        PlayerPrefs.SetFloat("MouseSensitivity", sens);

        // 同步设置到 DataSettingSystem
        DataSettingSystem.Set(d);
    }
}