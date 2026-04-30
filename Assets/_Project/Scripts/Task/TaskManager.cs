using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;
    private bool isGraphInitialized = false;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
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

        if (isGraphInitialized) yield break;
        isGraphInitialized = true;

        Debug.Log($"开始初始化任务图，共 {tasks.Count} 个任务");

        // Initialize the task graph here
        foreach (var task in tasks)
        {
            var taskNode = task.Value;
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
        yield break;
    }
}
