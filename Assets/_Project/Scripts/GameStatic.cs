using UnityEngine;
using System;

public static class GameStatic
{

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void DataLoad()
    {
        GlobalDataSystem.Load();
        SettingDataSystem.Load();
        ArchivesSystem.LoadAll();

    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void GameEntry()
    {
        if (!GlobalDataSystem.Get().hasEnteredGame)
        {
            // 打开新的游戏
            InputManager.Instance.EnableAllInput();
            UIManager.Instance.Push("StartPanel");
        }
    }

}