using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIType
{
    private string path;
    private string name;
    public string Path {get => path;}
    public string Name {get => name;}
    /// <summary>
    /// 获得UI信息
    /// </summary>
    /// <param name="path">对应Panel的路径</param>
    /// <param name="name">对应Panel的名称</param>
    public UIType(string path, string name)
    {
        this.path = path;
        this.name = name;
    }
}
