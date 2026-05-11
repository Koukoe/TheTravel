using System;
using System.Collections.Generic;

[Serializable]
public class DialogueState : BaseState
{
    public int dialogueIndex;
    public List<int> completedDialogueIndices = new List<int>();

    public override void Init(string id)
    {
        if (!string.IsNullOrEmpty(guid)) return;
        base.Init(id);
        dialogueIndex = 0;
        completedDialogueIndices.Clear();
    }

    public override BaseState Clone()
    {
        throw new NotImplementedException();
    }

    public override void Copyfrom(BaseState targetState)
    {
        throw new NotImplementedException();
    }
}
