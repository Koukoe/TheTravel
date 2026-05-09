using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;
using System;

[DefaultExecutionOrder(1)]
public class TaskNode : MonoBehaviour
{
    public string taskName;
    public string taskId;
    [Header("这个任务节点是哪些节点的前置节点")]
    public List<string> nextNodesIds = new List<string>();
    [Header("这个任务节点的影响物体和属性")]
    public List<TaskEffect> taskEffects = new List<TaskEffect>();

    [Header("这个任务结束后的效果")]
    public List<TaskEffect> taskEndEffects = new List<TaskEffect>();

    [Header("任务节点目标")]
    public List<TaskGoal> taskGoals = new List<TaskGoal>();

    [HideInInspector]
    public List<TaskNode> nextNodes = new List<TaskNode>();
    [HideInInspector]
    public int Out;
    private int In = 0;
    [HideInInspector]
    public bool isTaskFinished = false;

    // 取消相关
    private CancellationTokenSource _taskCts;
    private bool isTaskRunning = false;

    public int Inn
    {
        get { return In; }
        set
        {
            In = value;
            TaskManager.Instance.SaveTaskNode(taskId);
            Debug.Log(taskId + "入度为" + In);

            if (In == 0 && TaskManager.Instance.IsGraphInitialized && !isTaskFinished)
            {
                StartTask();  // 直接调用 StartTask
            }
        }
    }

    void Awake()
    {
        TaskManager.Instance.AddTask(taskId, this);
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        CancelTask();
    }

    /// <summary>
    /// 启动任务（外部可调用）
    /// </summary>
    public void StartTask()
    {
        if (isTaskFinished)
        {
            Debug.Log($"任务 {taskId} 已完成，无需启动");
            return;
        }

        if (isTaskRunning)
        {
            Debug.Log($"任务 {taskId} 已在运行中");
            return;
        }

        StartTaskAsync().Forget();
    }

    /// <summary>
    /// 取消当前正在运行的任务
    /// </summary>
    public void CancelTask()
    {
        if (_taskCts != null)
        {
            _taskCts.Cancel();
            _taskCts.Dispose();
            _taskCts = null;
        }
        isTaskRunning = false;
    }

    /// <summary>
    /// 重置任务状态（用于读档后重新启动）
    /// </summary>
    public void ResetForLoad()
    {
        CancelTask();
        isTaskFinished = false;
        isTaskRunning = false;
        // 注意：TaskGoal 的 isDone 状态需要单独处理
    }

    private async UniTaskVoid StartTaskAsync()
    {
        // 取消之前的任务
        CancelTask();

        // 创建新的取消令牌
        _taskCts = new CancellationTokenSource();
        var token = _taskCts.Token;
        isTaskRunning = true;

        Debug.Log("Start Task: " + taskName + " " + taskId);

        try
        {
            // 应用开始效果
            foreach (var effect in taskEffects)
            {
                if (token.IsCancellationRequested) return;
                effect.ApplyEffect();
            }

            // 等待任务完成（可取消）
            await CheckTaskFinishedAsync(token);

            if (token.IsCancellationRequested) return;

            // 任务完成后的效果
            foreach (var effect in taskEffects)
            {
                if (token.IsCancellationRequested) return;
                effect.RevertEffect();
            }
            foreach (var effect in taskEndEffects)
            {
                if (token.IsCancellationRequested) return;
                effect.ApplyEffect();
            }

            // 减少后继节点的入度
            foreach (var node in nextNodes)
            {
                if (token.IsCancellationRequested) return;
                node.Inn--;
            }

            Debug.Log($"任务 {taskId} 完成");
        }
        catch (OperationCanceledException)
        {
            Debug.Log($"任务 {taskId} 被取消");
        }
        finally
        {
            isTaskRunning = false;
        }
    }

    private async UniTask CheckTaskFinishedAsync(CancellationToken token)
    {
        while (!isTaskFinished)
        {
            // 检查取消
            token.ThrowIfCancellationRequested();

            bool allDone = true;

            foreach (var goal in taskGoals)
            {
                // 检查取消
                if (token.IsCancellationRequested) return;

                // 使用异步等待检查结果
                if (!await goal.IsDoneAsync())
                {
                    allDone = false;
                }
            }

            if (allDone)
            {
                isTaskFinished = true;
                // 任务完成时保存
                TaskManager.Instance.SaveTaskNode(taskId);
                return;
            }

            // 等待 0.5 秒后继续检查
            await UniTask.Delay(500, cancellationToken: token);
        }
    }
}