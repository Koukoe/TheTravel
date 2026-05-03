using UnityEngine;

/// <summary>
/// ItemState 操作工具
/// </summary>
public static class ItemStateUtils
{
    /// <summary>
    /// 标记指定 GUID 的道具为已获取
    /// </summary>
    public static void SetItemPicked(string itemGuid, bool picked)
    {
        if (string.IsNullOrWhiteSpace(itemGuid))
        {
            Debug.LogWarning("ItemStateUtils: 未指定 itemGuid");
            return;
        }

        if (GameFlowManager.Instance == null || GameFlowManager.Instance.PlayingData == null)
        {
            Debug.LogWarning($"ItemStateUtils: 存档未就绪: {itemGuid}");
            return;
        }

        ItemState state = GameFlowManager.Instance.PlayingData.GetState<ItemState>(itemGuid);
        state.isPicked = picked;
    }
}
