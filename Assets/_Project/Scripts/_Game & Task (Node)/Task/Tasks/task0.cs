using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class task0 : TaskBasic
{
    public static bool init = false;

    public override async UniTask TaskIEnumerator()
    {
        Debug.Log("Task 0");

        await UniTask.WaitUntil(() => init);

        isDone = true;
        init = false;
    }
}