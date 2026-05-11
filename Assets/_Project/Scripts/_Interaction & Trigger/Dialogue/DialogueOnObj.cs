using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueOnObj : MonoBehaviour, IInteractable
{
    [SerializeField]
    [Tooltip("用于识别对话进度的唯一 ID。设置后, 索引会通过 DialogueManager 对应的 DialogueState 管理")]
    private string dialogueGuid = string.Empty;

    [SerializeField]
    [Tooltip("交互优先级")]
    private int interactionPriority = 0;

    [SerializeField] private List<TextAsset> dialogueJsonList = new List<TextAsset>();
    [SerializeField]
    [Tooltip("默认对话索引。首次进入或没有存档时使用；有 dialogueGuid 时，运行时进度会同步到 DialogueState")]
    private int dialogueIndex = 0;

    public string DialogueGuid => dialogueGuid;
    public IReadOnlyList<TextAsset> DialogueJsonList => dialogueJsonList;
    public int DialogueIndex => GetEffectiveDialogueIndex();

    public int Priority => interactionPriority;
    public Transform InteractTransform => gameObject.transform;
    public InteractTipWorld InteractTip { get; set; }
    public Vector3 TipOffset => _tipOffset;

    [SerializeField]
    [Tooltip("标签偏移")]
    protected Vector3 _tipOffset;



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
    /// <param name="index">目标索引</param>
    public void SetDialogueIndex(int index)
    {
        int normalizedIndex = NormalizeIndex(index);
        dialogueIndex = normalizedIndex;

        if (!string.IsNullOrWhiteSpace(dialogueGuid) && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.SetDialogueIndex(dialogueGuid, normalizedIndex);
        }
    }

    /// <summary>
    /// 基于当前索引做偏移变更
    /// <paramref name="delta"/> 为正数时向后推进 为负数时向前回退
    /// </summary>
    /// <param name="delta">偏移量</param>
    public void ChangeDialogueIndex(int delta)
    {
        SetDialogueIndex(dialogueIndex + delta);
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

        int currentIndex = NormalizeIndex(GetEffectiveDialogueIndex());
        if (currentIndex != dialogueIndex)
        {
            dialogueIndex = currentIndex;
        }

        return dialogueJsonList[currentIndex];
    }

    /// <summary>
    /// 获取当前有效的对话索引。优先通过 DialogueManager 获取存档中的索引；如果未指定 dialogueGuid 或存档未就绪，则返回默认索引
    /// </summary> <returns>当前有效的对话索引</returns>
    private int GetEffectiveDialogueIndex()
    {
        if (!string.IsNullOrWhiteSpace(dialogueGuid) && DialogueManager.Instance != null)
        {
            Debug.Log($"获取 {name} 的对话索引: {dialogueGuid} -> {dialogueIndex}");
            return DialogueManager.Instance.GetDialogueIndex(dialogueGuid, dialogueIndex);
        }

        Debug.Log($"获取 {name} 的对话索引: 使用默认索引 {dialogueIndex}");
        return dialogueIndex;
    }

    /// <summary>
    /// 将索引钳制在有效范围内，避免越界访问 dialogueJsonList
    /// </summary>
    /// <param name="index">对话json索引</param>
    /// <returns>钳制后的有效索引</returns>
    private int NormalizeIndex(int index)
    {
        if (dialogueJsonList == null || dialogueJsonList.Count == 0)
        {
            return 0;
        }

        return Mathf.Clamp(index, 0, dialogueJsonList.Count - 1);
    }

    /// <summary>
    /// 判断是否可以交互 条件：
    /// 1. 对话列表非空
    /// 2. 当前对话文本有效
    /// </summary>
    public bool CanInteract()
    {
        if (dialogueJsonList == null || dialogueJsonList.Count == 0)
        {
            return false;
        }

        TextAsset currentDialogue = GetCurrentDialogue();
        if (currentDialogue == null)
        {
            return false;
        }

        return true;
    }

    public void DoInteract()
    {
        TriggerDialogue();
    }
}
