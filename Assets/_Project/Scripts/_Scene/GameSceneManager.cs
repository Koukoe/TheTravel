using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public abstract class SceneBase : MonoBehaviour
{
    public abstract void EnterScene();
    public abstract void ExitScene();
}

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    private SceneBase currentMainLogic;

    // 预加载任务字典
    private readonly Dictionary<string, (UniTask task, CancellationTokenSource cts)> mainPreLoadTasks = new();
    private readonly Dictionary<string, (UniTask task, CancellationTokenSource cts)> additivePreLoadTasks = new();

    // 状态锁
    public bool IsLoading { get; private set; }

    private void Awake()
    {
        Instance = this;
        currentMainLogic = null;
    }

    /// <summary>
    /// 预加载主场景
    /// </summary>
    public void PreloadMain(string sceneName)
    {
        if (mainPreLoadTasks.ContainsKey(sceneName)) return;  // 不重复预加载

        // UniTask 字典存是任务行为，不需要 allowSceneActivation = false
        // LoadMain 调用时如果没加载完会继续等待预加载，而不是再加载
        var cts = new CancellationTokenSource();
        var task = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single).ToUniTask(cancellationToken: cts.Token);
        mainPreLoadTasks.Add(sceneName, (task, cts));  // 传入元组
    }

    /// <summary>
    /// 取消预加载主场景
    /// </summary>
    public void CancelPreloadMain(string sceneName)
    {
        if (mainPreLoadTasks.TryGetValue(sceneName, out var pair))
        {
            pair.cts.Cancel();  // 取消 Task
            pair.cts.Dispose();  // 释放资源
            mainPreLoadTasks.Remove(sceneName);
        }
    }

    /// <summary>
    /// 取消所有预加载主场景
    /// </summary>
    public void CancelAllPreloadMain()
    {
        foreach (var pair in mainPreLoadTasks.Values)
        {
            pair.cts.Cancel();
            pair.cts.Dispose();
        }
        mainPreLoadTasks.Clear();
    }

    /// <summary>
    /// 加载主场景
    /// <paramref name="progress"/> 进度执行的委托
    /// </summary>
    public async UniTask LoadMain(string sceneName, IProgress<float> progress = null)
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            // 执行当前场景的退出逻辑
            currentMainLogic?.ExitScene();
            Debug.Log($"当前场景 {currentMainLogic} 退出成功捏");
            currentMainLogic = null;

            if (mainPreLoadTasks.TryGetValue(sceneName, out var pair))
            {
                mainPreLoadTasks.Remove(sceneName);
                pair.cts.Dispose();
                await pair.task;
            }
            else
            {
                // 没有预加载，开启新异步任务
                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single)
                    .ToUniTask(progress, cancellationToken: this.GetCancellationTokenOnDestroy());
            }

            await UniTask.Yield();  // 等一帧

            // 清空所有主场景预加载
            // CancelAllPreloadMain();

            InitNewMain(sceneName);
            Debug.Log($"加载主场景 {sceneName} 成功捏");
        }
        catch (Exception e)
        {
            Debug.LogError($"加载主场景 {sceneName} 失败: {e.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 预加载叠加场景
    /// </summary>
    public void PreloadAdditive(string sceneName)
    {
        if (additivePreLoadTasks.ContainsKey(sceneName)) return;

        var cts = new CancellationTokenSource();
        var task = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive)
            .ToUniTask(cancellationToken: cts.Token);
        additivePreLoadTasks.Add(sceneName, (task, cts));
    }

    /// <summary>
    /// 取消预加载叠加场景
    /// </summary>
    public void CancelPreloadAdditive(string sceneName)
    {
        if (additivePreLoadTasks.TryGetValue(sceneName, out var pair))
        {
            pair.cts.Cancel();
            pair.cts.Dispose();
            additivePreLoadTasks.Remove(sceneName);
        }
    }

    /// <summary>
    /// 取消所有预加载叠加场景
    /// </summary>
    public void CancelAllPreloadAdditive()
    {
        foreach (var pair in additivePreLoadTasks.Values)
        {
            pair.cts.Cancel();
            pair.cts.Dispose();
        }
        additivePreLoadTasks.Clear();
    }

    /// <summary>
    /// 加载叠加场景
    /// </summary>
    public async UniTask LoadAdditive(string sceneName, IProgress<float> progress = null)
    {
        try
        {
            if (additivePreLoadTasks.TryGetValue(sceneName, out var pair))
            {
                additivePreLoadTasks.Remove(sceneName);
                pair.cts.Dispose();
                await pair.task;
            }
            else
            {
                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive)
                    .ToUniTask(progress, cancellationToken: this.GetCancellationTokenOnDestroy());
            }

            InitAdditive(sceneName);
        }
        catch (Exception e)
        {
            Debug.LogError($"加载叠加场景 {sceneName} 失败: {e.Message}");
        }
    }

    /// <summary>
    /// 卸载叠加场景
    /// </summary>
    public async UniTask UnloadAdditive(string sceneName)
    {
        Scene s = SceneManager.GetSceneByName(sceneName);
        if (s.isLoaded)
        {
            SceneBase logic = FindLogicInScene(s);
            logic?.ExitScene();
            await SceneManager.UnloadSceneAsync(s).ToUniTask();
        }
    }


    private void InitNewMain(string sceneName)
    {
        Scene s = SceneManager.GetSceneByName(sceneName);
        currentMainLogic = FindLogicInScene(s);
        currentMainLogic?.EnterScene();
    }

    private void InitAdditive(string sceneName)
    {
        Scene s = SceneManager.GetSceneByName(sceneName);
        SceneBase logic = FindLogicInScene(s);
        logic?.EnterScene();
    }

    private SceneBase FindLogicInScene(Scene scene)
    {
        var roots = scene.GetRootGameObjects();

        // 精准寻找
        foreach (var root in roots)
        {
            if (root.name == "Scene" && root.TryGetComponent<SceneBase>(out var logic))  // 优先寻找名字为 Scene 的根节点物体
                return logic;
        }

        // 模糊寻找（提供容错）
        foreach (var root in roots)
        {
            if (root.TryGetComponent<SceneBase>(out var logic))
                return logic;
        }

        Debug.LogWarning($"场景 {scene.name} 未找到 SceneBase 逻辑脚本。");
        return null;
    }
}