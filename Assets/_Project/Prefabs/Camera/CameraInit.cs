using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInit : MonoBehaviour
{
    [SerializeField]
    public Vector3 cameraInitPos = new Vector3(0, 0, StaticDefination.CameraZ);

    public Camera cam;
    public Transform trans;
    private Quaternion CamRot;
    // Start is called before the first frame update
    void Start()
    {
        CamRot = Quaternion.Euler(StaticDefination.CameraRot);
        cam = GetComponent<Camera>();
        trans = GetComponent<Transform>();
        trans.position = cameraInitPos;
        trans.rotation = CamRot;
    }
}
