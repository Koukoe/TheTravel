using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public static class DataPath
{
    private const string FOLDER_NAME = ".userdata";

    public static string GetRoot()
    {
        string baseDir = Directory.GetParent(Application.dataPath).FullName;
        string subPath = Application.isEditor ? "Temp" : "";
        string finalPath = Path.Combine(baseDir, subPath, FOLDER_NAME);

        // 动态创建
        if (!Directory.Exists(finalPath))
        {
            DirectoryInfo di = Directory.CreateDirectory(finalPath);  // 创建文件夹
            // 打包后的版本执行系统隐藏
            /*
            if (!Application.isEditor && Application.platform == RuntimePlatform.WindowsPlayer)
             {
                 di.Attributes = FileAttributes.Directory | FileAttributes.Hidden | FileAttributes.System;
            }
            */
        }

        return finalPath;
    }
}

public static class DataSystem
{
    public static void SaveData<T>(string fileName, T data)
    {
        string j = JsonUtility.ToJson(data, true);
        string p = Path.Combine(DataPath.GetRoot(), fileName);

        File.WriteAllText(p, j);
    }

    public static T LoadData<T>(string fileName) where T : new()
    {
        string p = Path.Combine(DataPath.GetRoot(), fileName);

        if (!File.Exists(p))
        {
            Debug.LogWarning($"[DataSystem] 未找到数据: {fileName}");
            return new T();
        }

        string j = File.ReadAllText(p);
        T data = JsonUtility.FromJson<T>(j);
        return data;
    }

    public static void DeleteData(string fileName)
    {
        string p = Path.Combine(DataPath.GetRoot(), fileName);

        if (File.Exists(p))
        {
            File.Delete(p);
            Debug.Log($"<color=red>[DataSystem]</color> 数据已删除: {fileName}");
        }
        else
        {
            Debug.LogWarning($"[DataSystem] 未找到数据: {fileName}");
        }
    }
}



