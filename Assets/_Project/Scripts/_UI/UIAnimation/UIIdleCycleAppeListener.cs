using UnityEngine;
using System;

public class UIAppearanceIdle : UIListener, IUIAppearanceSource
{
    [SerializeField] private bool isProvider = false;

    [SerializeField] private Vector3 floatRange = new Vector3(0, 10f, 0);
    [SerializeField] private float floatFrequency = 2.0f;
    [SerializeField] private float scaleAmplitude = 0.05f;
    [SerializeField] private float scaleFrequency = 2.0f;
    [SerializeField] private bool randomStart = false;

    [Tooltip("Suspend 是否会等待运动回到原点")]
    [SerializeField] private bool smoothSuspend = true;

    public bool IsProvider => isProvider && enabled;
    public Vector3 PosOffset { get; private set; }
    public Vector3 AngleOffset => Vector3.zero;
    public Vector3 ScaleMult { get; private set; } = Vector3.one;
    public float AlphaMult => 1f;

    private float timer;
    private float lastSin;
    private bool isPendingDeactivation = false;
    private Action pendingCallback;

    [SerializeField] private RectTransform rect;
    private Vector3 initialPos;

    private void Awake()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        initialPos = rect.anchoredPosition3D;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // 计算当前相位
        float currentSin = Mathf.Sin(2 * Mathf.PI * timer / floatFrequency);

        // 检测穿过平衡点
        if (isPendingDeactivation && smoothSuspend)
        {
            // 当两帧相位异号，视为越过 0 点
            if (lastSin * currentSin <= 0)
            {
                Deactivate();
                return;
            }
        }
        lastSin = currentSin;

        PosOffset = floatRange * currentSin;
        float sSin = Mathf.Sin(2 * Mathf.PI * timer / scaleFrequency);
        float s = 1f + (sSin * scaleAmplitude);
        ScaleMult = new Vector3(s, s, s);

        if (!isProvider)
        {
            rect.anchoredPosition3D = initialPos + PosOffset;
            rect.localScale = ScaleMult;
        }
    }

    private void Deactivate()
    {
        isPendingDeactivation = false;
        this.enabled = false;

        PosOffset = Vector3.zero;
        ScaleMult = Vector3.one;

        if (rect != null)
        {
            rect.anchoredPosition3D = initialPos;
            rect.localScale = Vector3.one;
        }

        pendingCallback?.Invoke();
        pendingCallback = null;
    }

    public override void Open()
    {
        isPendingDeactivation = false;
        this.enabled = true;
        if (randomStart) timer = UnityEngine.Random.Range(0f, floatFrequency * scaleFrequency);
    }

    public override void Resume()
    {
        isPendingDeactivation = false;
        this.enabled = true;
    }

    public override void Suspend(Action onFinished)
    {
        if (smoothSuspend)
        {
            isPendingDeactivation = true;
            pendingCallback = onFinished;
        }
        else
        {
            pendingCallback = onFinished;
            Deactivate();
        }
    }

    public override void Close(Action onFinished)
    {
        pendingCallback = onFinished;
        Deactivate();
    }

    public override void Abort() => Deactivate();
}