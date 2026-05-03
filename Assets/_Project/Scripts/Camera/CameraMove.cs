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

        // 水平方向（相机后方）
        Vector3 horizontalDir = -camPos.forward;
        horizontalDir.y = 0;
        if (horizontalDir.magnitude < 0.001f) horizontalDir = Vector3.back;
        horizontalDir.Normalize();

        // 俯角（取正值）
        float pitchRad = Mathf.Asin(Mathf.Clamp(-camPos.forward.y, 0.01f, 0.99f));
        float horizontalDistance = CamY / Mathf.Tan(pitchRad);
        horizontalDistance = Mathf.Min(horizontalDistance, 50f);

        return target.position + horizontalDir * horizontalDistance + Vector3.up * CamY;
    }

    private void cameraFollow()
    {
        Vector3 targetPosition = calPos();
        Vector3 smoothPos = Vector3.Lerp(camPos.position, targetPosition, smoothSpeed);
        camPos.position = smoothPos;
    }

    private void cameraFree(Vector2 mouseMove)
    {

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
            Debug.Log("相机高度变为海上高度");
        }
        else
        {
            currentMode = CameraMode.Follow;
            CamY = StaticDefination.CameraY;
            Debug.Log("相机高度变为陆地高度");
        }
    }
}

public enum CameraMode
{
    Follow,
    Free,
    Stay
}