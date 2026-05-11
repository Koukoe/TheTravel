using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

public class SceneTeleport : MonoBehaviour
{
    public TeleportType teleportType = TeleportType.Portal;
    public Transform teleportTarget;
    public string TargetScene;
    Transform player;

    private SceneBase sceneTeleLogic;
    private System.Action<InputAction.CallbackContext> interactHandler;
    // private bool isSubscribed = false;

    void Start()
    {
        // 获取组件，如果不存在则添加
        sceneTeleLogic = GetComponent<RealScene>();
        if (sceneTeleLogic == null && !string.IsNullOrEmpty(TargetScene))
        {
            Debug.LogWarning($"SceneTeleLogic component missing on {gameObject.name}");
        }

        player = PlayerController.Instance.transform;
    }


    public async UniTask DoorTP()
    {
        await GameSceneManager.Instance.LoadMain(TargetScene);
        Debug.Log("Teleporting to " + TargetScene);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;


        if (teleportType == TeleportType.Portal)
        {
            PerformTeleport();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
    }

    private async void PerformTeleport()
    {
        // 同一场景内传送（不切换场景）
        if (string.IsNullOrEmpty(TargetScene))
        {
            if (player != null && teleportTarget != null)
            {
                player.position = teleportTarget.position;
            }
            return;
        }

        // 跨场景传送
        if (sceneTeleLogic != null)
        {
            await GameSceneManager.Instance.LoadMain(TargetScene);
        }
        else
        {
            Debug.LogError($"Cannot teleport to {TargetScene}: SceneTeleLogic missing");
        }
    }
}

public enum TeleportType
{
    Portal,  // 触碰即传送
    Door,    // 按交互键传送
    None
}