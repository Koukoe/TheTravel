using UnityEngine;

public class RadioKnobFreqEntity : StaticStateEntity<InteractionState>, IInteractable
{

    [SerializeField] private int _priority;
    [SerializeField] private Vector3 _tipOffset;

    public int Priority => _priority;

    public Transform InteractTransform => transform;
    public InteractTipWorld InteractTip { get; set; }
    public Vector3 TipOffset => _tipOffset;

    [SerializeField]
    private RadioBodyEntity _body;

    public InteractionState State
    {
        get => _state;
        private set => _state = value;
    }

    public bool CanInteract()
    {
        return _state.isInteracble;
    }

    public void DoInteract()
    {
        _state.stateIndex = (_state.stateIndex + 1) % 4;
        _body?.FinishRadio();
    }

    protected override void OnStateBound()
    {

    }
}