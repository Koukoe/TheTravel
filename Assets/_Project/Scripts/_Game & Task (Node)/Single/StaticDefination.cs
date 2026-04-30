using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticDefination
{
    [SerializeField]
    public static Vector3 CameraPos = new Vector3(0, 10, 0);//定义相机位置
    public static Vector3 CameraRot = new Vector3(45, -45, 0);//定义相机旋转
    public static float CameraY = 50;//定义相机Y轴高度差
}
