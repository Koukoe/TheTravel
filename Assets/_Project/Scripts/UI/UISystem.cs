using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

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
        UIManager.GetInstance().Push("StartPanel");
    }

    public void Menu(InputAction.CallbackContext context)
    {
        UIManager.GetInstance().Push("MainPanel");
        InputManager.Instance.EnableUIInput();
    }
}