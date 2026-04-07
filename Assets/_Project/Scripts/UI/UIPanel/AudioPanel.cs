using UnityEngine;
using UnityEngine.UI;

public class AudioPanel : SettingSubPanel
{
    [SerializeField] private UIOptionToggle masterVolToggle;

    public override void RefreshUI()
    {
        // 读数据并同步 UI
        masterVolToggle.SetIndex(Mathf.RoundToInt(Temp.masterVolume / 0.25f), false);
    }

    public void OnMasterVolChanged(int index)
    {
        // 写数据
        Temp.masterVolume = index * 0.25f;
    }
}