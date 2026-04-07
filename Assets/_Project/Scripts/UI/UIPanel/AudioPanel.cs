using UnityEngine;
using UnityEngine.UI;

public class AudioPanel : SettingSubPanel
{
    [SerializeField] private UIOptionToggle masterVolToggle;

    public override void RefreshUI()
    {
        // 读数据并同步 UI
        masterVolToggle.SetIndex(Temp.masterVolumeIndex, false);
    }

    public void OnMasterVolChanged(int index) => Temp.masterVolumeIndex = index;
}