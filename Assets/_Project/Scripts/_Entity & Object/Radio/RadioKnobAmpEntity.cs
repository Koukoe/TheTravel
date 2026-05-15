using UnityEngine;

/// <summary>
/// 振幅旋钮：交互时根据 stateIndex (0~3) 映射到连续比例，通知 Body 调 WaveDecode 的振幅。
/// </summary>
public class RadioKnobAmpEntity : StaticStateEntity<InteractionState>, IInteractable
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
        _state.stateIndex = (_state.stateIndex + 1) % 4;
        float ratio = _state.stateIndex / 3f;
        _body?.OnKnobAdjusted(KnobType.Amplitude, ratio);
    }

    protected override void OnStateBound() { }
}
