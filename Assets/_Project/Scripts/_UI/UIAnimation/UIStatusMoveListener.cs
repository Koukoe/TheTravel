using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class UIStatusTransformListener : UIListener, IUIAppearanceSource
{
    [Serializable]
    public struct TransformConfig
    {
        [FormerlySerializedAs("ease")]
        public EaseParam posEase;
        public EaseParam angleEase;
        public float duration;
        public float delay;

        [FormerlySerializedAs("offset")]
        public Vector3 posOffset;
        public Vector3 angleOffset;
    }

    [SerializeField] private bool isProvider = false;
    public bool IsProvider => isProvider;

    public TransformConfig openConfig;
    public TransformConfig closeConfig;
    public List<TransformConfig> resumeConfig = new List<TransformConfig> { new TransformConfig() };
    public List<TransformConfig> suspendConfig = new List<TransformConfig> { new TransformConfig() };

    protected override int StyleListCount => Math.Min(resumeConfig.Count, suspendConfig.Count);

    public bool useHideLogicForSuspend = true;

    [SerializeField] protected RectTransform rectTransform;
    [SerializeField] protected Vector3 targetPos;
    [SerializeField] protected Vector3 targetAngles;

    private Vector3 currentPosOffset = Vector3.zero;
    private Vector3 currentAngleOffset = Vector3.zero;
    public Vector3 PosOffset => currentPosOffset;
    public Vector3 AngleOffset => currentAngleOffset;
    public Vector3 ScaleMult => Vector3.one;
    public float AlphaMult => 1f;

    protected Coroutine moveRoutine;

    protected virtual void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        targetPos = rectTransform.anchoredPosition3D;
        targetAngles = rectTransform.localEulerAngles;
    }

    public override void Open() => StartMove(targetPos + openConfig.posOffset, targetPos, targetAngles + openConfig.angleOffset, targetAngles, openConfig, true, false);
    public override void Resume()
    {
        TransformConfig config = (_suspendStyle == -1) ? new TransformConfig() : resumeConfig[_suspendStyle];
        StartMove(targetPos + config.posOffset, targetPos, targetAngles + config.angleOffset, targetAngles, config, useHideLogicForSuspend, false);
    }
    public override void Close(Action onFinished) => StartMove(targetPos, targetPos + closeConfig.posOffset, targetAngles, targetAngles + closeConfig.angleOffset, closeConfig, false, true, onFinished);
    public override void Suspend(Action onFinished)
    {
        TransformConfig config = (_suspendStyle == -1) ? new TransformConfig() : suspendConfig[_suspendStyle];
        StartMove(targetPos, targetPos + config.posOffset, targetAngles, targetAngles + config.angleOffset, config, false, useHideLogicForSuspend, onFinished);
    }
    public override void Abort()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
    }

    private void StartMove(Vector3 fromPos, Vector3 toPos, Vector3 fromAngle, Vector3 toAngle, TransformConfig config, bool hideOnDelay, bool hideOnComplete, Action onDone = null)
    {
        Abort();

        if (hideOnDelay && config.delay > 0)
        {
            ApplyTransform(new Vector3(14514, 19810, 0), fromPos - targetPos, fromAngle, fromAngle - targetAngles);
        }
        else
        {
            ApplyTransform(fromPos, fromPos - targetPos, fromAngle, fromAngle - targetAngles);
        }

        if (config.duration <= 0f && config.delay <= 0f)
        {
            Vector3 finalPos = hideOnComplete ? new Vector3(14514, 19810, 0) : toPos;
            ApplyTransform(finalPos, toPos - targetPos, toAngle, toAngle - targetAngles);
            onDone?.Invoke();
            return;
        }

        moveRoutine = StartCoroutine(DoMove(fromPos, toPos, fromAngle, toAngle, config, hideOnDelay, hideOnComplete, onDone));
    }

    private IEnumerator DoMove(Vector3 fromPos, Vector3 toPos, Vector3 fromAngle, Vector3 toAngle, TransformConfig config, bool hideOnDelay, bool hideOnComplete, Action onDone)
    {
        if (config.delay > 0)
        {
            yield return new WaitForSecondsRealtime(config.delay);
            ApplyTransform(fromPos, fromPos - targetPos, fromAngle, fromAngle - targetAngles);
        }

        float elapsed = 0;
        while (elapsed < config.duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / config.duration);

            Vector3 curPos = config.posEase.Lerp(fromPos, toPos, t);
            Vector3 curAngle = config.angleEase.Lerp(fromAngle, toAngle, t);

            ApplyTransform(curPos, curPos - targetPos, curAngle, curAngle - targetAngles);
            yield return null;
        }

        Vector3 endPos = hideOnComplete ? new Vector3(14514, 19810, 0) : toPos;
        ApplyTransform(endPos, toPos - targetPos, toAngle, toAngle - targetAngles);
        onDone?.Invoke();
        moveRoutine = null;
    }

    private void ApplyTransform(Vector3 actualPos, Vector3 posOffset, Vector3 actualAngle, Vector3 angleOffset)
    {
        if (actualPos.x == 14514 && actualPos.y == 19810) currentPosOffset = new Vector3(14514, 19810, 0); // 抛出信号

        else currentPosOffset = posOffset; // 正常偏移

        currentAngleOffset = angleOffset;

        // 自己动
        if (!isProvider)
        {
            rectTransform.anchoredPosition3D = actualPos;
            rectTransform.localEulerAngles = actualAngle;
        }
    }

    private void OnDisable() => Abort();

    [ContextMenu("Update Target Position")]
    private void UpdateTargetPos()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        targetPos = rectTransform.anchoredPosition3D;
        targetAngles = rectTransform.localEulerAngles;
    }
}


// DOTween版本


/*

using UnityEngine;
using System;
using DG.Tweening; // 1

public class UITransformListener : UIListener
{
    [Serializable]
    public struct TransformConfig
    {
        public Ease posEase;   // 2
        public Ease angleEase; // 2
        public float duration;
        public float delay;
        public Vector3 posOffset;
        public Vector3 angleOffset;
    }

    public TransformConfig openConfig;
    public TransformConfig closeConfig;
    public TransformConfig resumeConfig;
    public TransformConfig suspendConfig;

    public bool useHideLogicForSuspend = true;

    [SerializeField] protected RectTransform rectTransform;
    [SerializeField] protected Vector3 targetPos;
    [SerializeField] protected Vector3 targetAngles;

    // 3

    protected virtual void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        targetPos = rectTransform.anchoredPosition3D;
        targetAngles = rectTransform.localEulerAngles;
    }

    public override void Open() => StartMove(targetPos + openConfig.posOffset, targetPos, targetAngles + openConfig.angleOffset, targetAngles, openConfig, true, false);
    public override void Resume() => StartMove(targetPos + resumeConfig.posOffset, targetPos, targetAngles + resumeConfig.angleOffset, targetAngles, resumeConfig, useHideLogicForSuspend, false);
    public override void Close(Action onFinished) => StartMove(targetPos, targetPos + closeConfig.posOffset, targetAngles, targetAngles + closeConfig.angleOffset, closeConfig, false, true, onFinished);
    public override void Suspend(Action onFinished) => StartMove(targetPos, targetPos + suspendConfig.posOffset, targetAngles, targetAngles + suspendConfig.angleOffset, suspendConfig, false, useHideLogicForSuspend, onFinished);

    public override void Abort()
    {
        // 杀死所有的位点动画
        rectTransform.DOKill();  // 4
    }

    // 5
    private void StartMove(Vector3 fromPos, Vector3 toPos, Vector3 fromAngle, Vector3 toAngle, TransformConfig config, bool hideOnDelay, bool hideOnComplete, Action onDone = null)
    {
        Abort();

        if (hideOnDelay && config.delay > 0)
        {
            rectTransform.anchoredPosition3D = new Vector3(14514, 19810, 0);
        }
        else
        {
            rectTransform.anchoredPosition3D = fromPos;
            rectTransform.localEulerAngles = fromAngle;
        }

        if (config.duration <= 0f && config.delay <= 0f)
        {
            rectTransform.anchoredPosition3D = hideOnComplete ? new Vector3(14514, 19810, 0) : toPos;
            rectTransform.localEulerAngles = toAngle;
            onDone?.Invoke();
            return;
        }

        // 分别应用不同的 Ease
        rectTransform.DOLocalRotate(toAngle, config.duration).SetEase(config.angleEase).SetDelay(config.delay).SetUpdate(true);
        rectTransform.DOAnchorPos3D(toPos, config.duration)
            .SetEase(config.posEase)
            .SetDelay(config.delay)
            .SetUpdate(true) // 相当于 Time.unscaledDeltaTime
            .OnStart(() => 
            {
                rectTransform.anchoredPosition3D = fromPos;
                rectTransform.localEulerAngles = fromAngle;
            })
            .OnComplete(() => 
            {
                if (hideOnComplete) rectTransform.anchoredPosition3D = new Vector3(14514, 19810, 0);
                onDone?.Invoke();
            });
    }

    private void OnDisable() => Abort();

    [ContextMenu("Update Target Position")]
    private void UpdateTargetPos()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        targetPos = rectTransform.anchoredPosition3D;
        targetAngles = rectTransform.localEulerAngles;
    }
}

*/