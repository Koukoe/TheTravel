using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipScene : RealScene
{

    // 船里面我还没想到写啥

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