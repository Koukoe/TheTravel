using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }
    public bool isGraphInitialized = false;

    public bool IsGraphInitialized => isGraphInitialized;
    private void Awake()
    {
        Debug.Log("TaskManager Awake");
        if (Instance == null)
        {
            Instance = this;
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
        isGraphInitialized = true;

        Debug.Log($"开始初始化任务图，共 {tasks.Count} 个任务");

        // Initialize the task graph here
        foreach (var task in tasks)
        {
            var taskNode = task.Value;
            TaskManager.Instance.SaveTaskNode(task.Key);
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

        LoadAllTaskNodes(); // 加载所有任务节点
    }

    /// <summary>
    /// 保存所有任务节点到存档
    /// </summary>
    public void SaveAllTaskNodes()
    {
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
        foreach (var task in tasks)
        {
            try
            {
                LoadTaskNode(task.Key);
            }
            catch
            {
                Debug.Log("未找到对应节点存档" + task.Key);
            }

        }
    }

    public void SaveTaskNode(string ID)
    {
        TaskNode taskNode = GetTask(ID);
        if (taskNode == null)
        {
            Debug.LogError("TaskNode not found: " + ID);
            return;
        }
        if (GameFlowManager.Instance.PlayingData.TaskNodesDic.ContainsKey(ID))
        {
            GameFlowManager.Instance.PlayingData.TaskNodesDic[ID] = (taskNode.Inn, taskNode.isTaskFinished);
            Debug.Log("已更新任务存档" + ID);
        }
        else
        {
            GameFlowManager.Instance.PlayingData.TaskNodesDic.Add(ID, (taskNode.Inn, taskNode.isTaskFinished));
            Debug.Log("未找到对应节点存档，已自动创建" + ID);
        }
    }

    public void LoadTaskNode(string ID)
    {
        TaskNode taskNode = GetTask(ID);
        if (taskNode == null)
        {
            Debug.LogError("TaskNode not found: " + ID);
            return;
        }
        if (GameFlowManager.Instance.PlayingData.TaskNodesDic.ContainsKey(ID))
        {
            taskNode.isTaskFinished = GameFlowManager.Instance.PlayingData.TaskNodesDic[ID].Item2;
            taskNode.Inn = GameFlowManager.Instance.PlayingData.TaskNodesDic[ID].Item1;
            Debug.Log("已加载任务存档" + ID);
        }
        else
        {
            Debug.Log("未找到对应节点存档" + ID);
            throw new System.Exception("未找到对应节点存档" + ID);
        }
    }
}
