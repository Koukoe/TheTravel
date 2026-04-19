using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaScene : RealScene
{
    static List<string> bgmList;

    public override void EnterScene()
    {
        base.EnterScene();
        // 检测是否第一次在场景里，决定是否播放 BGM
    }

    public override void ExitScene()
    {
        base.ExitScene();
        // 停止 BGM
        // 设置 pendingPosition
    }
}