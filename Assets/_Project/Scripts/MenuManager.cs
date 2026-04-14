using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(1)]
public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }

    private void OnEnable()
    {
        InputManager.Instance.PlayerActions.Menu.performed += Menu;
        if (InputManager.Instance != null)
        {

        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.PlayerActions.Menu.performed -= Menu;
        }
    }

    public void SetEnabled(bool value = true) => this.enabled = value;


    public void NewGame()
    {
        // 先清理现在状态


        InputManager.Instance.EnableAllInput();
        UIManager.Instance.Push("StartPanel");
    }

    public void Menu(InputAction.CallbackContext context)
    {
        EffectManager.Instance.SetBackgroundBlur(true);
        UIManager.Instance.Push("MainPanel");
        InputManager.Instance.EnableUIInput();
    }

    public void ApplySettings(DataSetting d)
    {
        var a = AudioManager.Instance;
        a.SetGroupVolume("MasterVol", d.masterVolumeIndex / 4f);
        a.SetGroupVolume("MusicVol", d.musicVolumeIndex / 4f);
        a.SetGroupVolume("SFXVol", d.sfxVolumeIndex / 4f);
        a.SetGroupVolume("AmbVol", d.ambVolumeIndex / 4f);



    }
}