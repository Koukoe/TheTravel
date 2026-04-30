using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

// 使用类便于 json 储存

[Serializable]
public class DataArchive
{
    public string saveTime;
    public string currentScene;
    public Dictionary<string, BaseState> states;

    public DataArchive()
    {
        saveTime = "----/--/--";
        currentScene = "";  // 海
        states = new Dictionary<string, BaseState>();
    }


    /// <summary>
    /// 统一获取状态的方法
    /// </summary>
    public T GetState<T>(string id) where T : BaseState, new()
    {
        if (states.TryGetValue(id, out BaseState state))
        {
            if (state is T target) return target;
            states.Remove(id);
            Debug.LogWarning($"Key {id} 类型转换失败，覆盖新实例");
        }

        T newState = new T();
        newState.Init(id);
        states[id] = newState;
        return newState;
    }
}



[Serializable]
public class DataSetting
{
    public string language;

    public int sensitivityIndex = 2;

    public int masterVolumeIndex = 2;
    public int musicVolumeIndex = 2;
    public int sfxVolumeIndex = 2;
    public int ambVolumeIndex = 2;

    public int resolutionIndex = 1;  // 分辨率索引
    public int isFullScreen = 1;
    public int qualityLevel = 2;
    public bool vSync = true;

    public DataSetting()
    {
        // SystemLanguage sysLang = UnityEngine.Application.systemLanguage;

        // if (sysLang == SystemLanguage.Chinese || sysLang == SystemLanguage.ChineseSimplified)
        //     language = "zh-CN";
        // else if (sysLang == SystemLanguage.ChineseTraditional)
        //     language = "zh-TW";
        // else
        //     language = "en-US";
        Resolution current = Screen.currentResolution;
        masterVolumeIndex = 2;
        musicVolumeIndex = 2;
        sfxVolumeIndex = 2;
        ambVolumeIndex = 2;
        resolutionIndex = 1; // 默认 1080p
        isFullScreen = 1;
    }
}



[Serializable]
public class DataGlobal
{
    public bool hasEnteredGame;
    public int clearCount;
    public List<string> unlockedIds;
    public bool isTrueEndingReached;
    public DataGlobal()
    {
        hasEnteredGame = false;
        clearCount = 0;
    }
}

