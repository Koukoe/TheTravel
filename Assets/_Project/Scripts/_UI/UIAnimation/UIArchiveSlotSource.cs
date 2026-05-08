using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIArchiveSlotSource : MonoBehaviour, IUIAppearanceSource
{
    [SerializeField] private bool isProvider = true;

    [SerializeField] public RawImage screenshotPreview;
    [SerializeField] public TextMeshProUGUI infoText;

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

    private void OnDisable()
    {
        // 面板隐藏时重置速度缓存，防止下次打开时 UI 乱飞
        posVelocity = Vector3.zero;
        alphaVelocity = 0f;
    }

    public void RefreshDisplay(Texture2D tex, string description)
    {
        // 设置图片
        if (screenshotPreview != null)
        {
            screenshotPreview.texture = tex;
            screenshotPreview.color = tex != null ? Color.white : Color.clear;
        }

        // 设置文本
        if (infoText != null)
        {
            infoText.text = string.IsNullOrEmpty(description) ? "请输入文本" : description;
        }
    }
}