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
    public Dictionary<string, (int, bool)> TaskNodesDic;
    public bool startFinish;

    public DataArchive()
    {
        saveTime = "----/--/--";
        currentScene = "Ocean";  // 海
        states = new Dictionary<string, BaseState>();
        TaskNodesDic = new Dictionary<string, (int, bool)>();
        startFinish = false;
    }


    /// <summary>
    /// 统一获取状态的方法
    /// </summary>
    public T GetState<T>(string id) where T : BaseState, new()
    {
        if (states.TryGetValue(id, out BaseState state))
        {
            if (state is T target) return target;

            // 兼容：新旧都是 RealSceneState 家族时，迁移基类字段（如 targetPortalGuid）
            if (state is RealSceneState && typeof(T).IsSubclassOf(typeof(RealSceneState)))
            {
                T newState = new T();
                newState.Init(id);

                newState.Copyfrom(state);

                states[id] = newState;
                return newState;
            }

            // 类型不匹配：不删除旧状态，返回临时实例
            // （避免同 guid 同时被 ActorState 和 DialogueState 使用时互相覆盖）
            Debug.LogWarning($"[DataArchive] 类型不匹配: id={id}, 已有={state.GetType().Name}, 请求={typeof(T).Name}，返回临时实例");
            T freshState = new T();
            freshState.Init(id);
            return freshState;
        }

        T newFreshState = new T();
        newFreshState.Init(id);
        states[id] = newFreshState;
        return newFreshState;
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
        masterVolumeIndex = 2;
        musicVolumeIndex = 2;
        sfxVolumeIndex = 2;
        ambVolumeIndex = 2;
        resolutionIndex = 1; // 默认 1080p
        isFullScreen = 1;
        sensitivityIndex = 2;
    }

    public static bool IsDataSettingSame(DataSetting a, DataSetting b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        return a.sensitivityIndex == b.sensitivityIndex &&
               a.masterVolumeIndex == b.masterVolumeIndex &&
               a.musicVolumeIndex == b.musicVolumeIndex &&
               a.sfxVolumeIndex == b.sfxVolumeIndex &&
               a.ambVolumeIndex == b.ambVolumeIndex &&
               a.resolutionIndex == b.resolutionIndex &&
               a.isFullScreen == b.isFullScreen;
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

