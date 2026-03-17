using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    private void Awake()
    {
        // 单例防重写
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 加载场景
    /// </summary>
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadCoroutine(sceneName));
    }

    private IEnumerator LoadCoroutine(string sceneName)
    {
        // 开启异步加载
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        
        // 更新进度条
        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            Debug.Log($"加载进度: {progress * 100}%");
            yield return null;
        }
    }
}
