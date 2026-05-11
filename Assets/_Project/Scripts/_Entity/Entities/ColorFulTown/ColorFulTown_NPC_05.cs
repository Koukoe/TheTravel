using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorFulTown_NPC_05 : StaticStateEntity<ActorState>
{
    [SerializeField] Animator _animator;
    protected override void OnStateBound()
    {
        if (_state.position != null) gameObject.transform.position = _state.position.Value;
        if (_state.rotation != null) gameObject.transform.rotation = Quaternion.Euler(_state.rotation.Value);
        if (_state.isVisible)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
        if (_state.animState == ActorState.AnimState.SIT)
        {
            _animator.SetBool("isSitting", true);
            _animator.SetFloat("moveAmount", 0);
        }
        else
        {
            _animator.SetBool("isSitting", false);

            if (_state.animState == ActorState.AnimState.WALK) _animator.SetFloat("moveAmount", 0.5f);
            else if (_state.animState == ActorState.AnimState.RUN) _animator.SetFloat("moveAmount", 1f);
            else { _animator.SetFloat("moveAmount", 0f); }
        }
    }
}
