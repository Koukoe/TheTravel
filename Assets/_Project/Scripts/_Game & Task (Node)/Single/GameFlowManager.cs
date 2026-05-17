using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }
    private const string DialogueBubblePanelName = "DialogueBubblePanel";

    /// <summary>读档进行中，防止 OnCheckPoint auto-save 把残留任务状态写进 PlayingData</summary>
    private bool _isLoadingSaveGame = false;

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
        UIManager.Instance.PopAll();

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

        _isLoadingSaveGame = true;
        PlayingData = data;
        Debug.Log("[GameFlowManager] 开始读取存档，先重置所有任务运行状态");

        // 第 1 步：重置任务运行状态（取消运行中任务、清理连接、重置图标记）
        //     不销毁 TaskNode 对象（它们在 GlobalManager/T 层级中跨场景存活）
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.ClearAllTaskNodesForLoad();
        }

        // 第 2 步：加载目标场景（新场景的 TaskNode 会在 Awake 中注册到字典）
        if (GameSceneManager.Instance != null)
        {
            Debug.Log("[GameFlowManager] 开始加载场景: " + PlayingData.currentScene);
            await GameSceneManager.Instance.LoadMain(PlayingData.currentScene);
            Debug.Log("[GameFlowManager] 场景加载完成");
        }

        // 等待至少一帧，确保场景中所有 TaskNode 的 Awake 已执行完毕
        // （有些项目通过 Addressables / 动态实例化延迟创建 TaskNode，多等几帧更安全）
        await UniTask.Yield(PlayerLoopTiming.Update);
        await UniTask.Yield(PlayerLoopTiming.Update);
        Debug.Log("[GameFlowManager] 已等待两帧，开始重建任务图");

        // 第 3 步：场景加载完成后，从存档重建任务图（连接 + 恢复状态 + 启动就绪任务）
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.RebuildGraphFromSave();
        }

        _isLoadingSaveGame = false;

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

        // 1. 锁定任务状态到 PlayingData
        TaskManager.Instance.SaveAllTaskNodes();

        // 2. ★ 立刻存盘，中间无任何 yield/await，防止任务推进篡改 PlayingData
        PlayingData.saveTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
        DataArchivesSystem.Set(slotIndex, PlayingData);

        // 3. 截图（纯 UI 用途，不影响存档数据）
        string fileName = $"thumb_{slotIndex}.jpg";
        Texture2D newThumb = await CameraUtils.CaptureAndSaveAsync(Camera.main, fileName);

        return newThumb;
    }

    /// <summary>
    /// 存档点触发
    /// </summary>
    public async UniTaskVoid OnCheckPoint()
    {
        if (_isLoadingSaveGame)
        {
            Debug.Log("[GameFlowManager] 读档进行中，跳过自动存档");
            return;
        }

        Texture2D thumb = await SaveGame(0);

        if (thumb != null)
        {
            Destroy(thumb);
        }

        Debug.Log($"自动存档");
    }
}
