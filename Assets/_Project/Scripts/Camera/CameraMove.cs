using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Camera cam;
    public Transform camPos;
    public Transform target;
    public CameraMode currentMode;
    public float smoothSpeed = 0.125f;
    private float CamY = StaticDefination.CameraY;

    void Start()
    {
        if (cam == null)
            cam = GetComponent<Camera>();
        camPos = transform;

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
        if (target == null) return camPos.position;

        // 水平方向（相机看向的反方向，忽略 Y 轴）
        Vector3 horizontalDir = -camPos.forward;
        horizontalDir.y = 0;

        if (horizontalDir.magnitude < 0.001f)
        {
            // 相机垂直向下时，使用默认方向
            horizontalDir = Vector3.back;
        }
        horizontalDir.Normalize();

        // 计算水平距离
        float forwardY = Mathf.Clamp(camPos.forward.y, -0.99f, 0.99f);  // 防止超出范围
        float sinPitch = forwardY;
        float cosPitch = Mathf.Sqrt(1 - sinPitch * sinPitch);

        float horizontalDistance;
        if (Mathf.Abs(sinPitch) < 0.001f)
        {
            // 相机接近水平时，使用较大的水平距离
            horizontalDistance = 100f;
        }
        else
        {
            // 水平距离 = 高度 / tanθ = 高度 × cosθ / sinθ
            horizontalDistance = CamY * cosPitch / sinPitch;
        }

        // 限制最大水平距离，避免相机飞太远
        horizontalDistance = Mathf.Min(horizontalDistance, 50f);

        return target.position + horizontalDir * horizontalDistance + Vector3.up * CamY;
    }

    private void cameraFollow()
    {
        Vector3 targetPosition = calPos();
        Vector3 smoothPos = Vector3.Lerp(camPos.position, targetPosition, smoothSpeed);
        camPos.position = smoothPos;

        // 让相机看向玩家
        camPos.LookAt(target);
    }

    void LateUpdate()
    {
        if (currentMode == CameraMode.Stay) return;
        if (currentMode == CameraMode.Follow)
        {
            cameraFollow();
        }
    }
}

public enum CameraMode
{
    Follow,
    Free,
    Stay
}