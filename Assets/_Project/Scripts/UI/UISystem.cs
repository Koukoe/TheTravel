using UnityEngine;
using UnityEngine.InputSystem;

public class UISystem : MonoBehaviour
{
    public static UISystem Instance { get; private set; }

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
    }

    private void OnDisable()
    {
        InputManager.Instance.PlayerActions.Menu.performed -= Menu;
    }

    public void NewGame()
    {
        InputManager.Instance.EnableAllInput();
        UIManager.Instance.Push("StartPanel");
    }

    public void Menu(InputAction.CallbackContext context)
    {
        UIManager.Instance.Push("MainPanel");
        InputManager.Instance.EnableUIInput();
    }
}