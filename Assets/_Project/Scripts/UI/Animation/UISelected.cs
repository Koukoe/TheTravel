using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SimpleScaleFocus : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("放大倍率")]
    public float selectScale = 1.15f;
    [Header("动画时间")]
    public float duration = 0.2f;
    [Header("动画曲线")]
    public EaseParam scaleEase;

    private Vector3 initialScale;
    private Coroutine scaleRoutine;

    void Awake()
    {
        // 强制初始值为 1，防止任何意外导致的消失
        initialScale = Vector3.one;
        transform.localScale = initialScale;

        // 自动禁用鼠标干扰，确保纯键盘手感
        var selectable = GetComponent<Selectable>();
        if (selectable != null)
        {
            var nav = selectable.navigation;
            // 保持原本的导航逻辑，只关掉鼠标射线
            var images = GetComponentsInChildren<Image>();
            foreach (var img in images) img.raycastTarget = false;
            var texts = GetComponentsInChildren<Text>();
            foreach (var txt in texts) txt.raycastTarget = false;
        }
    }

    // --- 键盘选中：放大 ---
    public void OnSelect(BaseEventData eventData)
    {
        StopScale();
        scaleRoutine = StartCoroutine(AnimateScale(initialScale * selectScale));
    }

    // --- 键盘取消选中：缩小 ---
    public void OnDeselect(BaseEventData eventData)
    {
        StopScale();
        scaleRoutine = StartCoroutine(AnimateScale(initialScale));
    }

    private IEnumerator AnimateScale(Vector3 target)
    {
        float t = 0;
        Vector3 start = transform.localScale;

        while (t < duration)
        {
            t += Time.deltaTime;
            // 直接使用你提供的 EaseParam 工具
            transform.localScale = scaleEase.Lerp(start, target, t / duration);
            yield return null;
        }
        transform.localScale = target;
    }

    private void StopScale()
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
    }
}