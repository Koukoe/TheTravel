using Cysharp.Threading.Tasks;
using UnityEngine;

public class task2 : TaskBasic
{
    public TextAsset dialogueText;

    protected override async UniTask OnTaskStart()
    {
        await UniTask.Delay(1000);
        await WaitForDialogue();

        FinishTask();
    }

    private async UniTask WaitForDialogue()
    {
        await UniTask.WaitUntil(() => !UIManager.Instance.UISys());
        await DialogueManager.Instance.StartWithAsyncUniTask(dialogueText);
    }
}

