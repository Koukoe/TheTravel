using UnityEngine;
using System;

public static class GameRoot
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void GameEntry()
    {
        if (!DataSystem.Exists("settings.dat"))
        {
            // 打开新的游戏
        }

        ArchivesSystem.LoadAll();

    }

}