using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using UnityEngine;

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
                TaskInit();
            }
        }
    }

    void Awake()
    {
        TaskManager.Instance.AddTask(taskId, this);
        DontDestroyOnLoad(gameObject);
    }


    void TaskInit()
    {
        if (isTaskFinished) return;
        StartCoroutine(StartTask());
    }

    IEnumerator StartTask()
    {
        Debug.Log("Start Task: " + taskName + " " + taskId);
        foreach (var effect in taskEffects)
        {
            effect.ApplyEffect();
        }

        yield return StartCoroutine(CheckTaskFinished());

        foreach (var effect in taskEffects)
        {
            effect.RevertEffect();
        }
        foreach (var effect in taskEndEffects)
        {
            effect.ApplyEffect();
        }
        foreach (var node in nextNodes)
        {
            node.Inn--;
        }
    }

    IEnumerator CheckTaskFinished()
    {
        while (!isTaskFinished)
        {
            bool flag = true;
            foreach (var goal in taskGoals)
            {
                if (!goal.IsDone)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                isTaskFinished = true;
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
