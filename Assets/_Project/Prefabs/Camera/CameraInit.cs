using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInit : MonoBehaviour
{
    [SerializeField]
    public Vector3 cameraInitPos=new Vector3(new Vector2,StaticDefination.CameraZ);
    
    public Camera cam;
    public Transform transform;
    private Vector3 CamRot;
    // Start is called before the first frame update
    void Start()
    {
        CamRot = Quaternion.Euler(StaticDefination.cameraInitRot);
        cam=GetComponent<Camera>();
        transform=GetComponent<Transform>();
        transform.position = cameraInitPos;
        transform.rotation = cam;
    }
}
