using UnityEngine;
using UnityEngine.UI;

public class AudioPanel : SettingSubPanel
{
    [SerializeField] private UIOptionToggle masterVolToggle;
    [SerializeField] private UIOptionToggle musicVolToggle;
    [SerializeField] private UIOptionToggle sfxVolToggle;
    [SerializeField] private UIOptionToggle ambVolToggle;

    public override void RefreshUI()
    {
        // 读数据并同步 UI
        masterVolToggle.SetIndex(Temp.masterVolumeIndex, false);
        musicVolToggle.SetIndex(Temp.musicVolumeIndex, false);
        sfxVolToggle.SetIndex(Temp.sfxVolumeIndex, false);
        ambVolToggle.SetIndex(Temp.ambVolumeIndex, false);
    }

    public void OnMasterVolChanged(int index) => Temp.masterVolumeIndex = index;
    public void OnMusicVolChanged(int index) => Temp.musicVolumeIndex = index;
    public void OnSFXVolChanged(int index) => Temp.sfxVolumeIndex = index;
    public void OnAmbVolChanged(int index) => Temp.ambVolumeIndex = index;
}