using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskGoal : MonoBehaviour
{
    private bool isDone;

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
