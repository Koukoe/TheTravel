using UnityEngine;

public class ControlPanel : SettingSubPanel
{
    [SerializeField] private UIOptionToggle sensitivityToggle;

    public override void RefreshUI()
    {
        sensitivityToggle.SetIndex(Temp.sensitivityIndex, false);
    }

    public void OnSensitivityChanged(int index)
    {
        Temp.sensitivityIndex = index;
    }
}