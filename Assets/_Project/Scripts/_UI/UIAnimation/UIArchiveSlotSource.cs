using UnityEngine;

public class UIArchiveSlotSource : MonoBehaviour, IUIAppearanceSource
{
    [SerializeField] private bool isProvider = true;
    public bool IsProvider => isProvider;

    public Vector3 PosOffset => currentOffset;
    public Vector3 AngleOffset => Vector3.zero;
    public Vector3 ScaleMult => Vector3.one;
    public float AlphaMult => currentAlpha;

    private Vector3 currentOffset = Vector3.zero;
    private float currentAlpha = 1f;

    // SmoothDamp 速度缓冲变量
    private Vector3 posVelocity;
    private float alphaVelocity;

    public void SetTarget(Vector3 targetOffset, float targetAlpha, float smoothTime)
    {
        // 使用 SmoothDamp 模拟物理阻尼效果
        currentOffset = Vector3.SmoothDamp(
            currentOffset,
            targetOffset,
            ref posVelocity,
            smoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );

        currentAlpha = Mathf.SmoothDamp(
            currentAlpha,
            targetAlpha,
            ref alphaVelocity,
            smoothTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );
    }

    /// <summary>
    /// 当存档列表刷新或关闭重开时，重置速度缓存，防止出现“弹射”瞬间
    /// </summary>
    private void OnDisable()
    {
        posVelocity = Vector3.zero;
        alphaVelocity = 0f;
    }
}