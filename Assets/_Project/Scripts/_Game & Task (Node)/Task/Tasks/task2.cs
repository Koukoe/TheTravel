using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class task2 : TaskBasic
{
    public TextAsset dialogueText;

    public override IEnumerator TaskIEnumerator()
    {
        yield return StartCoroutine(WaitForDialogue());
        isDone = true;
    }

    private IEnumerator WaitForDialogue()
    {
        yield return null;
        yield return DialogueManager.Instance.StartWithAsync(dialogueText);
    }
}
