using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 侧边对话消息面板：由 UIManager/PollManager clone 到场景里
/// 通过静态方法 PushMessage 在非对话场景下也可以直接投递消息
/// </summary>
public class DialogueBubblePanel : BasePanel
{
    public static DialogueBubblePanel Instance { get; private set; }

    private const string PanelName = "DialogueBubblePanel";

    [SerializeField] private DialogueBubble bubbleTemplate;
    [SerializeField] private RectTransform bubbleContainer;
    [SerializeField, Min(0f)] private float bubbleSpacing = 8f;
    [SerializeField, Min(1)] private int maxVisibleBubbles = 9;

    // [Header("给OnClick用来测试的")]
    // [SerializeField, Tooltip("OnClick 调 SendMessage 时使用的发送者名称")]
    // private string onClickSpeaker = string.Empty;
    // [SerializeField, Tooltip("OnClick 调 SendMessage 时使用的消息内容")]
    // private string onClickMessage = string.Empty;

    private readonly List<DialogueBubble> activeBubbles = new List<DialogueBubble>();

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    public override void OnOpen()
    {
        base.OnOpen();
        if (bubbleContainer == null)
        {
            bubbleContainer = transform as RectTransform;
        }

        if (bubbleTemplate == null)
        {
            bubbleTemplate = GetComponentInChildren<DialogueBubble>(true);
        }

        // 模板仅用于克隆，始终保持隐藏
        if (bubbleTemplate != null)
        {
            bubbleTemplate.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 在气泡面板显示一条新消息, 重载方法简化无需说话者名称的调用
    /// </summary><param name="message">消息内容, 支持富文本</param>
    public static void PushMessage(string message)
    {
        PushMessage(string.Empty, message);
    }

    /// <summary>
    /// 在气泡面板显示一条新消息
    /// </summary><param name="speakerName">说话者名称(非必填)</param>
    /// <param name="message">消息内容</param>
    public static void PushMessage(string speakerName, string message)
    {
        DialogueBubblePanel panel = EnsurePanelInstance();
        if (panel == null)
        {
            return;
        }

        panel.PushMessageInternal(speakerName, message);
    }

    /// <summary>
    /// 供 Unity OnClick 调用的无参方法，使用 Inspector 配置的 onClickSpeaker/onClickMessage
    /// </summary>
    // public void SendMessage()
    // {
    //     PushMessage(onClickSpeaker, onClickMessage);
    // }

    private static DialogueBubblePanel EnsurePanelInstance()
    {
        if (Instance != null)
        {
            if (!Instance.gameObject.activeInHierarchy && UIManager.Instance != null)
            {
                BasePanel reopened = UIManager.Instance.Show(PanelName);
                if (reopened is DialogueBubblePanel reopenedPanel)
                {
                    return reopenedPanel;
                }
            }

            return Instance;
        }

        if (UIManager.Instance == null)
        {
            Debug.LogWarning("DialogueBubblePanel: UIManager 未初始化，无法显示侧边消息");
            return null;
        }

        // 可以可以, 差点忘记有Show方法了
        BasePanel panel = UIManager.Instance.Show(PanelName);
        if (panel is DialogueBubblePanel panelInstance)
        {
            return panelInstance;
        }

        Debug.LogWarning("DialogueBubblePanel: 无法从 UIManager 获取面板实例");
        return null;
    }

    private void PushMessageInternal(string speakerName, string message)
    {
        if (bubbleTemplate == null)
        {
            Debug.LogWarning("DialogueBubblePanel: bubbleTemplate 未配置");
            return;
        }

        if (bubbleContainer == null)
        {
            bubbleContainer = transform as RectTransform;
        }

        if (bubbleContainer == null)
        {
            Debug.LogWarning("DialogueBubblePanel: bubbleContainer 无效");
            return;
        }

        // 实例化气泡，新气泡默认在底部等待 Reflow 分配位置
        DialogueBubble bubble = Instantiate(bubbleTemplate, bubbleContainer);
        bubble.gameObject.SetActive(true);
        bubble.transform.SetAsLastSibling();

        string text = string.IsNullOrEmpty(speakerName)
            ? message ?? string.Empty
            : $"<b>{speakerName}</b>: {message ?? string.Empty}";

        bubble.Initialize(text, OnBubbleExpired);

        activeBubbles.Add(bubble);
        ReflowVisibleBubbles();

        // 超出上限时移除最旧的
        while (activeBubbles.Count > maxVisibleBubbles)
        {
            DialogueBubble oldest = activeBubbles[0];
            activeBubbles.RemoveAt(0);
            if (oldest != null) Destroy(oldest.gameObject);
        }
    }

    private void OnBubbleExpired(DialogueBubble bubble)
    {
        if (bubble == null)
        {
            return;
        }

        int index = activeBubbles.IndexOf(bubble);
        if (index >= 0)
        {
            activeBubbles.RemoveAt(index);
        }

        Destroy(bubble.gameObject);
        ReflowVisibleBubbles();

        if (activeBubbles.Count == 0 && UIManager.Instance != null)
        {
            UIManager.Instance.Hide(PanelName);
        }
    }

    /// <summary>
    /// 按先进先出的视觉顺序从下到上排列可见气泡：最新气泡在最底部 (y=0)。
    /// </summary>
    private void ReflowVisibleBubbles()
    {
        // 从列表末尾往前遍历，让最新的（last）在底部 (y=0)
        float currentY = 0f;
        for (int i = activeBubbles.Count - 1; i >= 0; i--)
        {
            DialogueBubble bubble = activeBubbles[i];
            if (bubble == null || bubble.IsExiting) continue;

            bubble.SetStackY(currentY);
            currentY += bubble.Height + bubbleSpacing;
        }
    }

    private void OnDestroy()
    {
        for (int i = activeBubbles.Count - 1; i >= 0; i--)
        {
            if (activeBubbles[i] != null)
            {
                Destroy(activeBubbles[i].gameObject);
            }
        }

        activeBubbles.Clear();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
