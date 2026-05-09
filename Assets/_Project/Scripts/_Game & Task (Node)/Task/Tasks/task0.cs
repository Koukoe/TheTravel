using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class task0 : TaskBasic
{
    public static bool init = false;

    public override IEnumerator TaskIEnumerator()
    {
        Debug.Log("Task 0");
        yield return new WaitUntil(() => init);
        isDone = true;
        init = false;
    }
}
