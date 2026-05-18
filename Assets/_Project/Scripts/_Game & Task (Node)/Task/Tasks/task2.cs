using System.Threading.Tasks;
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

    protected override void OnTaskEnd()
    {
        GameFlowManager.Instance.PlayingData.startFinish = true;
        GameFlowManager.Instance.OnCheckPoint().Forget();
        DataGlobalSystem.GetShallow().hasEnteredGame = true;
        DataGlobalSystem.Save();
    }

    protected virtual async UniTask WaitForDialogue()
    {
        await UniTask.WaitUntil(() => !UIManager.Instance.UISys() && !(UIManager.Instance.Peek() is BookPanel));
        await DialogueManager.Instance.StartWithAsyncUniTask(dialogueText);
    }
}

