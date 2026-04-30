using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ShipSceneState : RealSceneState
{
    public HashSet<string> activeEntityGuids = new HashSet<string>();
}

public class ShipScene : EntityScene<ShipSceneState>
{
    // ...

    protected override IEnumerable<string> GetActiveGuids() => _state.activeEntityGuids;

    public override void EnterScene()
    {
        base.EnterScene();
        Playermove.Instance.OnSea = false;

        SpawnEntities();
    }
}