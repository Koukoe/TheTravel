using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class conch : StaticStateEntity<ItemState>
{
    protected override void OnStateBound()
    {
        if (_state.isPicked) gameObject.SetActive(false);
        else gameObject.SetActive(true);
    }
}
