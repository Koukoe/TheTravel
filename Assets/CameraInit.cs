using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInit : MonoBehaviour
{
    [SerializeField]
    public Vector3 cameraInitPos;

    public Camera cam;
    public Transform transform;
    // Start is called before the first frame update
    void Start()
    {
        cam=GetComponent<Camera>();
        transform=GetComponent<Transform>();
        transform.position = cameraInitPos;
    }
}
