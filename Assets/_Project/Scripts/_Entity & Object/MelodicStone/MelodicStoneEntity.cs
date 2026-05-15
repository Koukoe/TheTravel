using System.Collections.Generic;
using UnityEngine;

public class MelodicStonesEntity : StaticStateEntity<InteractionState>
{
    public InteractionState GetState { get => _state; private set { _state = value; } }

    [SerializeField] private int[] melodies;
    [SerializeField] private int melodyLength;

    protected override void OnStateBound()
    {

    }

    public void JugdeSolfege(int solfege)
    {
        if (melodies[_state.stateIndex] == solfege)
        {
            _state.stateIndex++;
            AudioManager.Instance.PlaySFX($"StoneMelody_{solfege}");
        }
        else
        {
            _state.stateIndex = 0;
            AudioManager.Instance.PlaySFX($"StoneMelody_Error");
        }

        if (_state.stateIndex == melodyLength - 1)
        {
            // Finish Task
        }
    }
}
