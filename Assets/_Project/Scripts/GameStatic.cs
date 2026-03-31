using UnityEngine;
using System;

public static class GameStatic
{

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void DataLoad()
    {
        DataGlobalSystem.Load();
        DataSettingSystem.Load();
        DataArchivesSystem.LoadAll();

    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void GameEntry()
    {
        if (!DataGlobalSystem.Get().hasEnteredGame)
        {
            // 打开新的游戏
            InputManager.Instance.EnableAllInput();
            UIManager.Instance.Push("StartPanel");
        }
    }

}