using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class UIButtonState : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("Selected")]
    public EaseParam scaleEase;
    public float duration = 0.2f;
    public float selectScale = 1.4f;

    [Header("Disabled")]
    [Range(0, 1)] public float disabledAlpha = 0.6f;
    [SerializeField] private Selectable selectable;
    [SerializeField] private CanvasGroup canvasGroup;

    private Vector3 initialScale;
    private Coroutine scaleRoutine;

    void Awake()
    {
        if (selectable == null) selectable = GetComponent<Selectable>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        Init();
    }
    void Init()
    {
        initialScale = Vector3.one;
        transform.localScale = initialScale;

        SyncInteractable();
    }


    public void SetInteractable(bool state)
    {
        if (selectable != null) selectable.interactable = state;
        SyncInteractable();
    }

    public void SyncInteractable()
    {
        if (selectable == null || canvasGroup == null) return;

        bool isReady = selectable.interactable;
        canvasGroup.alpha = isReady ? 1f : disabledAlpha;
        // canvasGroup.blocksRaycasts = isReady;  这个项目没有鼠标对 UI 的操作所以其实是否禁用射线检测无所谓

        if (!isReady)
        {
            // 焦点自动跳转：先下后上
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                Selectable next = selectable.FindSelectableOnDown() ?? selectable.FindSelectableOnUp();
                if (next != null) next.Select();
                else EventSystem.current.SetSelectedGameObject(null);
            }

            if (scaleRoutine != null) StopCoroutine(scaleRoutine);
            transform.localScale = initialScale;
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (selectable != null && !selectable.interactable) return;

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
        Vector3 start = transform.localScale; // 抓取当前值，保证丝滑折返

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = scaleEase.Lerp(start, target, t / duration);
            yield return null;
        }
        transform.localScale = target;
    }
}