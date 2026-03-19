using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMethods
{
    private static UIMethods instance;
    public static UIMethods GetInstance()
    {
        if (instance == null)
        {
            instance = new UIMethods();
        }
        return instance;
    }
    /// <summary>
    /// 获得场景中的Canvas
    /// </summary>
    /// <returns></returns>
    public GameObject FindCanvas()
    {
        GameObject gameObject = GameObject.FindObjectOfType<Canvas>().gameObject;
        if(gameObject == null)
        {
            Debug.LogError("未找到Canvas!");
            return null;
        }
        return gameObject;

    }

    public GameObject FindObjectInChild(GameObject panel, string child_name)
    {
        Transform[] transforms = panel.GetComponentsInChildren<Transform>();
        foreach (var tra in transforms)
        {
            if (tra.gameObject.name == child_name)
            {
                return tra.gameObject;
            }
        }
        Debug.LogWarning($"在 {panel.name} 中未找到 {child_name}!");
        return null;
    }
}