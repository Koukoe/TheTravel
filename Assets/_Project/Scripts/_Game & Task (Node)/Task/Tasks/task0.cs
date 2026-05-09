using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class task0 : TaskBasic
{
    protected override async UniTask OnTaskStart()
    {
        await UniTask.Yield();
        Debug.Log("Task 0 Start");
        FinishTask();
    }

    protected override void OnTaskEnd()
    {
        Debug.Log("Task 0 End");
    }
}