using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TownSceneState : RealSceneState
{
    public HashSet<string> activeEntityGuids = new HashSet<string>();
}

public class TownScene : EntityScene<TownSceneState>
{
    protected override IEnumerable<string> GetActiveGuids() => _state.activeEntityGuids;

    public override void EnterScene()
    {
        base.EnterScene();
        Playermove.Instance.OnSea = false;

        SpawnEntities();

    }
}