using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

[RequireComponent(typeof(CanvasGroup))]
public abstract class UIButtonState : MonoBehaviour, ISelectHandler, IDeselectHandler, IUIAppearanceSource, ISubmitHandler
{
    [SerializeField] private bool isProvider = false;
    public bool IsProvider => isProvider;

    [Header("Disabled")]
    [Range(0, 1)] public float disabledAlpha = 0.6f;
    [SerializeField] protected Selectable selectable;
    [SerializeField] protected CanvasGroup canvasGroup;
    private float currentAlphaMult = 1f;
    [SerializeField] protected string SubmitSFXName = "UI_sound";
    [SerializeField] protected string SelectSFXName = "Select_button_UI";

    public virtual Vector3 PosOffset => Vector3.zero;
    public virtual Vector3 AngleOffset => Vector3.zero;
    public virtual Vector3 ScaleMult => Vector3.one;
    public float AlphaMult => currentAlphaMult;

    void Awake()
    {
        if (selectable == null) selectable = GetComponent<Selectable>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }
    void Start() => Init();

    public virtual void Init()
    {
        currentAlphaMult = 1f;
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

        currentAlphaMult = isReady ? 1f : disabledAlpha;
        if (!isProvider) canvasGroup.alpha = currentAlphaMult;

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

            ResetAppearance();
        }
    }

    public virtual void OnSelect(BaseEventData eventData)
    {
        if (selectable != null && !selectable.interactable) return;
        AudioManager.Instance.PlaySFX("Select_button_UI");
    }

    public virtual void OnDeselect(BaseEventData eventData) { }

    protected abstract void ResetAppearance();

    public virtual void OnSubmit(BaseEventData eventData)
    {
        if (selectable != null && !selectable.interactable) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("UI_sound");
        }
    }

    protected virtual void OnDisable() { ResetAppearance(); }
}