using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using UnityEngine;

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

    [ReadOnly(true)] public List<TaskNode> nextNodes = new List<TaskNode>();
    [Tooltip("这个任务节点的出度")]
    [ReadOnly(true)] public int Out;
    [Tooltip("这个任务节点的入度")]
    [ReadOnly(true)] private int In;

    public bool isTaskFinished = false;


    public int Inn
    {
        get { return In; }
        set
        {
            TaskManager.Instance.SaveTaskNode(taskId);
            In = value;
            Debug.Log(taskName + " " + taskId + "In: " + In);

            if (In == 0)
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
