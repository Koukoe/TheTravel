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

    public float sensitivity;

    public float masterVolume = 0.75f;
    public float musicVolume = 0.75f;
    public float sfxVolume = 0.75f;
    public float ambVolume = 0.75f;

    public int resolutionIndex = 0;  // 分辨率索引
    public bool isFullScreen = true;
    public int qualityLevel = 2;
    public bool vSync = true;

    public DataSetting()
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

