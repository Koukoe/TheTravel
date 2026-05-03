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

    public Transform shipPosition;

    public override void EnterScene()
    {
        base.EnterScene();
        Playermove.Instance.OnSea = false;

        SpawnEntities();

        GameObject ship = GameObject.FindWithTag("ship");
        ship.transform.position = shipPosition.position;
        ship.transform.rotation = shipPosition.rotation * Quaternion.Euler(0, -90, 0);
    }
}