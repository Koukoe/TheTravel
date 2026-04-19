using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortScene : RealScene
{
    static bool fromSea;

    static string bgmName;

    public override void EnterScene()
    {
        if (fromSea)
        {
            // 这里写船进港的动画
            AudioManager.Instance.PlayBGM(bgmName, 1f);  // 播放对应小镇 BGM
        }
        else
        {
            base.EnterScene();  // 把默认的位置设置在从小镇出来的位置
        }
    }

    public override void ExitScene()
    {
        base.ExitScene();
        AudioManager.Instance.StopBGM(StopTarget.Oldest, 1f);
    }
}