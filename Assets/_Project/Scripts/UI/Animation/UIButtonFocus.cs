using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UIButtonFocus : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public EaseParam scaleEase;
    public float duration = 0.2f;
    public float selectScale = 1.4f;

    private Vector3 initialScale;
    private Coroutine scaleRoutine;

    void Awake()
    {
        initialScale = Vector3.one;
        transform.localScale = initialScale;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(AnimateScale(initialScale * selectScale));
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(AnimateScale(initialScale));
    }

    private IEnumerator AnimateScale(Vector3 target)
    {
        float t = 0;
        Vector3 start = transform.localScale;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = scaleEase.Lerp(start, target, t / duration);
            yield return null;
        }
        transform.localScale = target;
    }
}