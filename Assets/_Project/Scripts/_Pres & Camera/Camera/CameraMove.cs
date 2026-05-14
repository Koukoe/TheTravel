using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform target;
    public CameraMode currentMode;
    public float smoothSpeed = 0.125f;
    public float CamY = StaticDefination.CameraY;
    private bool isSmoothActive = true;

    // 自由视角专用变量
    private float freePitch = 25f;      // 俯角
    private float freeYaw = 180f;         // 水平角
    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        if (target == null)
        {
            Debug.Log("未找到玩家，相机将保持静止");
            currentMode = CameraMode.Stay;
        }
    }

    private Vector3 calPos()
    {
        if (target == null) return PlayerController.mainCam.transform.position;

        // 水平方向（相机后方）
        Vector3 horizontalDir = -PlayerController.mainCam.transform.forward;
        horizontalDir.y = 0;
        if (horizontalDir.magnitude < 0.001f) horizontalDir = Vector3.back;
        horizontalDir.Normalize();

        // 俯角（取正值）
        float pitchRad = Mathf.Asin(Mathf.Clamp(-PlayerController.mainCam.transform.forward.y, 0.01f, 0.99f));
        float horizontalDistance = CamY / Mathf.Tan(pitchRad);
        horizontalDistance = Mathf.Min(horizontalDistance, 50f);

        return target.position + horizontalDir * horizontalDistance + Vector3.up * CamY;
    }

    private void cameraFollow()
    {
        Vector3 targetPosition = calPos();
        Vector3 smoothPos = Vector3.Lerp(PlayerController.mainCam.transform.position, targetPosition, smoothSpeed);
        PlayerController.mainCam.transform.position = isSmoothActive ? smoothPos : targetPosition;
    }

    private void cameraFree(Vector2 mouseMove)
    {
        if (target == null) return;

        // 直接使用 CamY 作为球半径（相机到目标的距离）
        float distance = CamY;

        // 更新角度
        freeYaw += mouseMove.x;
        freePitch -= mouseMove.y;

        // 限制俯角范围：-90 到 90 度（半球面）
        freePitch = Mathf.Clamp(freePitch, 20f, 70f);

        // 球面坐标转直角坐标（半径为 CamY）
        Quaternion rotation = Quaternion.Euler(freePitch, freeYaw, 0);
        Vector3 offset = rotation * Vector3.back * distance;

        // 相机位置 = 目标位置 + 偏移
        Vector3 targetPosition = target.position + offset;

        // 平滑移动
        Vector3 smoothPos = Vector3.Lerp(PlayerController.mainCam.transform.position, targetPosition, smoothSpeed);
        PlayerController.mainCam.transform.position = smoothPos;

        // 让相机看向目标
        PlayerController.mainCam.transform.LookAt(target);
    }

    void LateUpdate()
    {
        if (currentMode == CameraMode.Stay) return;
        if (currentMode == CameraMode.Follow)
        {
            cameraFollow();
        }
        if (currentMode == CameraMode.Free)
        {
            cameraFree(InputManager.Instance.GetLook());
        }
    }

    public void changeSeaCam(bool isOnSea = false)
    {
        if (isOnSea)
        {
            currentMode = CameraMode.Free;
            CamY = StaticDefination.CameraYSea;
            // 重置角度，确保相机在玩家后方
            freePitch = 25f;
            freeYaw = 0f;
            Debug.Log("进入海上模式：自由视角");
        }
        else
        {
            currentMode = CameraMode.Follow;
            CamY = StaticDefination.CameraY;
            Debug.Log("进入陆地模式：跟随视角");
        }
        StartCoroutine(CameraSmooth());
    }

    IEnumerator CameraSmooth()
    {
        isSmoothActive = false;
        yield return new WaitForSeconds(0.5f);
        isSmoothActive = true;
    }

    public enum CameraMode
    {
        Follow,
        Free,
        Stay
    }
}

