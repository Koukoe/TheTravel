using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorFulTown_NPC_02 : StaticStateEntity<ActorState>
{
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
    }
}
