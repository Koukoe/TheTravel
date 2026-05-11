using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using System;

public abstract class RealScene : SceneBase
{
    public string sceneName;
    public abstract RealSceneState GetBaseState();

    public bool autoSave = false;
    public abstract void SyncPlayerPosition();
}

public abstract class RealScene<T> : RealScene where T : RealSceneState, new()
{
    protected static Transform player;

    [SerializeField] protected Transform defaultSpawnPoint;

    [SerializeField] protected T _state;

    public override RealSceneState GetBaseState() => _state;

    public override void EnterScene()
    {
        GameFlowManager.Instance.PlayingData.currentScene = sceneName;
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
        if (autoSave)
        {
            GameFlowManager.Instance.OnCheckPoint().Forget();
        }
        // 场景退出时的清理逻辑（如果有需要）
        // 不要抛出异常
    }

    public override void SyncPlayerPosition()
    {
        if (_state != null && player != null)
        {
            _state.lastExitPosition = player.position;
        }
    }
}

[Serializable]
public class EntitySceneState : RealSceneState
{
    public List<string> poolEntityGuids = new List<string>();
    [NonSerialized] public Action OnEntityChanged;

    /// <summary>
    /// 将实体登记到场景数据名单中
    /// </summary>
    public void RegisterPoolEntity(string guid)
    {
        if (string.IsNullOrEmpty(guid) || poolEntityGuids.Contains(guid)) return;

        poolEntityGuids.Add(guid);
        // 刷新表现层
        OnEntityChanged?.Invoke();
    }


    /// <summary>
    /// 将实体从场景数据名单中移除
    /// </summary>
    public void UnregisterPoolEntity(string guid)
    {
        if (poolEntityGuids.Remove(guid))
        {
            OnEntityChanged?.Invoke();
        }
    }
}


public abstract class EntityScene<TState> : RealScene<TState>
    where TState : EntitySceneState, new()
{
    public EntityDatabase entityDb;

    // 缓存已生成的实体，方便回收
    private Dictionary<string, PoolStateEntity<ActorState>> _activeEntities = new();

    // 运行时 Hash 缓存，方便快速同步
    private HashSet<string> _targetGuidsCache = new HashSet<string>();

    public override void EnterScene()
    {
        base.EnterScene();
        // 订阅数据变化
        if (_state != null) _state.OnEntityChanged += RefreshSceneEntities;

        // 执行初始生成
        RefreshSceneEntities();
    }

    public override void ExitScene()
    {
        // 退订
        if (_state != null) _state.OnEntityChanged -= RefreshSceneEntities;

        // 回收
        var activeGuids = new List<string>(_activeEntities.Keys);
        foreach (var guid in activeGuids)
        {
            DetachEntity(guid);
        }
        _activeEntities.Clear();

        base.ExitScene();
    }

    /// <summary>
    /// 全量刷新，根据最新名单同步场景表现（多退少补）
    /// </summary>
    protected void RefreshSceneEntities()
    {
        if (entityDb == null || _state == null) return;

        // List 转 Hash
        _targetGuidsCache.Clear();
        foreach (var g in _state.poolEntityGuids)
        {
            _targetGuidsCache.Add(g);
        }

        // 找出需要新增的
        foreach (var guid in _targetGuidsCache)
        {
            if (!_activeEntities.ContainsKey(guid)) AttachEntity(guid);
        }

        // 找出需要移除的
        List<string> toRemove = new List<string>();
        foreach (var spawnedGuid in _activeEntities.Keys)
        {
            if (!_targetGuidsCache.Contains(spawnedGuid))
            {
                toRemove.Add(spawnedGuid);
            }
        }

        foreach (var guid in toRemove) DetachEntity(guid);
    }

    /// <summary>
    /// 从对象池取出实体并完成数据绑定
    /// </summary>
    private void AttachEntity(string guid)
    {
        var template = entityDb.GetEntity(guid);
        if (template.prefab == null) return;

        // 预取状态，如果坐标没值则提前初始化（PoolStateEntity 内部 BindState 时能直接拿到正确位置）
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

        _activeEntities.Add(guid, entity);
    }

    /// <summary>
    /// 解绑实体并回收至对象池
    /// </summary>
    private void DetachEntity(string guid)
    {
        if (_activeEntities.TryGetValue(guid, out var entity))
        {
            entity.ReturnToPool();
            _activeEntities.Remove(guid);
        }
    }
}