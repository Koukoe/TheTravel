using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // 运行时对话流程状态（索引、跳转、END判定、数据读取）
    private DialogueFlowState flowState;
    // 角色ID到配置信息的映射字典
    private Dictionary<string, CharacterProfile> characterMap = new Dictionary<string, CharacterProfile>();

    // 运行时加载的对话数据来源
    private TextAsset dialogueJson;
    // 当前对话来源物体
    private DialogueOnObj activeDialogueSource;
    private Coroutine retryPendingOptionsRoutine;
    private DialogueTypewriter typewriter;
    private DialogueUIController uiController;
    private DialoguePresenter presenter;

    private const string DialogueContentTextNodeName = "DialogueContentText";
    private const string DialogueNameTextNodeName = "DialogueNameText";
    private void Awake()
    {
        Instance = this;
        flowState = new DialogueFlowState();
        typewriter = new DialogueTypewriter(this);
        uiController = new DialogueUIController(this);
        presenter = new DialoguePresenter(typewriter, uiController, ResolveCharacterName);
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
        flowState?.Load(dialogueJson);
    }

    // 开始对话并显示第一句
    public void DialogueStart()
    {
        if (flowState == null || !flowState.CanStart)
        {
            Debug.LogWarning("对话数据为空，无法开始");
            return;
        }

        uiController?.OpenDialoguePanelWithCleanup(OnDialoguePanelReady);
    }

    public void SuspendDialogueFlow()
    {
        typewriter?.Pause();
    }

    public void ResumeDialogueFlow()
    {
        if (typewriter != null && typewriter.IsPaused)
        {
            typewriter.Resume();
        }

        SchedulePendingOptionsRetry();
    }

    private void SchedulePendingOptionsRetry()
    {
        if (retryPendingOptionsRoutine != null)
        {
            StopCoroutine(retryPendingOptionsRoutine);
            retryPendingOptionsRoutine = null;
        }

        retryPendingOptionsRoutine = StartCoroutine(RetryPendingOptionsWhenUIReady());
    }

    private IEnumerator RetryPendingOptionsWhenUIReady()
    {
        while (UIManager.Instance != null && UIManager.Instance.IsTransitioning)
        {
            yield return null;
        }

        presenter?.TryPresentPendingOptions();
        retryPendingOptionsRoutine = null;
    }

    private void OnDialoguePanelReady(BasePanel panel)
    {
        if (panel == null)
        {
            Debug.LogError("打开 DialoguePanel 失败，请检查 UIManager/PoolManager 配置");
            return;
        }

        if (presenter == null || !presenter.Bind(panel.transform, DialogueNameTextNodeName, DialogueContentTextNodeName))
        {
            Debug.LogError("未能在 DialoguePanel 下找到 DialogueNameText/DialogueContentText 文本组件");
            return;
        }

        if (!flowState.StartFromFirst(out DialogueEntry firstEntry) || firstEntry == null)
        {
            Debug.LogWarning("对话首句无效，无法开始");
            return;
        }

        ShowCurrent(firstEntry);
    }

    // 播放下一句对话
    public void DialogueNext()
    {
        if (flowState == null || !flowState.HasLoadedData)
        {
            return;
        }

        if (typewriter != null && typewriter.IsTyping)
        {
            typewriter.CompleteNow();
            return;
        }

        DialogueEntry currentEntry = flowState != null ? flowState.GetCurrentEntry() : null;
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
        DialogueEntry currentEntry = flowState != null ? flowState.GetCurrentEntry() : null;
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

        DialogueEffectExecutor.ApplyEffects(option.effects, activeDialogueSource);
        AdvanceByNextId(option.nextId);
    }

    public List<DialogueOption> GetCurrentOptions()
    {
        DialogueEntry entry = flowState != null ? flowState.GetCurrentEntry() : null;
        if (entry == null || entry.options == null || entry.options.Count == 0)
        {
            return new List<DialogueOption>();
        }

        return entry.options;
    }

    public bool HasCurrentOptions()
    {
        DialogueEntry entry = flowState != null ? flowState.GetCurrentEntry() : null;
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
        if (flowState == null)
        {
            return;
        }

        if (!flowState.TryMoveNextByToken(nextId, out DialogueEntry nextEntry, out bool ended))
        {
            return;
        }

        if (ended)
        {
            DialogueEnd();
            return;
        }

        ShowCurrent(nextEntry);
    }

    private void AdvanceSequentially()
    {
        if (flowState == null)
        {
            return;
        }

        if (!flowState.TryMoveSequential(out DialogueEntry nextEntry, out bool ended))
        {
            return;
        }

        if (ended)
        {
            DialogueEnd();
            return;
        }

        ShowCurrent(nextEntry);
    }

    // 结束对话并清空显示
    public void DialogueEnd()
    {
        flowState?.ClearRuntime();
        activeDialogueSource = null;
        presenter?.Clear();

        typewriter?.Stop();
        uiController?.CloseDialoguePanels();

        if (retryPendingOptionsRoutine != null)
        {
            StopCoroutine(retryPendingOptionsRoutine);
            retryPendingOptionsRoutine = null;
        }
    }

    // 显示当前索引的内容
    private void ShowCurrent(DialogueEntry entry)
    {
        if (entry == null)
        {
            return;
        }

        presenter?.ShowEntry(entry, typewriterCharInterval);
    }

    private string ResolveCharacterName(string charID)
    {
        if (string.IsNullOrEmpty(charID))
        {
            return string.Empty;
        }

        if (characterMap.TryGetValue(charID, out CharacterProfile profile) && profile != null)
        {
            return profile.charName;
        }

        return null;
    }

    private void OnDestroy()
    {
        typewriter?.Stop();
        uiController?.StopAll();

        if (retryPendingOptionsRoutine != null)
        {
            StopCoroutine(retryPendingOptionsRoutine);
            retryPendingOptionsRoutine = null;
        }

        if (Instance == this) Instance = null;
    }
}
