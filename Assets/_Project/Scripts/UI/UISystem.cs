using UnityEngine;

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
        // 启动第一个面板
        UIManager.GetInstance().Push("StartPanel");
    }
}