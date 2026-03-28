using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ArchivesData
{
    public string saveTime;

    public ArchivesData()
    {
        saveTime = "----/--/--";
    }
}

public static class ArchivesSystem
{
    private const int MAX_SLOTS = 9;
    private static List<ArchivesData> _archives = new();

    public static void LoadAll()
    {
        _archives.Clear();
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            ArchivesData data = DataSystem.LoadData<ArchivesData>($"slot_{i}.dat");
            _archives.Add(data);
        }
    }

    /// <summary>
    /// 保存/覆盖指定索引槽位的数据
    /// </summary>
    public static void Save(int index)
    {
        if (index < 0 || index >= _archives.Count) return;
        _archives[index].saveTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
        DataSystem.SaveData($"slot_{index}.dat", _archives[index]);
    }

    /// <summary>
    /// 检查指定索引的槽位是否已经有数据
    /// </summary>
    public static bool IsSlotOccupied(int index)
    {
        if (index < 0 || index >= _archives.Count) return false;
        return _archives[index].saveTime != "----/--/--";
    }

    /// <summary>
    /// 删除指定索引槽位的数据
    /// </summary>
    public static void Delete(int index)
    {
        if (index < 0 || index >= _archives.Count) return;
        string fileName = $"slot_{index}.dat";

        DataSystem.DeleteData(fileName);
        _archives[index] = new ArchivesData();

        Debug.Log($"[Archives] 槽位 {index} 已重置为默认状态");
    }

    /// <summary>
    /// 获取指定槽位的数据
    /// </summary>
    public static ArchivesData Get(int index)
    {
        if (index < 0 || index >= _archives.Count) return null;
        return _archives[index];
    }

    /// <summary>
    /// 获取所有存档数据
    /// </summary>
    public static List<ArchivesData> GetAll() => _archives;

    /// <summary>
    /// 获取时间戳最晚的存档数据
    /// </summary>
    public static ArchivesData GetLatest()
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

[Serializable]
public class SettingData
{
    public string language;

    public float sensitivity;

    public float masterVolume = 0.75f;
    public float musicVolume = 0.75f;
    public float sfxVolume = 0.75f;
    public float ambVolume = 0.75f;

    public int resolutionIndex = 0;  // 分辨率索引
    public bool isFullScreen = true;
    public int qualityLevel = 2;
    public bool vSync = true;

    public SettingData()
    {
        SystemLanguage sysLang = UnityEngine.Application.systemLanguage;

        if (sysLang == SystemLanguage.Chinese || sysLang == SystemLanguage.ChineseSimplified)
            language = "zh-CN";
        else if (sysLang == SystemLanguage.ChineseTraditional)
            language = "zh-TW";
        else
            language = "en-US";
    }
}

public static class SettingDataSystem
{
    private static SettingData _current;

    /// <summary>
    /// 保存设置
    /// </summary>
    public static void Save()
    {
        DataSystem.SaveData("settings.dat", _current);
    }

    /// <summary>
    /// 加载设置
    /// </summary>
    public static SettingData Load()
    {
        bool isFirstTime = !DataSystem.Exists("settings.dat");
        _current = DataSystem.LoadData<SettingData>("settings.dat");

        if (isFirstTime)
        {
            Save();
        }
        return _current;
    }

    /// <summary>
    /// 恢复默认设置
    /// </summary>
    public static void Reset()
    {
        _current = new SettingData();
        Save();
        Debug.Log("<color=yellow>[Settings]</color> 已恢复默认设置。");
    }
}

[Serializable]
public class GlobalData
{
    public int clearCount;
    public List<string> unlockedIds;
    public bool isTrueEndingReached;
    public GlobalData()
    {

    }
}

public static class GlobalDataSystem
{
    private static GlobalData _current;

    /// <summary>
    /// 保存全局数据
    /// </summary>
    public static void Save()
    {
        DataSystem.SaveData("global.dat", _current);
    }

    /// <summary>
    /// 加载全局数据
    /// </summary>
    public static GlobalData Load()
    {
        bool isFirstTime = !DataSystem.Exists("global.dat");
        _current = DataSystem.LoadData<GlobalData>("global.dat");

        if (isFirstTime)
        {
            Save();
        }
        return _current;
    }
}