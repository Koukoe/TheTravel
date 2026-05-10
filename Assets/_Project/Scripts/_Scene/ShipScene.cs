using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ShipSceneState : EntitySceneState
{
}

public class ShipScene : EntityScene<ShipSceneState>
{
    public override void EnterScene()
    {
        base.EnterScene();
        Playermove.Instance.OnSea = false;
    }
}