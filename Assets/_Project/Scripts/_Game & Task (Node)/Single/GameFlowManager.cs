using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public DataArchive PlayingData { get; private set; }

    public async UniTask NewGame()
    {
        Debug.Log("是新游戏哦");
        PlayingData = new DataArchive();

        // ... 布置背景

        InputManager.Instance.SwitchAllMode();
        await GameSceneManager.Instance.LoadMain("Ocean");
        UIManager.Instance.Push("StartPanel");
    }
    public async UniTask LoadGame(int slotIndex)
    {
        Debug.Log("不是新游戏哦");
        var data = DataArchivesSystem.Get(slotIndex);
        if (data == null)
        {
            Debug.LogError("存档为空！");
            return;
        }

        PlayingData = data;
        PlayingData = DataArchivesSystem.Get(slotIndex);
        TaskManager.Instance.LoadAllTaskNodes();

        if (GameSceneManager.Instance != null)
        {
            await GameSceneManager.Instance.LoadMain(PlayingData.currentScene);
        }

        // ...
    }

    public void SaveGame(int slotIndex)
    {
        TaskManager.Instance.SaveAllTaskNodes();  // 保险
        DataArchivesSystem.Set(slotIndex, PlayingData);
    }



    /// <summary>
    /// 存档点触发
    /// </summary>
    public void OnCheckPoint(int slotIndex)
    {
        DataArchivesSystem.Set(slotIndex, PlayingData);
    }
}
