using Cysharp.Threading.Tasks;
using UnityEngine;

public class task1 : TaskBasic
{
    [Header("Camera 设置")]
    [SerializeField] private CameraMove cameraMove;

    [Header("对话设置")]
    [SerializeField] private TextAsset dialogueText;

    [Header("相机动画参数")]
    [SerializeField] private float targetY = 10f;
    [SerializeField] private float duration = 1f;

    private float _startY; // 用于记录初始高度

    protected override async UniTask OnTaskStart()
    {
        if (cameraMove == null)
        {
            cameraMove = FindObjectOfType<CameraMove>();
            if (cameraMove == null)
            {
                Debug.LogError("task1: 场景中未找到 CameraMove 组件！");
                return;
            }
        }

        _startY = cameraMove.CamY; // 记录 startY
        float elapsed = 0f;

        // 向上移动（放大效果）
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            cameraMove.CamY = Mathf.Lerp(_startY, targetY, smoothT);
            await UniTask.Yield();
        }
        cameraMove.CamY = targetY;

        // 等待对话
        await WaitForDialogue();

        FinishTask();
    }

    protected override void OnTaskEnd()
    {
        // 向下移动（恢复）
        MoveCameraDown().Forget();
    }

    private async UniTask MoveCameraDown()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            cameraMove.CamY = Mathf.Lerp(targetY, _startY, smoothT);
            await UniTask.Yield();
        }
        cameraMove.CamY = _startY;
    }

    protected virtual async UniTask WaitForDialogue()
    {
        await UniTask.WaitUntil(() => !UIManager.Instance.UISys() && !(UIManager.Instance.Peek() is BookPanel));
        await DialogueManager.Instance.StartWithAsyncUniTask(dialogueText);
    }
}