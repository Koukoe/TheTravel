using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonStateSwitchImage : UIButtonState
{
    [SerializeField] private Image targetImage;
    public Sprite normalSprite;
    public Sprite selectedSprite;

    private Sprite currentSprite;

    public Sprite CurrentSprite => currentSprite;

    public override void Init()
    {
        if (targetImage == null && !IsProvider)
        {
            targetImage = GetComponent<Image>();
        }

        currentSprite = normalSprite;

        base.Init();
    }

    public override void OnSelect(BaseEventData eventData)
    {
        if (selectable != null && !selectable.interactable) return;

        base.OnSelect(eventData);
        UpdateImage(selectedSprite);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        UpdateImage(normalSprite);
    }

    protected override void ResetAppearance()
    {
        UpdateImage(normalSprite);
    }

    private void UpdateImage(Sprite targetSprite)
    {
        if (targetSprite == null) return;

        currentSprite = targetSprite;

        targetImage.sprite = targetSprite;

        // Provider 没用了
    }
}