using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    [SerializeField]
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
        TaskManager.Instance.LoadAllTaskNodes();

        if (GameSceneManager.Instance != null)
        {
            await GameSceneManager.Instance.LoadMain(PlayingData.currentScene);
        }

        // ...
    }

    public async UniTask<Texture2D> SaveGame(int slotIndex)
    {
        // 保存当前位置
        GameSceneManager.Instance.currentMainLogic.SyncPlayerPosition();

        // 截图并保存到本地
        string fileName = $"thumb_{slotIndex}.jpg";
        Texture2D newThumb = await CameraUtils.CaptureAndSaveAsync(Camera.main, fileName);

        // 保险
        TaskManager.Instance.SaveAllTaskNodes();

        // 更新时间信息
        PlayingData.saveTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm");

        DataArchivesSystem.Set(slotIndex, PlayingData);

        return newThumb;
    }

    /// <summary>
    /// 存档点触发
    /// </summary>
    public async UniTaskVoid OnCheckPoint()
    {
        Texture2D thumb = await SaveGame(0);

        if (thumb != null)
        {
            Destroy(thumb);
        }

        Debug.Log($"自动存档");
    }
}
