using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class EntityPoolNPC_Default : PoolStateEntity<ActorState>
{
    [SerializeField] Animator _animator;

    protected override void OnStateBound()
    {
        // 场景变更检测：state 的 scene 与当前实体所在场景不符 → 自动回收
        if (!string.IsNullOrEmpty(_state.scene) && _state.scene != gameObject.scene.name)
        {
            // 不通过 ReturnToPool（会 SyncActorStateFromTransform 覆盖 scene），直接回收
            if (_state != null) _state.OnDataChanged -= OnStateBound;
            // PoolManager.Release(gameObject);
            return;
        }

        if (_state.position != null) gameObject.transform.position = _state.position.Value;
        if (_state.rotation != null) gameObject.transform.rotation = Quaternion.Euler(_state.rotation.Value);

        gameObject.SetActive(_state.isVisible);

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
