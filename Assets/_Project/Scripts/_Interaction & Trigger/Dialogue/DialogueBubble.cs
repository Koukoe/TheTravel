using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 对话气泡
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class DialogueBubble : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField, Tooltip("自身 RectTransform, Awake 自动获取")]
    private RectTransform rectTransform;
    [SerializeField, Tooltip("CanvasGroup, 用于淡入淡出, Awake 自动获取")]
    private CanvasGroup canvasGroup;
    [SerializeField, Tooltip("背景 Image, Awake 自动获取")]
    private Image backgroundImage;
    [SerializeField, Tooltip("消息 TMP_Text, 从子物体 'Text' 查找")]
    private TMP_Text messageText;
    [SerializeField, Tooltip("文字 RectTransform, 用于设置内边距偏移")]
    private RectTransform messageTextRect;

    [Header("入场")]
    [SerializeField, Min(0f), Tooltip("从右侧滑入的时长（秒）")]
    private float appearDuration = 0.25f;
    [SerializeField, Tooltip("入场时从右侧多少像素外滑入")]
    private float slideInOffsetX = 60f;

    [Header("停留")]
    [SerializeField, Min(0f), Tooltip("完全显示后停留多久才开始离场（秒）")]
    private float stayDuration = 2.5f;

    [Header("离场")]
    [SerializeField, Min(0f), Tooltip("上浮移动的时长（秒）")]
    private float moveOutDuration = 0.35f;
    [SerializeField, Min(0f), Tooltip("淡出时长（秒）, 上浮结束后执行")]
    private float disappearDuration = 0.2f;
    [SerializeField, Tooltip("离场时向上移动多少像素")]
    private float moveOutDistanceY = 560f;

    [Header("尺寸")]
    [SerializeField, Tooltip("气泡最大宽度（像素）, 超过后文字换行")]
    private float maxBubbleWidth = 320f;
    [SerializeField, Tooltip("气泡最小高度（像素）")]
    private float minBubbleHeight = 40f;
    [SerializeField, Tooltip("文字四周内边距: x=水平, y=垂直")]
    private Vector2 textPadding = new Vector2(14f, 10f);

    private const string TextChildName = "Text";
    private bool anchorsFixed;

    private Coroutine lifecycleRoutine;
    private Action<DialogueBubble> expiredCallback;
    private float stackY;
    private bool isExiting;

    public bool IsExiting => isExiting;

    public float Height
    {
        get
        {
            if (rectTransform == null)
            {
                return 0f;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            return rectTransform.rect.height;
        }
    }

    private void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (backgroundImage == null) backgroundImage = GetComponent<Image>();

        if (messageText == null)
        {
            Transform child = transform.Find(TextChildName);
            if (child != null) messageText = child.GetComponent<TMP_Text>();
        }
        if (messageTextRect == null && messageText != null)
            messageTextRect = messageText.rectTransform;

        // 强制启用自动换行，无论 prefab 设了什么
        if (messageText != null)
        {
            messageText.enableWordWrapping = true;
        }
    }

    public void Initialize(string message, Action<DialogueBubble> onExpired)
    {
        expiredCallback = onExpired;

        if (!anchorsFixed)
            FixAnchors();

        if (messageText != null)
        {
            messageText.text = message ?? string.Empty;
            ResizeToText(messageText.text);
        }

        if (lifecycleRoutine != null) StopCoroutine(lifecycleRoutine);
        lifecycleRoutine = StartCoroutine(Lifecycle());
    }

    public void SetStackY(float y)
    {
        stackY = y;
        SetAnchoredY(y);
    }

    public void ShiftY(float delta)
    {
        SetStackY(stackY + delta);
    }

    private IEnumerator Lifecycle()
    {
        isExiting = false;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        float appearStartX = slideInOffsetX;
        float appearEndX = 0f;

        yield return Animate(appearDuration, t =>
        {
            float eased = EasingUtils.GetValue(EasingUtils.EaseType.OutQuad, t);
            SetAnchoredPosition(Mathf.Lerp(appearStartX, appearEndX, eased), stackY);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, eased);
            }
        });

        yield return new WaitForSeconds(stayDuration);

        isExiting = true;
        float exitStartY = stackY;
        float exitEndY = exitStartY + moveOutDistanceY;

        // 先移动, 再淡出
        yield return Animate(moveOutDuration, t =>
        {
            float eased = EasingUtils.GetValue(EasingUtils.EaseType.InQuad, t);
            SetAnchoredPosition(0f, Mathf.Lerp(exitStartY, exitEndY, eased));
        });

        yield return Animate(disappearDuration, t =>
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            }
        });

        expiredCallback?.Invoke(this);
    }

    private IEnumerator Animate(float duration, Action<float> onStep)
    {
        if (duration <= 0f)
        {
            onStep?.Invoke(1f);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            onStep?.Invoke(Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        onStep?.Invoke(1f);
    }

    // ────── 位置 ──────

    private void SetAnchoredPosition(float x, float y)
    {
        if (rectTransform != null) rectTransform.anchoredPosition = new Vector2(x, y);
    }

    private void SetAnchoredY(float y)
    {
        if (rectTransform != null)
        {
            Vector2 p = rectTransform.anchoredPosition;
            p.y = y;
            rectTransform.anchoredPosition = p;
        }
    }

    /// <summary>
    /// 将 anchor/pivot 固定为右下角 (1,0),
    /// 使 anchoredPosition.x = 距面板右边距（负值向左），y = 距面板底部堆叠高度
    /// </summary>
    private void FixAnchors()
    {
        if (rectTransform == null) return;
        rectTransform.anchorMin = new Vector2(1f, 0f);
        rectTransform.anchorMax = new Vector2(1f, 0f);
        rectTransform.pivot = new Vector2(1f, 0f);
        anchorsFixed = true;
    }

    // 尺寸自适应（文字换行 + 内边距）
    private void ResizeToText(string message)
    {
        if (rectTransform == null || messageText == null) return;

        // 先占满再计算 preferred, 确保换行宽度正确
        if (messageTextRect != null)
        {
            messageTextRect.anchorMin = Vector2.zero;
            messageTextRect.anchorMax = Vector2.one;
            messageTextRect.sizeDelta = Vector2.zero;
        }

        float innerWidth = Mathf.Max(1f, maxBubbleWidth - textPadding.x * 2f);
        Vector2 preferred = messageText.GetPreferredValues(message ?? string.Empty, innerWidth, 0f);

        float bw = Mathf.Max(1f, Mathf.Min(maxBubbleWidth, preferred.x + textPadding.x * 2f));
        float bh = Mathf.Max(minBubbleHeight, preferred.y + textPadding.y * 2f);

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bw);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bh);

        // 内边距
        if (messageTextRect != null)
        {
            messageTextRect.anchorMin = Vector2.zero;
            messageTextRect.anchorMax = Vector2.one;
            messageTextRect.offsetMin = new Vector2(textPadding.x, textPadding.y);
            messageTextRect.offsetMax = new Vector2(-textPadding.x, -textPadding.y);
        }
    }
}
