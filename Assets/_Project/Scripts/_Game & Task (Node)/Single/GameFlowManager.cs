using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }
    private const string DialogueBubblePanelName = "DialogueBubblePanel";

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField]
    public DataArchive PlayingData { get; private set; }

    public void NewGame()
    {
        Debug.Log("是新游戏哦");
        PlayingData = new DataArchive();

        PlayerController.Instance.detector.ResetDetector();  // 清理 Detector

        InputManager.Instance.SwitchAllMode();
        GameSceneManager.Instance.LoadMain("Ocean").Forget();
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

        await CloseDialogueRelatedPanelsBeforeLoad();

        PlayingData = data;
        TaskManager.Instance.LoadAllTaskNodes();

        if (GameSceneManager.Instance != null)
        {
            await GameSceneManager.Instance.LoadMain(PlayingData.currentScene);
        }

        // ...
    }

    /// <summary>
    /// 加载存档前关闭对话相关面板
    /// </summary>
    private async UniTask CloseDialogueRelatedPanelsBeforeLoad()
    {
        UIManager uiManager = UIManager.Instance;
        if (uiManager == null)
        {
            return;
        }

        // DialogueBubblePanel 使用 Show/Hide 生命周期, 单独按名称隐藏
        uiManager.Hide(DialogueBubblePanelName);

        while (uiManager.IsTransitioning)
        {
            await UniTask.Yield();
        }

        Stack<BasePanel> stack = uiManager._singleStack;
        if (stack == null || stack.Count == 0)
        {
            return;
        }

        // 重建整个栈并剔除对话相关面板
        BasePanel[] snapshot = stack.ToArray(); // top -> bottom
        bool removedTopDialoguePanel = snapshot[0] is DialoguePanel || snapshot[0] is DialogueOptionsPanel;

        stack.Clear();

        for (int i = snapshot.Length - 1; i >= 0; i--)
        {
            BasePanel panel = snapshot[i];
            if (panel == null)
            {
                continue;
            }

            bool isDialoguePanel = panel is DialoguePanel || panel is DialogueOptionsPanel;
            if (isDialoguePanel)
            {
                panel.Abort(true);
                panel.gameObject.SetActive(false);
                PoolManager.Release(panel.gameObject);
                continue;
            }

            stack.Push(panel);
        }

        // 如果原栈顶被移除，需要恢复新的栈顶面板
        if (removedTopDialoguePanel && stack.Count > 0)
        {
            stack.Peek().Resume();
        }

        while (uiManager.IsTransitioning)
        {
            await UniTask.Yield();
        }
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
