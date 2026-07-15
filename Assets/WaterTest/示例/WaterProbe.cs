using System.Collections.Generic;
using UnityEngine;

// 浮力组件，挂载在需要受浮力影响的物体上，通过探测点计算浮力方向和大小
public class WaterProbe : MonoBehaviour
{
    public WaterController waterController;
    public List<Transform> probePoints;
    private Rigidbody _rb;

    [Header("浮力倍率")]
    public float mp = 5;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        Vector3 totalForce = Vector3.zero; // 总浮力
        Vector3 buoyancyCenter = Vector3.zero; // 浮力中心
        float totalBuoyancy = 0f; // 总浮力大小

        foreach (Transform probe in probePoints)
        {
            // 水波高度，低于水面则获得浮力
            float waterHeight = waterController.GetWaveHeight(
                new Vector2(probe.position.x, probe.position.z)
            );
            float dive = waterHeight - probe.position.y;
            if (dive > 0f)
            {
                float forceMag = dive * mp;
                totalBuoyancy += forceMag;
                totalForce += Vector3.up * forceMag;
                buoyancyCenter += probe.position * forceMag;
            }
        }

        // 施加浮力
        if (totalBuoyancy > 0f)
        {
            buoyancyCenter /= totalBuoyancy;
            _rb.AddForceAtPosition(totalForce, buoyancyCenter, ForceMode.Force);
        }
    }
}
