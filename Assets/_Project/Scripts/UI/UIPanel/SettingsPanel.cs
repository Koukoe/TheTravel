using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class SettingsPanel : MenuPanel
{
    public static readonly UIType uiType = new UIType("SettingsPanel");
    public SettingsPanel() : base(uiType) { }

    protected override void Init()
    {
        Button backBtn = UIMethods.GetInstance().FindObjectInChild<Button>(ActiveObj, "Back");
        backBtn?.onClick.AddListener(() =>
        {
            UIManager.GetInstance().Pop(false);
        });
    }

    protected override GameObject GetFirstSelectable()
    {
        return UIMethods.GetInstance().FindObjectInChild<Button>(ActiveObj, "Back").gameObject;
    }
}