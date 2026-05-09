using Cysharp.Threading.Tasks;
using UnityEngine;

public class task2 : TaskBasic
{
    public TextAsset dialogueText;

    protected override async UniTask OnTaskStart()
    {
        // 直接调用基类的通用方法
        await WaitForDialogue();

        FinishTask();
    }

    private async UniTask WaitForDialogue()
    {
        await UniTask.Yield();
        await DialogueManager.Instance.StartWithAsyncUniTask(dialogueText);
    }
}

