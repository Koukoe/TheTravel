using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 水面反射效果，挂载在水体上，每帧计算反射相机的位置
public class WaterReflect : MonoBehaviour
{
    
    public Camera mainCamera;
    public Camera reflectionCamera;

    // 进入视野，启动反射相机
    private void OnBecameVisible()
    {
        if (reflectionCamera != null)
            reflectionCamera.gameObject.SetActive(true);
    }

    // 离开视野，禁用反射相机
    private void OnBecameInvisible()
    {
        if (reflectionCamera != null)
            reflectionCamera.gameObject.SetActive(false);
    }

    // 同步反射相机位置
    private void LateUpdate()
    {
        // 获取水面平面参数(点和法线)
        Vector3 planePoint = transform.position;
        Vector3 planeNormal = transform.up;

        // 1. 反射相机位置
        Vector3 mainPos = mainCamera.transform.position;
        float distance = Vector3.Dot(mainPos - planePoint, planeNormal);
        reflectionCamera.transform.position = mainPos - 2f * distance * planeNormal;

        // 2. 反射相机的朝向
        Vector3 forward = mainCamera.transform.forward;
        Vector3 up = mainCamera.transform.up;
        Vector3 reflectedForward = Vector3.Reflect(forward, planeNormal);
        Vector3 reflectedUp = Vector3.Reflect(up, planeNormal);
        reflectionCamera.transform.rotation = Quaternion.LookRotation(reflectedForward, reflectedUp);
    }
}
