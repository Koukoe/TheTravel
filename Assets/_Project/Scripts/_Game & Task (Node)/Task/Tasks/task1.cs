using System.Collections;
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

    public override IEnumerator TaskIEnumerator()
    {
        if (cameraMove == null)
        {
            cameraMove = FindObjectOfType<CameraMove>();
            if (cameraMove == null)
            {
                Debug.LogError("task1: 场景中未找到 CameraMove 组件！");
            }
        }

        float startY = cameraMove.CamY;
        float elapsed = 0f;

        // 向上移动（放大效果）
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            cameraMove.CamY = Mathf.Lerp(startY, targetY, smoothT);
            yield return null;
        }
        cameraMove.CamY = targetY;

        // 等待对话
        yield return WaitForDialogue();

        // 向下移动（恢复）
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            cameraMove.CamY = Mathf.Lerp(targetY, startY, smoothT);
            yield return null;
        }
        cameraMove.CamY = startY;

        isDone = true;
    }

    private IEnumerator WaitForDialogue()
    {
        yield return null;
        yield return DialogueManager.Instance.StartWithAsync(dialogueText);
    }
}