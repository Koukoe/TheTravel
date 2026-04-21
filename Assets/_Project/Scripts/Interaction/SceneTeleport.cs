using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneTeleport : MonoBehaviour
{
    public TeleportType teleportType = TeleportType.Portal;
    public Transform teleportTarget;
    public string TargetScene;
    Transform player;

    private SceneBase sceneTeleLogic;
    private System.Action<InputAction.CallbackContext> interactHandler;
    private bool isSubscribed = false;

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

    private void OnDestroy()
    {
        // 确保清理订阅，防止内存泄漏
        if (isSubscribed && interactHandler != null)
        {
            InputManager.Instance.PlayerDynamicActions.Interact.performed -= interactHandler;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;


        if (teleportType == TeleportType.Portal)
        {
            PerformTeleport();
        }
        else if (teleportType == TeleportType.Door)
        {
            SubscribeToInteract();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;


        if (teleportType == TeleportType.Door)
        {
            UnsubscribeFromInteract();
        }
    }

    private void SubscribeToInteract()
    {
        if (isSubscribed) return;

        interactHandler = ctx => PerformTeleport();
        InputManager.Instance.PlayerDynamicActions.Interact.performed += interactHandler;
        isSubscribed = true;
    }

    private void UnsubscribeFromInteract()
    {
        if (!isSubscribed || interactHandler == null) return;

        InputManager.Instance.PlayerDynamicActions.Interact.performed -= interactHandler;
        isSubscribed = false;
    }

    private async void PerformTeleport()
    {
        // // 存储目标位置
        // if (teleportTarget != null)
        // {
        //     PlayerPrefs.SetFloat("SceneTeleportX", teleportTarget.position.x);
        //     PlayerPrefs.SetFloat("SceneTeleportY", teleportTarget.position.y);
        //     PlayerPrefs.SetFloat("SceneTeleportZ", teleportTarget.position.z);
        //     PlayerPrefs.Save();
        // }

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