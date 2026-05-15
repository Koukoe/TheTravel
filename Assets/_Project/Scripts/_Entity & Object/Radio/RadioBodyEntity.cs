using System.Collections.Generic;
using UnityEngine;

public class RadioBodyEntity : StaticStateEntity<InteractionState>, IInteractable
{
    [SerializeField] private int _priority;
    [SerializeField] private Vector3 _tipOffset;
    [SerializeField] private Vector2 _targetFreqAndAmp;

    public int Priority => _priority;

    public Transform InteractTransform => transform;
    public InteractTipWorld InteractTip { get; set; }
    public Vector3 TipOffset => _tipOffset;

    public RadioKnobFreqEntity freqEntity;
    public RadioKnobAmpEntity ampEntity;

    public bool CanInteract()
    {
        return _state.isInteracble && !_state.isTriggered;
    }

    public void DoInteract()
    {
        _state.isTriggered = true;
        if (freqEntity != null && ampEntity != null)
        {
            freqEntity.State.isInteracble = true;
            ampEntity.State.isInteracble = true;
        }
    }

    public void FinishRadio()
    {
        _state.stateIndex = freqEntity.State.stateIndex * 4 + ampEntity.State.stateIndex;
        if (_state.stateIndex == _targetFreqAndAmp.x * 4 + _targetFreqAndAmp.y)
        {
            // Finish Task
        }
    }

    protected override void OnStateBound()
    {

    }
}