using UnityEngine;

public class GraphicsPanel : SettingSubPanel
{
    [SerializeField] private UIOptionToggle fullscreenToggle;
    [SerializeField] private UIOptionToggle resolutionToggle;

    public override void RefreshUI()
    {
        // 全屏：Temp.isFullScreen 本身就是 0/1 的 int，直接作为索引
        fullscreenToggle.SetIndex(Temp.isFullScreen, false);
        // 分辨率索引
        resolutionToggle.SetIndex(Temp.resolutionIndex, false);
    }

    public void OnResolutionChanged(int index)
    {
        Temp.resolutionIndex = index;
    }

    public void OnFullscreenChanged(int index)
    {
        // index 为 0（窗口）或 1（全屏），直接赋值
        Temp.isFullScreen = index;
    }
}