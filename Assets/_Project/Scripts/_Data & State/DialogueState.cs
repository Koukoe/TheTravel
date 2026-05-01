using System;

[Serializable]
public class DialogueState : BaseState
{
    public int dialogueIndex;

    public override void Init(string id)
    {
        if (!string.IsNullOrEmpty(guid)) return;
        base.Init(id);
        dialogueIndex = 0;
    }
}
