using UnityEngine;
using UnityEngine.InputSystem;

public class UISystem : MonoBehaviour
{
    private static UISystem instance;
    public static UISystem GetInstance() => instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InputReader.Instance.PlayerActions.Menu.performed += Menu;

    }

    public void NewGame()
    {
        InputReader.Instance.EnableAllInput();
        UIManager.GetInstance().Push("StartPanel");
    }

    public void Menu(InputAction.CallbackContext context)
    {
        UIManager.GetInstance().Push("MainPanel");
        InputReader.Instance.EnableUIInput();
    }
}