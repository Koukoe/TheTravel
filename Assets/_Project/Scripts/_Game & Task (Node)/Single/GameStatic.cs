using UnityEngine;
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

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
    public static async Task GameEntry()
    {
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.ApplySettings(DataSettingSystem.Get());
        }
        if (!DataGlobalSystem.Get().hasEnteredGame)
        {
            GameFlowManager.Instance.NewGame().Forget();
        }
        else
        {
            await GameFlowManager.Instance.LoadGame(DataArchivesSystem.GetLatestIndex());
            InputManager.Instance.SwitchPlayerMode();
        }
    }

}