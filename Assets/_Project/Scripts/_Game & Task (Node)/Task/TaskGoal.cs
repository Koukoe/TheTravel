using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskGoal : MonoBehaviour
{
    TaskGoalType taskGoalType;
    private bool isDone;
    string targetId;

    public bool IsDone
    {
        get { return isDone; }
        set
        {
            isDone = value;
            if (isDone)
            {
                // Perform actions when the task is completed
                Debug.Log("Task completed!");
            }
        }
    }
}

public enum TaskGoalType
{
    TRIGGER,
    NORMALITEM,
    BOOKITEM
}
