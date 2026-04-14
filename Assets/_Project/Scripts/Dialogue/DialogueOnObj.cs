using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueOnObj : MonoBehaviour
{
    [SerializeField] private TextAsset dialogueJson;

    public TextAsset DialogueJson => dialogueJson;

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartWith(dialogueJson);
    }
}
