using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using System;

public abstract class RealScene : SceneBase
{
    public abstract RealSceneState GetBaseState();
}

public abstract class RealScene<T> : RealScene where T : RealSceneState, new()
{
    protected static Transform player;

    [SerializeField] protected Transform defaultSpawnPoint;

    protected T _state;

    public override RealSceneState GetBaseState() => _state;

    public override void EnterScene()
    {
        CameraLink.Instance.LinkToMainCamera();

        if (PlayerController.Instance != null)
            player = PlayerController.Instance.transform;

        _state = GameFlowManager.Instance.PlayingData.GetState<T>(gameObject.scene.name);
        HandlePlayerPosition();
    }

    protected virtual void HandlePlayerPosition()
    {
        if (!player) return;

        // 传送目标地点
        if (!string.IsNullOrEmpty(_state.targetPortalGuid))
        {
            GameObject portal = GameObject.Find(_state.targetPortalGuid);
            if (portal != null)
            {
                player.position = portal.transform.position;
                _state.targetPortalGuid = null;  // 清理传送位置
                _state.lastExitPosition = player.position;
                return;
            }
        }

        // 最后离开位置
        if (_state.lastExitPosition.HasValue)
        {
            player.position = _state.lastExitPosition.Value;
            return;
        }

        // 默认出生点
        if (defaultSpawnPoint != null)
        {
            player.position = defaultSpawnPoint.position;
            _state.lastExitPosition = player.position;
        }
        else
        {
            Debug.LogWarning($"场景 {gameObject.scene.name} 传送失败");
        }
    }

    public override void ExitScene()
    {
        SyncPlayerPosition();
        // 场景退出时的清理逻辑（如果有需要）
        // 不要抛出异常
    }

    public void SyncPlayerPosition()
    {
        if (_state != null && player != null)
        {
            _state.lastExitPosition = player.position;
        }
    }
}

public abstract class EntityScene<TState> : RealScene<TState>
    where TState : RealSceneState, new()
{
    public EntityDatabase entityDb;

    protected abstract IEnumerable<string> GetActiveGuids();

    protected void SpawnEntities()
    {
        if (entityDb == null) return;
        var guids = GetActiveGuids();
        if (guids == null) return;

        foreach (var guid in guids)
        {
            var template = entityDb.GetEntity(guid);
            if (template.prefab == null) continue;

            var rawState = GameFlowManager.Instance.PlayingData.GetState<ActorState>(guid);

            if (!rawState.position.HasValue)
            {
                rawState.position = template.defaultPosition;
                rawState.rotation = template.defaultRotation;
            }

            GameObject go = PoolManager.Global.Get(template.prefab);
            var entity = go.GetComponent<PoolStateEntity<ActorState>>();

            entity.defaultName = template.defaultName;
            entity.BindState(guid);
        }
    }
}

