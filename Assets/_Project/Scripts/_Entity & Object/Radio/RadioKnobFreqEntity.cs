using UnityEngine;

/// <summary>
/// 频率旋钮：交互时根据 stateIndex (0~3) 映射到连续比例，通知 Body 调 WaveDecode 的频率。
/// </summary>
public class RadioKnobFreqEntity : StaticStateEntity<InteractionState>, IInteractable
{
    [SerializeField] private int _priority;
    [SerializeField] private Vector3 _tipOffset;

    public int Priority => _priority;
    public Transform InteractTransform => transform;
    public InteractTipWorld InteractTip { get; set; }
    public Vector3 TipOffset => _tipOffset;

    [SerializeField] private RadioBodyEntity _body;

    public InteractionState State
    {
        get => _state;
        private set => _state = value;
    }

    public bool CanInteract() => _state.isInteracble;

    public void DoInteract()
    {
        // 循环 0→1→2→3→0
        _state.stateIndex = (_state.stateIndex + 1) % 4;
        // 映射到 0~1 连续比例
        float ratio = _state.stateIndex / 3f;
        _body?.OnKnobAdjusted(KnobType.Frequency, ratio);
    }

    protected override void OnStateBound() { }
}
