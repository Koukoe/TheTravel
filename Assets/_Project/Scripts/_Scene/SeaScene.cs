using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SeaSceneState : RealSceneState
{
    public List<string> bgmList = new List<string>();
}

public class SeaScene : RealScene<SeaSceneState>
{

    public override void EnterScene()
    {
        base.EnterScene();
        // 检测是否第一次在场景里，决定是否播放 BGM
        Playermove.Instance.OnSea = true;
    }

    public override void ExitScene()
    {
        base.ExitScene();
        // 停止 BGM
        // 设置 pendingPosition
        Playermove.Instance.OnSea = false;
    }
}