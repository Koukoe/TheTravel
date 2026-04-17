using System;
using System.Collections.Generic;
using UnityEngine;

// 使用类便于 json 储存

[Serializable]
public class DataArchives
{
    public string saveTime;

    public DataArchives()
    {
        saveTime = "----/--/--";
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

