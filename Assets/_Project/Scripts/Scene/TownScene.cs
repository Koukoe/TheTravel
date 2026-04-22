using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownScene : RealScene
{

    // BGM 逻辑暂且在港口

    public override void EnterScene()
    {
        base.EnterScene();
        Playermove.Instance.OnSea = false;
    }

    public override void ExitScene()
    {
        base.ExitScene();
    }
}