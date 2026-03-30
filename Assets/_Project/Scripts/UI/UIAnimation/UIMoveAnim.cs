using UnityEngine;
using System.Collections;
using System;

public class UIMoveListener : UIListener
{
    [Serializable]
    public struct MoveConfig
    {
        public EaseParam ease;
        public float duration;
        public float delay;
        public Vector2 offset; // 相对 targetPos 的偏移
    }

    public MoveConfig openSettings = new MoveConfig { duration = 0.5f, offset = new Vector2(-250, 0) };
    public MoveConfig resumeSettings = new MoveConfig { duration = 0.5f, offset = new Vector2(-250, 0) };
    public MoveConfig closeSettings = new MoveConfig { duration = 0f, offset = Vector2.zero };
    public MoveConfig suspendSettings = new MoveConfig { duration = 0f, offset = Vector2.zero };

    protected RectTransform rectTransform;
    protected Vector2 targetPos;  // UI 在 Hierarchy 中原本的位置
    protected Coroutine moveRoutine;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        targetPos = rectTransform.anchoredPosition;
    }

    public override void Open() => ExecuteMove(openSettings, rectTransform.anchoredPosition = targetPos + openSettings.offset, targetPos);
    public override void Resume() => ExecuteMove(resumeSettings, rectTransform.anchoredPosition = moveRoutine == null ? targetPos + resumeSettings.offset : rectTransform.anchoredPosition, targetPos);
    public override void Close(Action onFinished) => ExecuteMove(closeSettings, rectTransform.anchoredPosition, targetPos + closeSettings.offset, onFinished);
    public override void Suspend(Action onFinished) => ExecuteMove(suspendSettings, rectTransform.anchoredPosition, targetPos + suspendSettings.offset, onFinished);

    public override void Abort()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
    }

    private void ExecuteMove(MoveConfig config, Vector2 from, Vector2 to, Action onDone = null)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);

        float distance = Vector2.Distance(from, to);
        // 考虑到可能只有 delay 没有 duration 的情况
        if (config.duration <= 0f && config.delay <= 0f || distance < 0.01f)
        {
            rectTransform.anchoredPosition = to;
            onDone?.Invoke();
            return;
        }

        moveRoutine = StartCoroutine(DoMove(config, from, to, onDone));
    }

    private IEnumerator DoMove(MoveConfig config, Vector2 from, Vector2 to, Action onDone)
    {
        // 增加延迟处理逻辑
        if (config.delay > 0)
        {
            yield return new WaitForSecondsRealtime(config.delay);
        }

        float elapsed = 0;
        // 以当前位置为起点，保证连点时的平滑
        Vector2 currentStart = rectTransform.anchoredPosition;

        while (elapsed < config.duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / config.duration);

            rectTransform.anchoredPosition = config.ease.Lerp(currentStart, to, t);
            yield return null;
        }

        rectTransform.anchoredPosition = to;
        onDone?.Invoke();

        moveRoutine = null;
    }

    // 编辑器里快速同步 TargetPos
    [ContextMenu("Update Target Position")]
    private void UpdateTargetPos()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        targetPos = rectTransform.anchoredPosition;
    }
}