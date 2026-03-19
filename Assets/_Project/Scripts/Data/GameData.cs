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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void LoadAllArchives()
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
    public static void SaveArchiveData(int index)
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
    public static void DeleteArchiveData(int index)
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
    public static ArchivesData GetArchiveData(int index)
    {
        if (index < 0 || index >= _archives.Count) return null;
        return _archives[index];
    }

    /// <summary>
    /// 获取所有存档数据
    /// </summary>
    public static List<ArchivesData> GetAllArchives() => _archives;

    /// <summary>
    /// 获取时间戳最晚的存档数据
    /// </summary>
    public static ArchivesData GetLatestArchive()
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
        return GetArchiveData(latestIndex);
    }
}