using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueFlowState
{
    private const string EndToken = "END";

    private DialogueDatabase database;
    private int currentIndex = -1;
    private string currentDialogueId = string.Empty;
    private readonly Dictionary<string, int> dialogueIndexById = new Dictionary<string, int>();

    public bool HasLoadedData => database != null && database.dialogues != null;

    public bool CanStart => HasLoadedData && database.dialogues.Count > 0;

    public string CurrentDialogueId => currentDialogueId;

    public bool Load(TextAsset dialogueJson)
    {
        if (dialogueJson == null)
        {
            Debug.LogError("未指定对话JSON文件");
            return false;
        }

        database = JsonUtility.FromJson<DialogueDatabase>(dialogueJson.text);

        if (database == null || database.dialogues == null)
        {
            Debug.LogError("JSON解析失败, 请检查格式或字段名是否匹配");
            return false;
        }

        NormalizeEndTokens();
        BuildDialogueIndex();
        ClearRuntime();
        return true;
    }

    public bool StartFromFirst(out DialogueEntry entry)
    {
        entry = null;
        if (!CanStart)
        {
            return false;
        }

        currentIndex = 0;
        currentDialogueId = GetDialogueIdByIndex(currentIndex);
        entry = GetCurrentEntry();
        return entry != null;
    }

    public DialogueEntry GetCurrentEntry()
    {
        if (!HasLoadedData)
        {
            return null;
        }

        if (currentIndex < 0 || currentIndex >= database.dialogues.Count)
        {
            return null;
        }

        return database.dialogues[currentIndex];
    }

    // 按 nextId 推进；如果是 END 就直接结束，如果为空就走顺序推进
    public bool TryMoveNextByToken(string nextId, out DialogueEntry nextEntry, out bool ended)
    {
        nextEntry = null;
        ended = false;

        if (IsEndToken(nextId))
        {
            ClearRuntime();
            ended = true;
            return true;
        }

        if (string.IsNullOrWhiteSpace(nextId))
        {
            return TryMoveSequential(out nextEntry, out ended);
        }

        if (TryJumpTo(nextId.Trim(), out nextEntry))
        {
            return true;
        }

        Debug.LogWarning($"未找到 nextId 对应的对话: {nextId}，对话将结束");
        ClearRuntime();
        ended = true;
        return true;
    }

    public bool TryMoveSequential(out DialogueEntry nextEntry, out bool ended)
    {
        nextEntry = null;
        ended = false;

        int nextIndex = currentIndex + 1;
        if (!HasLoadedData || nextIndex >= database.dialogues.Count)
        {
            ClearRuntime();
            ended = true;
            return true;
        }

        currentIndex = nextIndex;
        currentDialogueId = GetDialogueIdByIndex(currentIndex);
        nextEntry = GetCurrentEntry();
        return nextEntry != null;
    }

    public void ClearRuntime()
    {
        currentIndex = -1;
        currentDialogueId = string.Empty;
    }

    private bool TryJumpTo(string targetId, out DialogueEntry entry)
    {
        entry = null;

        if (string.IsNullOrWhiteSpace(targetId))
        {
            return false;
        }

        if (!dialogueIndexById.TryGetValue(targetId, out int targetIndex))
        {
            return false;
        }

        currentIndex = targetIndex;
        currentDialogueId = targetId;
        entry = GetCurrentEntry();
        return entry != null;
    }

    private void NormalizeEndTokens()
    {
        if (!HasLoadedData)
        {
            return;
        }

        for (int i = 0; i < database.dialogues.Count; i++)
        {
            DialogueEntry entry = database.dialogues[i];
            if (entry == null)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(entry.id))
            {
                entry.id = entry.id.Trim();
            }

            if (!string.IsNullOrWhiteSpace(entry.nextId))
            {
                entry.nextId = entry.nextId.Trim();
            }

            if (IsEndToken(entry.nextId))
            {
                entry.nextId = EndToken;
            }

            if (entry.options == null)
            {
                continue;
            }

            for (int j = 0; j < entry.options.Count; j++)
            {
                DialogueOption option = entry.options[j];
                if (option == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(option.nextId))
                {
                    option.nextId = option.nextId.Trim();
                }

                if (IsEndToken(option.nextId))
                {
                    option.nextId = EndToken;
                }
            }
        }
    }

    private bool IsEndToken(string value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               string.Equals(value.Trim(), EndToken, StringComparison.OrdinalIgnoreCase);
    }

    private void BuildDialogueIndex()
    {
        dialogueIndexById.Clear();

        if (!HasLoadedData)
        {
            return;
        }

        for (int i = 0; i < database.dialogues.Count; i++)
        {
            DialogueEntry entry = database.dialogues[i];
            if (entry == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.id))
            {
                Debug.LogWarning($"第 {i} 条对话缺少 id, 无法被 nextId 精确跳转");
                continue;
            }

            if (!dialogueIndexById.ContainsKey(entry.id))
            {
                dialogueIndexById.Add(entry.id, i);
            }
            else
            {
                Debug.LogWarning($"对话ID重复: {entry.id}，将使用首次出现的条目");
            }
        }
    }

    private string GetDialogueIdByIndex(int index)
    {
        if (!HasLoadedData || index < 0 || index >= database.dialogues.Count)
        {
            return string.Empty;
        }

        DialogueEntry entry = database.dialogues[index];
        if (entry == null || string.IsNullOrWhiteSpace(entry.id))
        {
            return string.Empty;
        }

        return entry.id;
    }
}
