using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MainPanel : MenuPanel
{
    public static readonly UIType uiType = new UIType("MainPanel");
    public MainPanel() : base(uiType) { }

    protected override void Init()
    {
        // 绑定按钮
        Button settingsBtn = UIMethods.GetInstance().FindObjectInChild<Button>(ActiveObj, "Settings");
        settingsBtn?.onClick.AddListener(() =>
        {
            UIManager.GetInstance().Push("SettingsPanel");
        });

        Button backBtn = UIMethods.GetInstance().FindObjectInChild<Button>(ActiveObj, "Back");
        backBtn?.onClick.AddListener(() =>
        {
            UIManager.GetInstance().Pop(false);
        });
    }

    protected override GameObject GetFirstSelectable()
    {
        return UIMethods.GetInstance().FindObjectInChild<Button>(ActiveObj, "Settings").gameObject;
    }
}