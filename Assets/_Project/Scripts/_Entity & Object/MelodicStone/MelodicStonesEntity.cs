using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MelodicStonesEntity : StaticStateEntity<InteractionState>
{
    public InteractionState GetState { get => _state; private set { _state = value; } }

    [SerializeField] private int[] _melody;

    public int MelodyLength => _melody != null ? _melody.Length : 0;

    public TextAsset dialogueText;
    public string itemID;

    protected override void OnStateBound()
    {

    }

    public void PlayTargetMelody()
    {
        StartCoroutine(PlayMelodyCoroutine());
    }

    private IEnumerator PlayMelodyCoroutine()
    {
        if (_melody == null) yield break;

        for (int i = 0; i < _melody.Length; i++)
        {
            AudioManager.Instance.PlaySFX($"StoneMelody_{_melody[i]}");
            yield return new WaitForSeconds(0.6f);
        }
    }

    public void JugdeSolfege(int solfege)
    {
        if (_melody[_state.stateIndex] == solfege)
        {
            _state.stateIndex++;
            AudioManager.Instance.PlaySFX($"StoneMelody_{solfege}");
        }
        else
        {
            _state.stateIndex = 0;
            AudioManager.Instance.PlaySFX($"StoneMelody_Error");
        }

        if (_state.stateIndex == _melody.Length)
        {
            // Finish Task
            DialogueManager.Instance.StartWith(dialogueText);
            GameFlowManager.Instance.PlayingData.GetState<ItemState>(itemID).isPicked = true;
            DialogueManager.Instance.SetDialogueIndex("stonetablet", 2);
        }
    }
}
