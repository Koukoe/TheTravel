using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// /// 收音机主体：交互启动解密，监听波形解密完成事件。
/// Knob 旋钮调的是 WaveDecodeController 的连续值，不再用离散档位。
/// </summary>
public class RadioBodyEntity : StaticStateEntity<InteractionState>, IInteractable
{
    [Header("交互")]
    [SerializeField] private int _priority;
    [SerializeField] private Vector3 _tipOffset;
    public int Priority => _priority;
    public Transform InteractTransform => transform;
    public InteractTipWorld InteractTip { get; set; }
    public Vector3 TipOffset => _tipOffset;

    [Header("引用")]
    public RadioKnobFreqEntity freqEntity;
    public RadioKnobAmpEntity ampEntity;
    public WaveDecodeController waveDecoder;

    // 直接用波形匹配完成触发 item 收集，不再暴露额外事件
    public TextAsset getRecord;

    public bool CanInteract()
    {
        return _state.isInteracble && !_state.isTriggered;
    }

    public void DoInteract()
    {
        if (_state.isTriggered) return;
        _state.isTriggered = true;

        // 启用两个旋钮
        if (freqEntity != null) freqEntity.State.isInteracble = true;
        if (ampEntity != null) ampEntity.State.isInteracble = true;

        // 生成新谜题
        if (waveDecoder != null)
        {
            waveDecoder.NewPuzzle();
        }
    }

    /// <summary>
    /// Knob 旋钮交互时，WaveDecodeController 不再校验离散档位，
    /// 改为走连续值检测，所以 Knob 只负责改值，不再调 FinishRadio。
    /// 解密完成由 WaveDecodeController 的事件触发。
    /// </summary>
    public void OnKnobAdjusted(KnobType type, float stepRatio)
    {
        if (waveDecoder == null) return;

        if (type == KnobType.Frequency)
        {
            float val = Mathf.Lerp(0.5f, 10f, stepRatio);
            waveDecoder.SetFrequency(val);
        }
        else if (type == KnobType.Amplitude)
        {
            float val = Mathf.Lerp(0.1f, 2f, stepRatio);
            waveDecoder.SetAmplitude(val);
        }
    }

    private void OnDecodeSuccess()
    {
        GameFlowManager.Instance.PlayingData.GetState<ItemState>("record").isPicked = true;
        DialogueManager.Instance.StartWith(getRecord);
        AudioManager.Instance.PlaySFX("ClassicMusic");

        // 解密完成后禁掉旋钮交互
        if (freqEntity != null) freqEntity.State.isInteracble = false;
        if (ampEntity != null) ampEntity.State.isInteracble = false;
    }

    protected override void Start()
    {
        base.Start();

        // 监听解密完成事件，只绑定一次
        if (waveDecoder != null)
            waveDecoder.onDecodeSuccess.AddListener(OnDecodeSuccess);
    }

    protected override void OnStateBound() { }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (waveDecoder != null)
            waveDecoder.onDecodeSuccess.RemoveListener(OnDecodeSuccess);
    }
}

public enum KnobType
{
    Frequency,
    Amplitude
}
