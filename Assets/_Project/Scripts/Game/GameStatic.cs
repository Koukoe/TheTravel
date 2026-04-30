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
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.ApplySettings(DataSettingSystem.Get());
        }
        if (!DataGlobalSystem.Get().hasEnteredGame)
        {
            MenuManager.Instance.NewGame();
        }
    }

}