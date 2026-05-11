using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class EntityNPC_Default : PoolStateEntity<ActorState>
{
    [SerializeField] Animator _animator;

    protected override void OnStateBound()
    {
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
