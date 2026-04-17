using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneTeleLogic : SceneBase
{
    public override void EnterScene()
    {
        Transform player = GameObjectsDefination.Instance.player.transform;
        if (player != null)
        {
            float x = PlayerPrefs.GetFloat("SceneTeleportX", 0);
            float y = PlayerPrefs.GetFloat("SceneTeleportY", 0);
            float z = PlayerPrefs.GetFloat("SceneTeleportZ", 0);
            player.position = new Vector3(x, y, z);
        }
    }

    public override void ExitScene()
    {
        // 场景退出时的清理逻辑（如果有需要）
        // 不要抛出异常
    }
}

public class SceneTeleport : MonoBehaviour
{
    public TeleportType teleportType = TeleportType.Portal;
    public Transform teleportTarget;
    public string TargetScene;

    private SceneTeleLogic sceneTeleLogic;
    private System.Action<InputAction.CallbackContext> interactHandler;
    private bool isSubscribed = false;

    void Start()
    {
        // 获取组件，如果不存在则添加
        sceneTeleLogic = GetComponent<SceneTeleLogic>();
        if (sceneTeleLogic == null && !string.IsNullOrEmpty(TargetScene))
        {
            Debug.LogWarning($"SceneTeleLogic component missing on {gameObject.name}");
        }
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

    private void PerformTeleport()
    {
        // 存储目标位置
        if (teleportTarget != null)
        {
            PlayerPrefs.SetFloat("SceneTeleportX", teleportTarget.position.x);
            PlayerPrefs.SetFloat("SceneTeleportY", teleportTarget.position.y);
            PlayerPrefs.SetFloat("SceneTeleportZ", teleportTarget.position.z);
            PlayerPrefs.Save();
        }

        // 同一场景内传送（不切换场景）
        if (string.IsNullOrEmpty(TargetScene))
        {
            Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player != null && teleportTarget != null)
            {
                player.position = teleportTarget.position;
            }
            return;
        }

        // 跨场景传送
        if (sceneTeleLogic != null)
        {
            GameSceneManager.Instance.ActivatePreloadedScene(TargetScene, sceneTeleLogic);
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