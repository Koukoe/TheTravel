using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

public static class DataArchivesSystem
{
    private const int MAX_SLOTS = 9;
    private static List<DataArchive> _archives = new();

    /// <summary>
    /// 从文件加载所有槽位的存档数据 ( F -> _ )
    /// </summary>
    public static void LoadAll()
    {
        _archives.Clear();
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            DataArchive data = DataPersistence.LoadData<DataArchive>($"slot_{i}.dat");
            _archives.Add(data);
        }
    }

    /// <summary>
    /// 保存指定槽位的数据到文件 ( _ -> F )
    /// </summary>
    public static void Save(int index)
    {
        if (index < 0 || index >= _archives.Count) return;
        _archives[index].saveTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
        DataPersistence.SaveData($"slot_{index}.dat", _archives[index]);
    }

    /// <summary>
    /// 检查指定槽位是否已被占用 ( _ -> M )
    /// </summary>
    public static bool IsSlotOccupied(int index)
    {
        if (index < 0 || index >= _archives.Count) return false;
        return _archives[index].saveTime != "----/--/--";
    }

    /// <summary>
    /// 删除指定槽位的存档数据（内存和文件） ( _ & F )
    /// </summary>
    public static void Delete(int index)
    {
        if (index < 0 || index >= _archives.Count) return;
        string fileName = $"slot_{index}.dat";

        DataPersistence.DeleteData(fileName);
        _archives[index] = new DataArchive();

        Debug.Log($"[Archives] 槽位 {index} 已重置为默认状态");
    }

    /// <summary>
    /// 获取指定槽位存档数据的深拷贝 ( _ -> M )
    /// </summary>
    public static DataArchive Get(int index)
    {
        if (index < 0 || index >= _archives.Count) return null;
        string json = JsonConvert.SerializeObject(_archives[index], DataPersistence.Settings);
        return JsonConvert.DeserializeObject<DataArchive>(json, DataPersistence.Settings);
    }

    /// <summary>
    /// 获取最新保存的存档索引 ( _ -> M )
    /// </summary>
    public static int GetLatestIndex()
    {
        int latestIndex = -1;
        DateTime latestTime = DateTime.MinValue;

        for (int i = 0; i < _archives.Count; i++)
        {
            if (!IsSlotOccupied(i)) continue;

            if (DateTime.TryParse(_archives[i].saveTime, out DateTime time))
            {
                if (time > latestTime)
                {
                    latestTime = time;
                    latestIndex = i;
                }
            }
        }
        return latestIndex;
    }

    /// <summary>
    /// 设置指定槽位的存档数据并自动保存到文件 ( M -> _ & F )
    /// </summary>
    public static void Set(int index, DataArchive d)
    {
        if (index < 0 || index >= _archives.Count || d == null) return;
        string json = JsonConvert.SerializeObject(d, DataPersistence.Settings);
        _archives[index] = JsonConvert.DeserializeObject<DataArchive>(json, DataPersistence.Settings);
        Save(index);
    }
}

public static class DataSettingSystem
{
    private static DataSetting _current;

    /// <summary>
    /// 保存当前设置数据到文件 ( _ -> F )
    /// </summary>
    public static void Save()
    {
        DataPersistence.SaveData("settings.dat", _current);
    }

    /// <summary>
    /// 从文件加载设置数据 ( F -> _ )
    /// </summary>
    public static void Load()
    {
        bool isFirstTime = !DataPersistence.Exists("settings.dat");
        _current = DataPersistence.LoadData<DataSetting>("settings.dat");

        if (isFirstTime)
        {
            Save();
        }
    }

    /// <summary>
    /// 获取当前设置数据的深拷贝 ( _ -> M )
    /// </summary>
    public static DataSetting Get()
    {
        string json = JsonConvert.SerializeObject(_current, DataPersistence.Settings);
        return JsonConvert.DeserializeObject<DataSetting>(json, DataPersistence.Settings);
    }

    /// <summary>
    /// 设置当前设置数据并自动保存到文件 ( M -> _ & F )
    /// </summary>
    public static void Set(DataSetting d)
    {
        if (d == null) return;
        string json = JsonConvert.SerializeObject(d, DataPersistence.Settings);
        _current = JsonConvert.DeserializeObject<DataSetting>(json, DataPersistence.Settings);
        Save();
    }

    /// <summary>
    /// 恢复默认设置数据并自动保存到文件 ( _ & F )
    /// </summary>
    public static void Reset()
    {
        _current = new DataSetting();
        Save();
        Debug.Log("<color=yellow>[Settings]</color> 已恢复默认设置。");
    }
}

public static class DataGlobalSystem
{
    private static DataGlobal _current;

    /// <summary>
    /// 保存全局数据到文件 ( _ -> F )
    /// </summary>
    public static void Save()
    {
        DataPersistence.SaveData("global.dat", _current);
    }

    /// <summary>
    /// 从文件加载全局数据 ( F -> _ )
    /// </summary>
    public static DataGlobal Load()
    {
        bool isFirstTime = !DataPersistence.Exists("global.dat");
        _current = DataPersistence.LoadData<DataGlobal>("global.dat");

        if (isFirstTime)
        {
            Save();
        }
        return _current;
    }

    /// <summary>
    /// 获取当前全局数据的深拷贝 ( _ -> M )
    /// </summary>
    public static DataGlobal Get()
    {
        if (_current == null) return null;
        string json = JsonConvert.SerializeObject(_current, DataPersistence.Settings);
        return JsonConvert.DeserializeObject<DataGlobal>(json, DataPersistence.Settings);
    }

    /// <summary>
    /// 设置当前全局数据并自动保存到文件 ( M -> _ & F )
    /// </summary>
    public static void Set(DataGlobal d)
    {
        if (d == null) return;
        string json = JsonConvert.SerializeObject(d, DataPersistence.Settings);
        _current = JsonConvert.DeserializeObject<DataGlobal>(json, DataPersistence.Settings);
        Save();
    }
}