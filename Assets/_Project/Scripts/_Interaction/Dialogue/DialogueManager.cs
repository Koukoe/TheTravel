using System.Collections;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    // 对话数据不再直接写在这里，而是通过挂载了DialogueOnObj的物体传入
    // [Header("对话数据")]
    // [SerializeField] private TextAsset dialogueJson;

    [Header("角色配置")]
    [SerializeField] private TextAsset characterMapJson;

    [Header("文本播放速度")]
    [SerializeField, Min(0f)] private float typewriterCharInterval = 0.05f;

    // [Header("设置")]
    // 是否在脚本启动时自动开始对话, 对话数据需要从物体传入所以这个没用了
    // [SerializeField] private bool playOnStart = false;

    // 运行时对话流程状态（索引、跳转、END判定、数据读取）
    private DialogueFlowState flowState;
    // 角色ID到显示名的映射字典
    private Dictionary<string, string> characterMap = new Dictionary<string, string>();

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

    /// <summary>
    /// 使用指定对话文本并开始对话流程
    /// </summary>
    /// <param name="json">对话 JSON 资源</param>
    public void StartWith(TextAsset json)
    {
        StartWith(json, null);
    }

    /// <summary>
    /// 使用指定对话文本和来源物体开始对话流程
    /// </summary>
    /// <param name="json">对话 JSON 资源</param>
    /// <param name="source">对话来源物体，用于读取和写回对话索引</param>
    public void StartWith(TextAsset json, DialogueOnObj source)
    {
        if (json == null) return;
        dialogueJson = json;
        activeDialogueSource = source;
        DialogueLoad();
        DialogueStart();
    }

    /// <summary>
    /// 获取指定对话 GUID 当前保存的索引
    /// </summary>
    /// <param name="dialogueGuid">对话的唯一标识</param>
    /// <param name="fallbackIndex">未找到存档时返回的默认索引</param>
    /// <returns>当前保存的对话索引；若未命中则返回 <paramref name="fallbackIndex"/></returns>
    public int GetDialogueIndex(string dialogueGuid, int fallbackIndex = 0)
    {
        if (string.IsNullOrWhiteSpace(dialogueGuid))
        {
            Debug.LogWarning("GetDialogueIndex 未指定 dialogueGuid, 将返回默认索引");
            return fallbackIndex;
        }

        if (GameFlowManager.Instance == null || GameFlowManager.Instance.PlayingData == null)
        {
            Debug.LogWarning($"GetDialogueIndex 存档未就绪: {dialogueGuid}, 将返回默认索引");
            return fallbackIndex;
        }

        DialogueState state = GameFlowManager.Instance.PlayingData.GetState<DialogueState>(dialogueGuid);
        return state != null ? state.dialogueIndex : fallbackIndex;
    }

    /// <summary>
    /// 设置指定对话 GUID 的索引
    /// </summary>
    /// <param name="dialogueGuid">对话的唯一标识</param>
    /// <param name="index">要写入的索引值，会被钳制为非负数</param>
    public void SetDialogueIndex(string dialogueGuid, int index)
    {
        if (string.IsNullOrWhiteSpace(dialogueGuid))
        {
            return;
        }

        if (GameFlowManager.Instance == null || GameFlowManager.Instance.PlayingData == null)
        {
            Debug.LogWarning($"无法设置对话索引, 存档未就绪: {dialogueGuid}");
            return;
        }

        DialogueState state = GameFlowManager.Instance.PlayingData.GetState<DialogueState>(dialogueGuid);
        if (state == null)
        {
            return;
        }

        state.dialogueIndex = Mathf.Max(0, index);
    }

    // 构建角色字典，方便通过角色ID快速查找配置
    private void BuildCharacterMap()
    {
        characterMap.Clear();

        if (characterMapJson == null)
        {
            Debug.LogWarning("未配置 character_map.json");
            return;
        }

        Dictionary<string, string> map = null;
        try
        {
            map = JsonConvert.DeserializeObject<Dictionary<string, string>>(characterMapJson.text);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"读取 character_map.json 失败: {ex.Message}");
            return;
        }

        if (map == null || map.Count == 0)
        {
            Debug.LogWarning("character_map.json 内容为空");
            return;
        }

        foreach (var pair in map)
        {
            if (string.IsNullOrWhiteSpace(pair.Key))
            {
                continue;
            }

            if (!characterMap.ContainsKey(pair.Key))
            {
                characterMap.Add(pair.Key, pair.Value ?? string.Empty);
            }
            else
            {
                Debug.LogWarning($"角色ID重复: {pair.Key}");
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

    /// <summary>
    /// 安排一次延迟重试，用于在 UI 转场完成后继续显示选项
    /// </summary>
    private void SchedulePendingOptionsRetry()
    {
        if (retryPendingOptionsRoutine != null)
        {
            StopCoroutine(retryPendingOptionsRoutine);
            retryPendingOptionsRoutine = null;
        }

        retryPendingOptionsRoutine = StartCoroutine(RetryPendingOptionsWhenUIReady());
    }

    /// <summary>
    /// 等待 UI 转场结束后，再尝试把当前待显示的选项补出来
    /// </summary>
    private IEnumerator RetryPendingOptionsWhenUIReady()
    {
        while (UIManager.Instance != null && UIManager.Instance.IsTransitioning)
        {
            yield return null;
        }

        presenter?.TryPresentPendingOptions();
        retryPendingOptionsRoutine = null;
    }

    /// <summary>
    /// 对话面板打开后回调，用于绑定 UI 并显示首句
    /// </summary>
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

    /// <summary>
    /// 推进到下一句对话；若当前正在打字则先补完当前句
    /// </summary>
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

    /// <summary>
    /// 由选项按钮调用，按选项索引进入分支
    /// </summary>
    /// <param name="optionIndex">选项索引，从 0 开始</param>
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

        bool blocked = DialogueEffectExecutor.TryApplyBlockingEffects(
            option.effects,
            activeDialogueSource,
            PlayActionAndResume,
            () => AdvanceByNextId(option.nextId));

        if (blocked)
        {
            return;
        }

        DialogueEffectExecutor.ApplyEffects(option.effects, activeDialogueSource);
        AdvanceByNextId(option.nextId);
    }

    /// <summary>
    /// 占位的动作驱动回调，后续接入角色动作系统时替换实现
    /// </summary>
    /// <param name="actionId">动作标识</param>
    /// <param name="onCompleted">动作结束后的继续回调</param>
    private void PlayActionAndResume(string actionId, Action onCompleted)
    {
        Debug.LogWarning($"PlayActionAndResume 还未接入角色动作系统, actionId={actionId}，当前先直接继续对话");
        onCompleted?.Invoke();
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

    /// <summary>
    /// 设置打字机每个字符的显示间隔
    /// </summary>
    /// <param name="interval">字符间隔，最小为 0</param>
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
        ShowCurrent(entry, true);
    }

    private void ShowCurrent(DialogueEntry entry, bool executeEntryEffects)
    {
        if (entry == null)
        {
            return;
        }

        if (executeEntryEffects)
        {
            // 先应用所有 immediate effects
            DialogueEffectExecutor.ApplyEffects(entry.effects, activeDialogueSource);

            // 再处理 blocking effects
            bool blocked = DialogueEffectExecutor.TryApplyBlockingEffects(
                entry.effects,
                activeDialogueSource,
                PlayActionAndResume,
                () => ShowCurrent(entry, false));

            if (blocked)
            {
                return;
            }
        }

        presenter?.ShowEntry(entry, typewriterCharInterval);
    }

    private string ResolveCharacterName(string charID)
    {
        if (string.IsNullOrEmpty(charID))
        {
            return string.Empty;
        }

        if (characterMap.TryGetValue(charID, out string name))
        {
            return name;
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
