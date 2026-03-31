using UnityEngine;
using UnityEngine.InputSystem;

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
        if (InputManager.Instance != null)
        {
            InputManager.Instance.PlayerActions.Menu.performed += Menu;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.PlayerActions.Menu.performed += Menu;
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
        UIManager.Instance.Push("MainPanel");
        InputManager.Instance.EnableUIInput();
    }
}