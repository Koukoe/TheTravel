using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueOnObj : MonoBehaviour
{
    [SerializeField] private List<TextAsset> dialogueJsonList = new List<TextAsset>();
    [SerializeField] private int dialogueIndex = 0;

    public IReadOnlyList<TextAsset> DialogueJsonList => dialogueJsonList;
    public int DialogueIndex => dialogueIndex;

    /// <summary>
    /// 触发当前索引对应的对话
    /// </summary>
    public void TriggerDialogue()
    {
        TextAsset json = GetCurrentDialogue();
        if (json == null)
        {
            Debug.LogWarning($"{name} 未配置可用的对话文本");
            return;
        }

        DialogueManager.Instance.StartWith(json, this);
    }

    /// <summary>
    /// 设置当前物体的对话索引
    /// <paramref name="index"/> 超出范围时会自动钳制
    /// </summary>
    /// <param name="index">目标索引 超出范围时会自动钳制</param>
    public void SetDialogueIndex(int index)
    {
        if (dialogueJsonList == null || dialogueJsonList.Count == 0)
        {
            dialogueIndex = 0;
            return;
        }

        dialogueIndex = Mathf.Clamp(index, 0, dialogueJsonList.Count - 1);
    }

    /// <summary>
    /// 基于当前索引做偏移变更
    /// <paramref name="delta"/> 为正数时向后推进 为负数时向前回退
    /// </summary>
    /// <param name="delta">正数向后推进 负数向前回退</param>
    public void ChangeDialogueIndex(int delta)
    {
        SetDialogueIndex(dialogueIndex + delta);
    }

    /// <summary>
    /// 获取用于存档的对话索引
    /// </summary>
    /// <returns>当前生效的对话索引</returns>
    public int GetDialogueIndexForSave()
    {
        return dialogueIndex;
    }

    /// <summary>
    /// 从存档恢复对话索引
    /// <paramref name="index"/> 表示存档中的索引值
    /// </summary>
    /// <param name="index">存档中的对话索引值</param>
    public void LoadDialogueIndexFromSave(int index)
    {
        SetDialogueIndex(index);
    }

    /// <summary>
    /// 获取当前索引对应的对话文本
    /// </summary>
    /// <returns>当前索引对应的文本资源 若未配置则返回 null</returns>
    public TextAsset GetCurrentDialogue()
    {
        if (dialogueJsonList == null || dialogueJsonList.Count == 0)
        {
            return null;
        }

        SetDialogueIndex(dialogueIndex);
        return dialogueJsonList[dialogueIndex];
    }
}
