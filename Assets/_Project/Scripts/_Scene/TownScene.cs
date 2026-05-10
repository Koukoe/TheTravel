using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TownSceneState : EntitySceneState
{
    public HashSet<string> activeEntityGuids = new HashSet<string>();
}

public class TownScene : EntityScene<TownSceneState>
{
    public override void EnterScene()
    {
        base.EnterScene();
        Playermove.Instance.OnSea = false;
    }
}