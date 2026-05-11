using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }
    public bool isGraphInitialized = false;
    public bool IsGraphInitialized => isGraphInitialized;

    public GameObject mainPlayer; // 主玩家对象
    private bool isLoadingTasks = false;  // 防止加载时重复保存

    private HashSet<TaskNode> activeTasks = new HashSet<TaskNode>();

    public void RegisterActive(TaskNode node) => activeTasks.Add(node);
    public void UnregisterActive(TaskNode node) => activeTasks.Remove(node);

    private void Awake()
    {
        Debug.Log("TaskManager Awake");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(InitTaskGraph());
    }

    private static Dictionary<string, TaskNode> tasks = new Dictionary<string, TaskNode>();

    public void AddTask(string taskId, TaskNode taskNode)
    {
        if (tasks.ContainsKey(taskId))
        {
            Debug.LogError("TaskId already exists: " + taskId);
        }
        else
        {
            tasks.Add(taskId, taskNode);
        }
    }

    public TaskNode GetTask(string taskId)
    {
        if (tasks.ContainsKey(taskId))
        {
            return tasks[taskId];
        }
        else
        {
            Debug.LogError("TaskId does not exist: " + taskId);
            return null;
        }
    }

    IEnumerator InitTaskGraph()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (isGraphInitialized) yield break;

        Debug.Log($"开始初始化任务图，共 {tasks.Count} 个任务");

        // 初始化任务图
        foreach (var task in tasks)
        {
            var taskNode = task.Value;
            SaveTaskNode(task.Key);

            foreach (var id in taskNode.nextNodesIds)
            {
                TaskNode targetTask = TaskManager.Instance.GetTask(id);
                if (targetTask == null)
                {
                    Debug.Log("TaskNode not found: " + id);
                }
                else
                {
                    taskNode.nextNodes.Add(targetTask);
                    targetTask.Inn++;
                    taskNode.Out++;
                }
            }
        }

        // 加载所有任务节点存档（会恢复任务状态）
        LoadAllTaskNodes();

        isGraphInitialized = true;

        // 图初始化完成后，启动所有入度为0且未完成的任务
        StartAllReadyTasks();
    }

    /// <summary>
    /// 启动所有可以开始的任务（入度为0且未完成）
    /// </summary>
    private void StartAllReadyTasks()
    {
        foreach (var task in tasks.Values)
        {
            if (task.Inn == 0 && !task.isTaskFinished)
            {
                Debug.Log($"启动任务: {task.taskId}");
                task.StartTask();
            }
        }
    }

    /// <summary>
    /// 保存所有任务节点到存档
    /// </summary>
    public void SaveAllTaskNodes()
    {
        if (isLoadingTasks) return;  // 加载期间不保存

        foreach (var task in tasks)
        {
            SaveTaskNode(task.Key);
        }
        Debug.Log("所有任务节点已保存");
    }

    /// <summary>
    /// 从存档中加载所有任务节点
    /// </summary>
    public void LoadAllTaskNodes()
    {
        isLoadingTasks = true;

        foreach (var task in tasks)
        {
            if (GameFlowManager.Instance.PlayingData.TaskNodesDic.ContainsKey(task.Key))
            {
                LoadTaskNode(task.Key);
            }
            else
            {
                Debug.Log("未找到对应节点存档: " + task.Key);
            }
        }

        isLoadingTasks = false;
    }

    public void SaveTaskNode(string ID)
    {
        if (isLoadingTasks) return;  // 加载期间不保存

        TaskNode taskNode = GetTask(ID);
        if (taskNode == null)
        {
            Debug.LogError("TaskNode not found: " + ID);
            return;
        }

        if (GameFlowManager.Instance?.PlayingData?.TaskNodesDic == null)
        {
            Debug.LogWarning("GameFlowManager.PlayingData 未就绪，跳过保存");
            return;
        }

        if (GameFlowManager.Instance.PlayingData.TaskNodesDic.ContainsKey(ID))
        {
            GameFlowManager.Instance.PlayingData.TaskNodesDic[ID] = (taskNode.Inn, taskNode.isTaskFinished);
            Debug.Log("已更新任务存档: " + ID);
        }
        else
        {
            GameFlowManager.Instance.PlayingData.TaskNodesDic.Add(ID, (taskNode.Inn, taskNode.isTaskFinished));
            Debug.Log("未找到对应节点存档，已自动创建: " + ID);
        }
    }

    public void LoadTaskNode(string ID)
    {
        TaskNode taskNode = GetTask(ID);
        if (taskNode == null) return;

        if (GameFlowManager.Instance?.PlayingData?.TaskNodesDic.ContainsKey(ID) == true)
        {
            var (inn, isFinished) = GameFlowManager.Instance.PlayingData.TaskNodesDic[ID];

            // 恢复任务核心数值
            taskNode.isTaskFinished = isFinished;
            taskNode.Inn = inn;

            // 执行一次完成表现（比如关闭相关 UI 或触发后续）
            if (isFinished)
            {
            }

            Debug.Log("已加载任务存档: " + ID);
        }
    }

    /// <summary>
    /// 重置并重新启动所有未完成的任务（读档后调用）
    /// </summary>
    public void ReloadAndRestartTasks()
    {
        Debug.Log("重新加载并重启未完成的任务");

        // 1. 取消所有正在运行的任务
        foreach (var task in tasks.Values)
        {
            if (!task.isTaskFinished)
            {
                task.CancelTask();
            }
        }

        // 2. 重新加载任务状态
        LoadAllTaskNodes();

        // 3. 延迟一帧后启动可开始的任务
        StartCoroutine(DelayedStartReadyTasks());
    }

    private IEnumerator DelayedStartReadyTasks()
    {
        yield return null;  // 等待一帧，确保所有状态已恢复

        foreach (var task in tasks.Values)
        {
            if (task.Inn == 0 && !task.isTaskFinished)
            {
                Debug.Log($"重新启动任务: {task.taskId}");
                task.StartTask();
            }
        }
    }

    public void OnGoalReached(TaskGoal goal)
    {
        // 找出目前正在运行的任务中，哪一个包含了这个目标
        foreach (var node in activeTasks)
        {
            if (node.taskGoals.Contains(goal))
            {
                // 立即让该节点检查自己是否全目标完成
                node.RefreshStatus();
                break;
            }
        }
    }


    [ContextMenu("Finish All Active Tasks")]
    public void FinishAllActiveTasks()
    {
        foreach (var node in activeTasks)
        {
            foreach (var goal in node.taskGoals)
            {
                if (goal.targetScript != null)
                    goal.targetScript.FinishTask();
                else
                    goal.IsDone = true;
            }
        }
    }

    // 可视化当前任务
    private void OnGUI()
    {
        if (activeTasks.Count == 0) return;

        GUI.color = Color.red;
        GUILayout.BeginArea(new Rect(15, 15, 300, 500));
        GUILayout.Label("--- ACTIVE TASKS ---", GUI.skin.box);
        foreach (var task in activeTasks)
        {
            GUILayout.Label($"ID: {task.taskId} | Name: {task.taskName}");
        }
        GUILayout.EndArea();
    }
}