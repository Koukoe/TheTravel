using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class HouseSceneState : EntitySceneState
{
}

public class HouseScene : EntityScene<HouseSceneState>
{
    public override void EnterScene()
    {
        base.EnterScene();
        Playermove.Instance.OnSea = false;

    }
}