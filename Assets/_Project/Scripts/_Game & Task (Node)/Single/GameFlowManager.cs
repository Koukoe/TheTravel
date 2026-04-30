using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public DataArchive PlayingData { get; private set; }

    public void NewGame()
    {
        PlayingData = new DataArchive();
    }
    public void LoadGame(int slotIndex)
    {
        var data = DataArchivesSystem.Get(slotIndex);
        if (data == null)
        {
            Debug.LogError("存档为空！");
            return;
        }

        PlayingData = data;
        PlayingData = DataArchivesSystem.Get(slotIndex);

        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.LoadMain(PlayingData.currentScene);  // 异步
        }

        //...
    }


    /// <summary>
    /// 存档点触发
    /// </summary>
    public void OnCheckPoint(int slotIndex)
    {
        DataArchivesSystem.Set(slotIndex, PlayingData);
    }
}
