using UnityEngine;
using System;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using System.Threading.Tasks;

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
            await GameFlowManager.Instance.NewGame();
        }
        else
        {
            await GameFlowManager.Instance.LoadGame(DataArchivesSystem.GetLatestIndex());
            MenuManager.Instance.Menu();
        }
    }

}