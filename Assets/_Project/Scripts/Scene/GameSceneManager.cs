using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#region --- 基础定义 (可以根据需要拆分) ---

/// <summary>
/// 场景逻辑基类：所有具体关卡的逻辑（如 Level1Logic）都要继承它
/// </summary>
public abstract class SceneBase
{
    // 进入场景时触发：用于生成玩家、初始化JSON对话、播放BGM
    public abstract void EnterScene();

    // 离开场景时触发：用于保存关卡分数、清理弹幕、停止音乐
    public abstract void ExitScene();
}

#endregion

/// <summary>
/// 核心场景管理器：支持异步、叠加、预加载及逻辑绑定
/// </summary>
public class GameSceneManager : MonoBehaviour
{
    // 单例访问点
    public static GameSceneManager Instance { get; private set; }

    // 存储场景名与逻辑类的映射表
    private Dictionary<string, SceneBase> sceneDict = new Dictionary<string, SceneBase>();

    // 存储预加载任务的句柄
    private Dictionary<string, AsyncOperation> preloadTasks = new Dictionary<string, AsyncOperation>();

    private void Awake()
    {
        // 确保全局只有一个管理器，且切换场景时不销毁
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

    // ================= [ 核心调用方法 ] =================

    /// <summary>
    /// 【最常用】异步加载场景
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    /// <param name="nextLogic">该场景对应的逻辑类实例（灵魂参数）</param>
    /// <param name="mode">加载模式：Single(替换当前) 或 Additive(叠加到当前)</param>
    public void LoadSceneAsync(string sceneName, SceneBase nextLogic = null, LoadSceneMode mode = LoadSceneMode.Single)
    {
        // 绑定逻辑到字典
        if (nextLogic != null) sceneDict[sceneName] = nextLogic;

        StartCoroutine(LoadCoroutine(sceneName, mode));
    }

    /// <summary>
    /// 【优化用】预加载场景（加载到90%停住，不跳转）
    /// </summary>
    public void PreloadScene(string sceneName)
    {
        if (preloadTasks.ContainsKey(sceneName)) return;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false; // 关键：禁止自动跳转
        preloadTasks.Add(sceneName, op);
        Debug.Log($"[预加载] 场景 {sceneName} 已开始在后台准备...");
    }

    /// <summary>
    /// 【优化用】瞬间激活已经预加载好的场景
    /// </summary>
    public void ActivatePreloadedScene(string sceneName, SceneBase nextLogic = null)
    {
        if (preloadTasks.TryGetValue(sceneName, out AsyncOperation op))
        {
            if (nextLogic != null) sceneDict[sceneName] = nextLogic;
            StartCoroutine(ActivateCoroutine(sceneName, op));
        }
        else
        {
            Debug.LogWarning($"场景 {sceneName} 尚未预加载，将执行普通异步加载。");
            LoadSceneAsync(sceneName, nextLogic);
        }
    }

    /// <summary>
    /// 【功能用】卸载叠加场景（Additive模式专用）
    /// </summary>
    public void UnloadScene(string sceneName)
    {
        if (sceneDict.TryGetValue(sceneName, out SceneBase logic))
        {
            logic.ExitScene(); // 卸载前清理逻辑
        }
        SceneManager.UnloadSceneAsync(sceneName);
    }

    // ================= [ 内部逻辑处理 ] =================

    private IEnumerator LoadCoroutine(string sceneName, LoadSceneMode mode)
    {
        // 1. 如果是替换模式，通知当前场景“交代后事”
        if (mode == LoadSceneMode.Single) HandleExit();

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, mode);

        // 2. 加载循环：可在此处对接UI进度条
        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            // Debug.Log($"正在载入 {sceneName}: {progress * 100}%");
            yield return null;
        }

        // 3. 加载完成，通知新场景“开始工作”
        HandleEnter(sceneName);
    }

    private IEnumerator ActivateCoroutine(string sceneName, AsyncOperation op)
    {
        HandleExit();

        op.allowSceneActivation = true; // 允许Unity跳转场景

        while (!op.isDone) yield return null;

        HandleEnter(sceneName);
        preloadTasks.Remove(sceneName);
    }

    private void HandleExit()
    {
        string currentName = SceneManager.GetActiveScene().name;
        if (sceneDict.TryGetValue(currentName, out SceneBase logic))
        {
            logic.ExitScene();
        }
    }

    private void HandleEnter(string sceneName)
    {
        if (sceneDict.TryGetValue(sceneName, out SceneBase logic))
        {
            logic.EnterScene();
        }
    }
}