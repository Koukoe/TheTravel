using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Camera cam;
    public Transform camPos;
    public Transform target;
    public CameraMode currentMode;
    private float CamY = StaticDefination.CameraY;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        camPos = transform;
        if (target == null)
        {
            target = GameObject.FindWithTag("Player").transform;
        }
        if (target == null)
        {
            Debug.Log("no player Camera will Stay");
            currentMode = CameraMode.Stay;
        }
    }

    private Vector3 calPos()
    {
        Vector3 res;
        Vector3 forward = camPos.forward;
        res = target.position + (-forward) * CamY / forward.y;
        return res;
    }

    private void cameraFollow()
    {
        camPos.position = calPos();
    }

    // Update is called once per frame
    void Update()
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
};
