using UnityEngine;

/// <summary>
/// ActorState 操作与实体应用工具——纯状态层，不依赖任何对话或 UI 系统
/// </summary>
public static class ActorStateUtils
{
    /// <summary>
    /// 修改指定 GUID 的 ActorState 可见性，并立即应用到场景中已绑定的实体
    /// </summary>
    public static void SetActorVisibility(string actorGuid, bool isVisible)
    {
        if (!TryGetActorState(actorGuid, out ActorState state))
        {
            return;
        }

        state.isVisible = isVisible;
        ApplyActorStateToLiveEntities(actorGuid, state);
    }

    /// <summary>
    /// 修改指定 GUID 的 ActorState 位置和旋转，并立即应用到场景中已绑定的实体
    /// </summary>
    public static void SetActorTransform(string actorGuid, Vector3 position, Vector3? rotation)
    {
        if (!TryGetActorState(actorGuid, out ActorState state))
        {
            return;
        }

        state.position = position;
        if (rotation.HasValue)
        {
            state.rotation = rotation.Value;
        }

        ApplyActorStateToLiveEntities(actorGuid, state);
    }

    /// <summary>
    /// 从 DataArchive 获取 ActorState，失败时打印警告
    /// </summary>
    private static bool TryGetActorState(string actorGuid, out ActorState state)
    {
        state = null;

        if (string.IsNullOrWhiteSpace(actorGuid))
        {
            Debug.LogWarning("ActorStateUtils: 未指定 actorGuid");
            return false;
        }

        if (GameFlowManager.Instance == null || GameFlowManager.Instance.PlayingData == null)
        {
            Debug.LogWarning($"ActorStateUtils: 存档未就绪, 无法修改 NPC 状态: {actorGuid}");
            return false;
        }

        state = GameFlowManager.Instance.PlayingData.GetState<ActorState>(actorGuid);
        if (state == null)
        {
            Debug.LogWarning($"ActorStateUtils: 目标 ActorState 不存在: {actorGuid}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 将 ActorState 应用到场景中所有绑定了该 GUID 的 PoolStateEntity&lt;ActorState&gt;
    /// </summary>
    private static void ApplyActorStateToLiveEntities(string actorGuid, ActorState state)
    {
        if (state == null)
        {
            return;
        }

        PoolStateEntity<ActorState>[] entities = UnityEngine.Object.FindObjectsOfType<PoolStateEntity<ActorState>>(true);
        for (int i = 0; i < entities.Length; i++)
        {
            PoolStateEntity<ActorState> entity = entities[i];
            if (entity == null || !string.Equals(entity.BoundGuid, actorGuid, System.StringComparison.Ordinal))
            {
                continue;
            }

            entity.TryApplyActorStateImmediate(state);
        }
    }
}
