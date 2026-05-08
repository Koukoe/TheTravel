using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class task0 : TaskBasic
{
    public override IEnumerator TaskIEnumerator()
    {
        Debug.Log("Task 0");
        yield return null;
        isDone = true;
    }
}
