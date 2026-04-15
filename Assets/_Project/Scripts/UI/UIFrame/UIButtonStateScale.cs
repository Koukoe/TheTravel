using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UIButtonStateScale : UIButtonState
{
    [Header("Selected Animation")]
    public EaseParam scaleEase;
    public float duration = 0.2f;
    public float selectScale = 1.4f;

    private Vector3 initialScale;
    private Vector3 currentScaleMult = Vector3.one;
    private Coroutine scaleRoutine;

    public override Vector3 ScaleMult => currentScaleMult;

    public override void Init()
    {
        initialScale = transform.localScale;
        currentScaleMult = initialScale;

        base.Init();

        // 视觉复位
        if (!IsProvider)
        {
            transform.localScale = initialScale;
        }
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(AnimateScale(initialScale * selectScale));
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(AnimateScale(initialScale));
    }

    protected override void ResetAppearance()
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);

        currentScaleMult = initialScale;
        if (!IsProvider)
        {
            transform.localScale = initialScale;
        }
    }

    private IEnumerator AnimateScale(Vector3 target)
    {
        float t = 0;
        Vector3 start = IsProvider ? currentScaleMult : transform.localScale;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            currentScaleMult = scaleEase.Lerp(start, target, t / duration);

            if (!IsProvider) transform.localScale = currentScaleMult;
            yield return null;
        }

        currentScaleMult = target;
        if (!IsProvider)
        {
            transform.localScale = target;
        }
    }
}