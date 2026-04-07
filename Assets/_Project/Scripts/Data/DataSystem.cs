using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class DataArchivesSystem
{
    private const int MAX_SLOTS = 9;
    private static List<DataArchives> _archives = new();

    /// <summary>
    /// 加载数据文件 ( F -> _ )
    /// </summary>
    public static void LoadAll()
    {
        _archives.Clear();
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            DataArchives data = DataPersistence.LoadData<DataArchives>($"slot_{i}.dat");
            _archives.Add(data);
        }
    }

    /// <summary>
    /// 保存/覆盖指定索引槽位的数据 ( M -> _ -> F )
    /// </summary>
    public static void Save(int index)
    {
        if (index < 0 || index >= _archives.Count) return;
        _archives[index].saveTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
        DataPersistence.SaveData($"slot_{index}.dat", _archives[index]);
    }

    /// <summary>
    /// 检查指定索引的槽位是否已经有数据 ( _ -> M )
    /// </summary>
    public static bool IsSlotOccupied(int index)
    {
        if (index < 0 || index >= _archives.Count) return false;
        return _archives[index].saveTime != "----/--/--";
    }

    /// <summary>
    /// 删除指定索引槽位的数据 ( _ & F )
    /// </summary>
    public static void Delete(int index)
    {
        if (index < 0 || index >= _archives.Count) return;
        string fileName = $"slot_{index}.dat";

        DataPersistence.DeleteData(fileName);
        _archives[index] = new DataArchives();

        Debug.Log($"[Archives] 槽位 {index} 已重置为默认状态");
    }

    /// <summary>
    /// 获取指定槽位的数据 ( _ -> M )
    /// </summary>
    public static DataArchives Get(int index)
    {
        if (index < 0 || index >= _archives.Count) return null;
        return _archives[index];
    }

    /// <summary>
    /// 获取所有存档数据 ( _ -> M )
    /// </summary>
    public static List<DataArchives> GetAll() => _archives;

    /// <summary>
    /// 获取时间戳最晚的存档数据 ( _ -> M )
    /// </summary>
    public static DataArchives GetLatest()
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
        return Get(latestIndex);
    }
}

public static class DataSettingSystem
{
    private static DataSetting _current;
    private static DataSetting _temp;

    /// <summary>
    /// 保存设置数据 ( _ -> F )
    /// </summary>
    public static void Save()
    {
        DataPersistence.SaveData("settings.dat", _current);
    }

    /// <summary>
    /// 加载设置数据并同步 ( F -> _ )
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
    /// 获取当前设置数据 ( _ -> M )
    /// </summary>
    public static DataSetting Get() => _current;

    /// <summary>
    /// 设定当前设置数据 ( M -> _ )
    /// </summary>
    public static void Set(DataSetting d) => _current = d;

    /// <summary>
    /// 恢复默认设置数据 ( _ & M )
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
    /// 保存全局数据 ( _ -> F )
    /// </summary>
    public static void Save()
    {
        DataPersistence.SaveData("global.dat", _current);
    }

    /// <summary>
    /// 加载全局数据 (F -> _ )
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
    /// 获取当前全局数据 ( _ -> M )
    /// </summary>
    public static DataGlobal Get() => _current;

    /// <summary>
    /// 设定当前全局数据 ( M -> _ )
    /// </summary>
    public static void Set(DataGlobal d) => _current = d;
}