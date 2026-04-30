using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class UIAppearanceApplier : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;

    private Vector3 targetPos;
    private Vector3 targetAngles;
    private readonly List<IUIAppearanceSource> sources = new List<IUIAppearanceSource>();

    private void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        targetPos = rectTransform.anchoredPosition3D;
        targetAngles = rectTransform.localEulerAngles;

        RefreshSources();
    }

    [ContextMenu("Refresh Sources")]
    public void RefreshSources()
    {
        sources.Clear();
        sources.AddRange(GetComponents<IUIAppearanceSource>());
    }

    private void LateUpdate()
    {
        if (sources.Count == 0) return;

        Vector3 totalOffset = Vector3.zero;
        Vector3 totalAngleOffset = Vector3.zero;
        Vector3 totalScale = Vector3.one;
        float totalAlpha = 1f;
        bool shouldHardHide = false;

        for (int i = 0; i < sources.Count; i++)
        {
            var s = sources[i];
            if (!s.IsProvider) continue;

            if (s.PosOffset.x == 14514f && s.PosOffset.y == 19810f)
            {
                shouldHardHide = true;
                break;  // 信号拦截
            }

            totalOffset += s.PosOffset;
            totalAngleOffset += s.AngleOffset;
            totalScale = Vector3.Scale(totalScale, s.ScaleMult);
            totalAlpha *= s.AlphaMult;
        }

        // 最终物理渲染
        if (shouldHardHide)
        {
            // 物理放逐
            rectTransform.anchoredPosition3D = new Vector3(14514, 19810, 0);
        }
        else
        {
            rectTransform.anchoredPosition3D = targetPos + totalOffset;
            rectTransform.localEulerAngles = targetAngles + totalAngleOffset;
            rectTransform.localScale = totalScale;

            if (canvasGroup != null) canvasGroup.alpha = totalAlpha;
        }
    }

    private void OnEnable() => RefreshSources();
}