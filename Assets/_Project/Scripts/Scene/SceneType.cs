using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

public class RealScene : SceneBase
{
    private static Transform player;

    [SerializeField] private Transform defaultSpawnPoint;
    private static Vector3? pendingPosition;

    static RealScene()
    {
        player = PlayerController.Instance.transform;
    }

    public override void EnterScene()
    {
        CameraLink.Instance.LinkToMainCamera();

        if (pendingPosition.HasValue)
        {
            // 传送到指定点
            player.position = pendingPosition.Value;
            pendingPosition = null;
        }
        else if (defaultSpawnPoint != null)
        {
            // 默认出生点
            player.position = defaultSpawnPoint.position;
        }
        else
        {
            Debug.LogWarning($"场景 {gameObject.scene.name} 没有设置默认出生点，玩家位置未改变");
        }
    }

    public override void ExitScene()
    {
        // 场景退出时的清理逻辑（如果有需要）
        // 不要抛出异常
    }
}

