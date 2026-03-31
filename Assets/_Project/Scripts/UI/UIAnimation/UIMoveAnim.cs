using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization;

public class UIMoveListener : UIListener
{
    [Serializable]
    public struct MoveConfig
    {
        public EaseParam ease;
        public float duration;
        public float delay;
        public Vector2 offset;
        public bool hideOnDelay;
    }

    public MoveConfig openConfig;
    public MoveConfig closeConfig;
    public MoveConfig resumeConfig;
    public MoveConfig suspendConfig;

    [SerializeField] protected RectTransform rectTransform;
    [SerializeField] protected Vector2 targetPos;

    protected Coroutine moveRoutine;

    protected virtual void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        targetPos = rectTransform.anchoredPosition;
    }

    public override void Open() => StartMove(targetPos + openConfig.offset, targetPos, openConfig);
    public override void Resume() => StartMove(targetPos + suspendConfig.offset, targetPos, resumeConfig);
    public override void Close(Action onFinished) => StartMove(targetPos, targetPos + closeConfig.offset, closeConfig, onFinished);
    public override void Suspend(Action onFinished) => StartMove(targetPos, targetPos + suspendConfig.offset, suspendConfig, onFinished);

    public override void Abort()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
    }

    private void StartMove(Vector2 from, Vector2 to, MoveConfig config, Action onDone = null)
    {
        Abort();

        if (config.hideOnDelay && config.delay > 0)
        {
            rectTransform.anchoredPosition = new Vector2(10000, 10000);
        }
        else
        {
            rectTransform.anchoredPosition = from;
        }

        if (config.duration <= 0f && config.delay <= 0f)
        {
            rectTransform.anchoredPosition = to;
            onDone?.Invoke();
            return;
        }

        moveRoutine = StartCoroutine(DoMove(from, to, config, onDone));
    }

    private IEnumerator DoMove(Vector2 from, Vector2 to, MoveConfig config, Action onDone)
    {
        if (config.delay > 0)
        {
            yield return new WaitForSecondsRealtime(config.delay);
            rectTransform.anchoredPosition = from;
        }

        float elapsed = 0;
        while (elapsed < config.duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / config.duration);

            rectTransform.anchoredPosition = config.ease.Lerp(from, to, t);
            yield return null;
        }

        rectTransform.anchoredPosition = to;
        onDone?.Invoke();
        moveRoutine = null;
    }

    private void OnDisable() => Abort();

    [ContextMenu("Update Target Position")]
    private void UpdateTargetPos()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        targetPos = rectTransform.anchoredPosition;
    }
}



// DOTween版本

/*

using UnityEngine;
using System;
using DG.Tweening; // 1

public class UIMoveListener : UIListener
{
    [Serializable]
    public struct MoveConfig
    {
        public Ease ease;      // 2
        public float duration;
        public float delay;
        public Vector2 offset;
        public bool hideOnDelay;
    }

    public MoveConfig openConfig;
    public MoveConfig closeConfig;
    public MoveConfig resumeConfig;
    public MoveConfig suspendConfig;

    [SerializeField] protected RectTransform rectTransform;
    [SerializeField] protected Vector2 targetPos;

    // 3

    protected virtual void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        targetPos = rectTransform.anchoredPosition;
    }

    public override void Open() => StartMove(targetPos + openConfig.offset, targetPos, openConfig);
    public override void Resume() => StartMove(targetPos + suspendConfig.offset, targetPos, resumeConfig);
    public override void Close(Action onFinished) => StartMove(targetPos, targetPos + closeConfig.offset, closeConfig, onFinished);
    public override void Suspend(Action onFinished) => StartMove(targetPos, targetPos + suspendConfig.offset, suspendConfig, onFinished);

    public override void Abort()
    {
        // 杀死所有的位点动画
        rectTransform.DOKill();  // 4
    }

    // 5
    private void StartMove(Vector2 from, Vector2 to, MoveConfig config, Action onDone = null)
    {
        Abort();

        if (config.hideOnDelay && config.delay > 0)
        {
            rectTransform.anchoredPosition = new Vector2(10000, 10000);
        }
        else
        {
            rectTransform.anchoredPosition = from;
        }

        if (config.duration <= 0f && config.delay <= 0f)
        {
            rectTransform.anchoredPosition = to;
            onDone?.Invoke();
            return;
        }

        rectTransform.DOAnchorPos(to, config.duration)
            .SetEase(config.ease)
            .SetDelay(config.delay)
            .SetUpdate(true) // 相当于 Time.unscaledDeltaTime
            .OnStart(() => 
            {
                rectTransform.anchoredPosition = from;
            })
            .OnComplete(() => onDone?.Invoke());
    }

    private void OnDisable() => Abort();

    [ContextMenu("Update Target Position")]
    private void UpdateTargetPos()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        targetPos = rectTransform.anchoredPosition;
    }
}

*/