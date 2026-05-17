using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TownSceneState : EntitySceneState
{
}

public class TownScene : EntityScene<TownSceneState>
{
    public float camY;
    public override void EnterScene()
    {
        base.EnterScene();
        Playermove.Instance.OnSea = false;
        if (camY != 0)
        {
            PlayerController.mainCam.GetComponent<CameraMove>().CamY = camY;
        }
    }
}