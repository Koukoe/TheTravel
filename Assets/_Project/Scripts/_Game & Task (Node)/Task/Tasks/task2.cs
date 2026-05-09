using Cysharp.Threading.Tasks;
using UnityEngine;

public class task2 : TaskBasic
{
    public TextAsset dialogueText;

    public override async UniTask TaskIEnumerator()
    {
        await WaitForDialogue();
        isDone = true;
    }

    private async UniTask WaitForDialogue()
    {
        await UniTask.Yield();

        await DialogueManager.Instance.StartWithAsyncUniTask(dialogueText);
    }
}