using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [System.Serializable]
    // 这些本来是为了头像的但是现在不需要头像所以大概是不需要了，不过没关系因为这个不是必填
    public class CharacterProfile
    {
        // 角色ID，需与JSON中的character字段一致
        public string charID;
        // 显示在UI上的名字
        public string charName;
    }

    // 对话数据不再直接写在这里，而是通过挂载了DialogueOnObj的物体传入
    // [Header("对话数据")]
    // [SerializeField] private TextAsset dialogueJson;

    [Header("角色配置")]
    // 嗯对这个也是不必要的但是好像可以在这里填玩家的名字之类的吧，先不注释掉了
    [SerializeField] private List<CharacterProfile> characters = new List<CharacterProfile>();

    [Header("文本播放速度")]
    [SerializeField, Min(0f)] private float typewriterCharInterval = 0.05f;

    // [Header("设置")]
    // 是否在脚本启动时自动开始对话, 对话数据需要从物体传入所以这个没用了
    // [SerializeField] private bool playOnStart = false;

    // 解析后的完整数据库
    private DialogueDatabase database;
    // 当前播放到的对话索引
    private int currentIndex = -1;
    // 当前播放到的对话ID
    private string currentDialogueId = string.Empty;
    // 运行时对话ID索引
    private Dictionary<string, int> dialogueIndexById = new Dictionary<string, int>();
    // 角色ID到配置信息的映射字典
    private Dictionary<string, CharacterProfile> characterMap = new Dictionary<string, CharacterProfile>();

    // 运行时从 DialoguePanel 子物体自动获取
    private TMP_Text nameText;
    private TMP_Text contentText;

    // 运行时加载的对话数据来源
    private TextAsset dialogueJson;
    // 当前对话来源物体
    private DialogueOnObj activeDialogueSource;
    private Coroutine closePanelsRoutine;
    private Coroutine startDialogueRoutine;
    private Coroutine typingRoutine;
    private string currentFullContent = string.Empty;
    private bool isTypingContent = false;

    private const string DialoguePanelName = "DialoguePanel";
    private const string DialogueOptionsPanelName = "DialogueOptionsPanel";
    private const string DialogueContentTextNodeName = "DialogueContentText";
    private const string DialogueNameTextNodeName = "DialogueNameText";
    private const string EndToken = "END";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 初始化角色映射表
        BuildCharacterMap();
        // 加载并解析 JSON
        // DialogueLoad();

        // if (playOnStart)
        // {
        //     DialogueStart();
        // }
    }

    // 用于从外部传入新的对话数据并开始对话
    public void StartWith(TextAsset json)
    {
        StartWith(json, null);
    }

    public void StartWith(TextAsset json, DialogueOnObj source)
    {
        if (json == null) return;
        dialogueJson = json;
        activeDialogueSource = source;
        DialogueLoad();
        DialogueStart();
    }

    // 构建角色字典，方便通过角色ID快速查找配置
    private void BuildCharacterMap()
    {
        characterMap.Clear();

        if (characters == null)
        {
            return;
        }

        foreach (var profile in characters)
        {
            if (profile == null || string.IsNullOrEmpty(profile.charID))
            {
                continue;
            }

            if (!characterMap.ContainsKey(profile.charID))
            {
                characterMap.Add(profile.charID, profile);
            }
            else
            {
                Debug.LogWarning($"角色ID重复: {profile.charID}");
            }
        }
    }

    // 读取并解析JSON对话数据
    public void DialogueLoad()
    {
        if (dialogueJson == null)
        {
            Debug.LogError("未指定对话JSON文件");
            return;
        }

        database = JsonUtility.FromJson<DialogueDatabase>(dialogueJson.text);

        if (database == null || database.dialogues == null)
        {
            Debug.LogError("JSON解析失败, 请检查格式或字段名是否匹配");
            return;
        }

        NormalizeEndTokens();
        BuildDialogueIndex();
    }

    // 将"End", "end", "END", 甚至是" eNd"之类的统一为 EndToken
    private void NormalizeEndTokens()
    {
        if (database == null || database.dialogues == null)
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

        if (database == null || database.dialogues == null)
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

    // 开始对话并显示第一句
    public void DialogueStart()
    {
        if (database == null || database.dialogues == null || database.dialogues.Count == 0)
        {
            Debug.LogWarning("对话数据为空，无法开始");
            return;
        }

        if (startDialogueRoutine != null)
        {
            StopCoroutine(startDialogueRoutine);
            startDialogueRoutine = null;
        }

        startDialogueRoutine = StartCoroutine(StartDialogueWithCleanup());
    }

    private IEnumerator StartDialogueWithCleanup()
    {
        while (UIManager.Instance != null && UIManager.Instance.IsTransitioning)
        {
            yield return null;
        }

        yield return CleanupResidualDialoguePanels();

        BasePanel panel = UIManager.Instance.Peek();
        if (!(panel is DialoguePanel))
        {
            panel = UIManager.Instance.Push(DialoguePanelName);
        }

        if (panel == null)
        {
            Debug.LogError("打开 DialoguePanel 失败，请检查 UIManager/PoolManager 配置");
            startDialogueRoutine = null;
            yield break;
        }

        ResolveTextRefs(panel.transform);
        if (nameText == null || contentText == null)
        {
            Debug.LogError("未能在 DialoguePanel 下找到 DialogueNameText/DialogueContentText 文本组件");
            startDialogueRoutine = null;
            yield break;
        }

        currentIndex = 0;
        currentDialogueId = GetDialogueIdByIndex(currentIndex);
        ShowCurrent();
        startDialogueRoutine = null;
    }

    private IEnumerator CleanupResidualDialoguePanels()
    {
        if (UIManager.Instance == null)
        {
            yield break;
        }

        while (UIManager.Instance.Peek() is DialogueOptionsPanel || UIManager.Instance.Peek() is DialoguePanel)
        {
            UIManager.Instance.Pop();

            while (UIManager.Instance.IsTransitioning)
            {
                yield return null;
            }
        }
    }

    // 播放下一句对话
    public void DialogueNext()
    {
        if (database == null || database.dialogues == null)
        {
            return;
        }

        if (isTypingContent)
        {
            CompleteCurrentContentInstantly();
            return;
        }

        DialogueEntry currentEntry = GetCurrentEntry();
        if (currentEntry == null)
        {
            return;
        }

        // 存在选项时，等待外部UI调用 SelectOption
        if (currentEntry.options != null && currentEntry.options.Count > 0)
        {
            return;
        }

        AdvanceByNextId(currentEntry.nextId);
    }

    // 由选项按钮调用，按选项索引进入分支
    public void SelectOption(int optionIndex)
    {
        DialogueEntry currentEntry = GetCurrentEntry();
        if (currentEntry == null)
        {
            return;
        }

        if (currentEntry.options == null || currentEntry.options.Count == 0)
        {
            Debug.LogWarning("当前对话没有可选项, 请先调用 DialogueNext 或检查数据");
            return;
        }

        if (optionIndex < 0 || optionIndex >= currentEntry.options.Count)
        {
            Debug.LogWarning($"无效选项索引: {optionIndex}");
            return;
        }

        DialogueOption option = currentEntry.options[optionIndex];
        if (option == null)
        {
            Debug.LogWarning("选项数据为空, 将尝试按顺序继续");
            AdvanceSequentially();
            return;
        }

        ApplyOptionEffects(option.effects);
        AdvanceByNextId(option.nextId);
    }

    private void ApplyOptionEffects(List<DialogueEffect> effects)
    {
        if (effects == null || effects.Count == 0)
        {
            return;
        }

        for (int i = 0; i < effects.Count; i++)
        {
            DialogueEffect effect = effects[i];
            if (effect == null)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(effect.SetDialogueIndex))
            {
                if (activeDialogueSource == null)
                {
                    Debug.LogWarning("当前对话没有来源物体, 无法设置对话索引");
                    continue;
                }

                if (!int.TryParse(effect.SetDialogueIndex.Trim(), out int nextDialogueIndex))
                {
                    Debug.LogWarning($"SetDialogueIndex 参数不是有效整数: {effect.SetDialogueIndex}");
                    continue;
                }

                activeDialogueSource.SetDialogueIndex(nextDialogueIndex);
                continue;
            }
        }
    }

    public List<DialogueOption> GetCurrentOptions()
    {
        DialogueEntry entry = GetCurrentEntry();
        if (entry == null || entry.options == null || entry.options.Count == 0)
        {
            return new List<DialogueOption>();
        }

        return entry.options;
    }

    public bool HasCurrentOptions()
    {
        DialogueEntry entry = GetCurrentEntry();
        return entry != null && entry.options != null && entry.options.Count > 0;
    }

    public void SetTypewriterCharInterval(float interval)
    {
        typewriterCharInterval = Mathf.Max(0f, interval);
    }

    public float GetTypewriterCharInterval()
    {
        return typewriterCharInterval;
    }

    private void AdvanceByNextId(string nextId)
    {
        if (IsEndToken(nextId))
        {
            DialogueEnd();
            return;
        }

        if (string.IsNullOrWhiteSpace(nextId))
        {
            AdvanceSequentially();
            return;
        }

        if (TryJumpTo(nextId.Trim()))
        {
            return;
        }

        Debug.LogWarning($"未找到 nextId 对应的对话: {nextId}，对话将结束");
        DialogueEnd();
    }

    private void AdvanceSequentially()
    {
        int nextIndex = currentIndex + 1;
        if (database == null || database.dialogues == null || nextIndex >= database.dialogues.Count)
        {
            DialogueEnd();
            return;
        }

        currentIndex = nextIndex;
        currentDialogueId = GetDialogueIdByIndex(currentIndex);
        ShowCurrent();
    }

    private bool TryJumpTo(string targetId)
    {
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
        ShowCurrent();
        return true;
    }

    private DialogueEntry GetCurrentEntry()
    {
        if (database == null || database.dialogues == null)
        {
            return null;
        }

        if (currentIndex < 0 || currentIndex >= database.dialogues.Count)
        {
            return null;
        }

        return database.dialogues[currentIndex];
    }

    private string GetDialogueIdByIndex(int index)
    {
        if (database == null || database.dialogues == null || index < 0 || index >= database.dialogues.Count)
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

    // 结束对话并清空显示
    public void DialogueEnd()
    {
        currentIndex = -1;
        currentDialogueId = string.Empty;
        activeDialogueSource = null;
        currentFullContent = string.Empty;
        isTypingContent = false;

        if (contentText != null) contentText.text = string.Empty;
        if (nameText != null) nameText.text = string.Empty;

        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        if (closePanelsRoutine != null)
        {
            StopCoroutine(closePanelsRoutine);
            closePanelsRoutine = null;
        }

        closePanelsRoutine = StartCoroutine(CloseDialoguePanelsFromStack());
    }

    private IEnumerator CloseDialoguePanelsFromStack()
    {
        if (UIManager.Instance == null)
        {
            yield break;
        }

        if (UIManager.Instance.Peek() is DialogueOptionsPanel)
        {
            UIManager.Instance.Pop();
        }

        while (UIManager.Instance.IsTransitioning)
        {
            yield return null;
        }

        if (UIManager.Instance.Peek() is DialoguePanel)
        {
            UIManager.Instance.Pop();
        }

        closePanelsRoutine = null;
    }

    // 显示当前索引的内容
    private void ShowCurrent()
    {
        if (database == null || currentIndex < 0 || currentIndex >= database.dialogues.Count)
        {
            return;
        }

        DialogueEntry entry = database.dialogues[currentIndex];
        currentDialogueId = GetDialogueIdByIndex(currentIndex);
        currentFullContent = entry != null ? entry.content : string.Empty;

        if (UIManager.Instance != null && UIManager.Instance.Peek() is DialogueOptionsPanel)
        {
            UIManager.Instance.Pop();
        }

        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        isTypingContent = true;

        if (contentText != null)
        {
            contentText.text = string.Empty;
        }

        // 更新角色信息
        UpdateUIWithCharacter(entry.character);

        typingRoutine = StartCoroutine(TypeCurrentContent(entry));
    }

    private IEnumerator TypeCurrentContent(DialogueEntry entry)
    {
        if (contentText == null)
        {
            isTypingContent = false;
            typingRoutine = null;
            ShowOptionsAfterTyping(entry);
            yield break;
        }

        if (string.IsNullOrEmpty(currentFullContent))
        {
            contentText.text = string.Empty;
            isTypingContent = false;
            typingRoutine = null;
            ShowOptionsAfterTyping(entry);
            yield break;
        }

        for (int i = 0; i < currentFullContent.Length; i++)
        {
            contentText.text += currentFullContent[i];

            if (typewriterCharInterval > 0f)
            {
                yield return new WaitForSeconds(typewriterCharInterval);
            }
            else
            {
                yield return null;
            }
        }

        isTypingContent = false;
        typingRoutine = null;
        ShowOptionsAfterTyping(entry);
    }

    private void CompleteCurrentContentInstantly()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        if (contentText != null)
        {
            contentText.text = currentFullContent;
        }

        isTypingContent = false;

        DialogueEntry entry = GetCurrentEntry();
        if (entry != null)
        {
            ShowOptionsAfterTyping(entry);
        }
    }

    private void ShowOptionsAfterTyping(DialogueEntry entry)
    {
        if (entry == null)
        {
            return;
        }

        RefreshOptionsPanel(entry);
    }

    private void RefreshOptionsPanel(DialogueEntry entry)
    {
        bool hasOptions = entry != null && entry.options != null && entry.options.Count > 0;
        if (!hasOptions)
        {
            if (UIManager.Instance.Peek() is DialogueOptionsPanel)
            {
                UIManager.Instance.Pop();
            }
            return;
        }

        BasePanel panel = UIManager.Instance.Peek();
        if (!(panel is DialogueOptionsPanel))
        {
            panel = UIManager.Instance.Push(DialogueOptionsPanelName);
        }

        if (panel is DialogueOptionsPanel optionsPanel)
        {
            optionsPanel.RefreshOptions();
        }
    }

    // 根据角色ID更新UI上的名字
    private void UpdateUIWithCharacter(string charID)
    {
        if (string.IsNullOrEmpty(charID))
        {
            ClearCharacterUI();
            return;
        }

        if (characterMap.TryGetValue(charID, out CharacterProfile profile))
        {
            if (nameText != null) nameText.text = profile.charName;
        }
        else
        {
            // 如果没找到配置，则直接显示 ID
            if (nameText != null) nameText.text = charID;
            Debug.LogWarning($"未找到角色 ID 为 {charID} 的配置");
        }
    }

    // 清除角色 UI 显示
    private void ClearCharacterUI()
    {
        if (nameText != null) nameText.text = string.Empty;
    }

    private void ResolveTextRefs(Transform root)
    {
        if (root == null) return;

        nameText = FindTextByNodeName(root, DialogueNameTextNodeName);
        contentText = FindTextByNodeName(root, DialogueContentTextNodeName);
    }

    private TMP_Text FindTextByNodeName(Transform root, string nodeName)
    {
        var texts = root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].gameObject.name == nodeName)
            {
                return texts[i];
            }
        }
        return null;
    }

    private void OnDestroy()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        if (Instance == this) Instance = null;
    }
}