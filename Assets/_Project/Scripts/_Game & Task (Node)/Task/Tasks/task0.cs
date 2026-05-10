using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class task0 : TaskBasic
{
    protected override async UniTask OnTaskStart()
    {
        Debug.Log("Task Initial");
        FinishTask();
    }

    protected override void OnTaskEnd()
    {
        Debug.Log("Start");
    }
}