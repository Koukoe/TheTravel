using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFlow : MonoBehaviour
{
    [Header("随机范围")]
    [Range(0, 10)]
    public float maxRot = 10f;
    [Range(0, 2)]
    public float maxGap = 1f;
    [Range(5, 10)]
    public float maxDuringTime = 5f;
    [Header("初始角度")]
    public Vector3 initRot = new Vector3(45, 45, 0);
    private Transform cam;
    private float duringTime;
    private float gap;
    private float rot;
    private float speed;
    private bool isRotating = false;
    private bool canFlow = false;//判断是否在间歇时间
    private float gapTime = 0f;
    void Start()
    {
        cam = transform;
        canFlow = true;
    }
    void Update()
    {
        if (!isRotating && canFlow)
        {
            flowInit();
            StartCoroutine(flowCoroutine());
        }
        else if (!canFlow)
        {
            gapTime += Time.deltaTime;
            if (gapTime > gap)
            {
                canFlow = true;
                gapTime = 0f;
            }
        }
    }
    private void flowInit()
    {
        rot = UnityEngine.Random.Range(0, maxRot);
        duringTime = UnityEngine.Random.Range(1f, maxDuringTime);
        speed = 2 * Mathf.PI / duringTime;
        isRotating = true;
    }
    IEnumerator flowCoroutine()
    {
        Vector3 rotVec = cam.eulerAngles;
        Vector3 DrotVec = new Vector3(0, 0, rot);
        float curTime = 0f;
        while (curTime <= duringTime)
        {
            curTime += Time.deltaTime;
            cam.eulerAngles = rotVec + DrotVec * Mathf.Sin(curTime * speed);
            yield return null;
        }
        cam.eulerAngles = rotVec;//防止误差
        isRotating = false;
        canFlow = false;
        gap = UnityEngine.Random.Range(0f, maxGap);//随机间隔时间
        yield break;
    }
}
