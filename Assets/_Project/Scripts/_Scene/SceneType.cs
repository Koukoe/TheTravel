using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using System;

public abstract class RealScene : SceneBase
{
    public string sceneName;
    public string guid;
    public abstract RealSceneState GetBaseState();

    public bool autoSave = false;
    public abstract void SyncPlayerPosition();
}

[Serializable]
public struct PortalInfo
{
    public string portalName;
    public Transform transform;
}

public abstract class RealScene<T> : RealScene where T : RealSceneState, new()
{
    protected static Transform player;

    [SerializeField] private List<PortalInfo> scenePortals = new List<PortalInfo>();

    [SerializeField] protected Transform defaultSpawnPoint;

    [SerializeField] protected T _state;

    public override RealSceneState GetBaseState() => _state;

    public override void EnterScene()
    {
        GameFlowManager.Instance.PlayingData.currentScene = sceneName;
        CameraLink.Instance.LinkToMainCamera();

        if (PlayerController.Instance != null)
            player = PlayerController.Instance.transform;

        _state = GameFlowManager.Instance.PlayingData.GetState<T>(guid);
        if (!_state.isInitialized)
        {
            StateInitial();
            _state.isInitialized = true;
        }
        HandlePlayerPosition();
    }

    protected virtual void StateInitial() { }

    protected virtual void HandlePlayerPosition()
    {
        if (!player) return;

        // 传送目标地点
        if (!string.IsNullOrEmpty(_state.targetPortalGuid))
        {
            var portal = scenePortals.Find(p => p.portalName == _state.targetPortalGuid);

            if (portal.transform != null)
            {
                ApplyPosition(portal.transform.position, portal.transform.rotation);
                _state.targetPortalGuid = null;
                return;
            }
            Debug.LogWarning($"未找到传送点: {_state.targetPortalGuid}");
        }

        // 最后离开位置
        if (_state.lastExitPosition.HasValue)
        {
            ApplyPosition(_state.lastExitPosition.Value, _state.lastExitRotation.Value);
            return;
        }

        // 默认出生点
        if (defaultSpawnPoint != null)
        {
            ApplyPosition(defaultSpawnPoint.position, defaultSpawnPoint.rotation);
            SyncPlayerPosition();
        }
        else
        {
            Debug.LogWarning($"场景 {gameObject.scene.name} 传送失败");
        }
    }

    private void ApplyPosition(Vector3 pos, Quaternion rot)
    {
        player.position = pos;
        player.rotation = rot;
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
            _state.lastExitRotation = player.rotation;
        }
    }
}

[Serializable]
public class EntitySceneState : RealSceneState
{
    public List<string> poolEntityGuids = new List<string>();
    [NonSerialized] public Action OnEntityChanged;

    public override void Init(string id)
    {
        if (!string.IsNullOrEmpty(guid)) return;
        base.Init(id);
        poolEntityGuids = new List<string>();
    }

    public override BaseState Clone()
    {
        var clone = new EntitySceneState();
        clone.Copyfrom(this);
        clone.SetGUID(guid);
        return clone;
    }
    public override void Copyfrom(BaseState targetState)
    {
        base.Copyfrom(targetState);

        var state = targetState as EntitySceneState;
        if (state != null)
        {
            if (state.poolEntityGuids != null)
            {
                poolEntityGuids = new List<string>(state.poolEntityGuids);
            }
            else
            {
                poolEntityGuids = new List<string>();
            }
        }
    }
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
    // 初始
    [SerializeField] protected List<string> initPoolGuids;

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

    protected override void StateInitial()
    {
        if (_state.poolEntityGuids == null)
        {
            _state.poolEntityGuids = new List<string>(initPoolGuids);
        }
        else
        {
            foreach (var guid in initPoolGuids)
            {
                if (!_state.poolEntityGuids.Contains(guid))
                {
                    _state.poolEntityGuids.Add(guid);
                }
            }
        }
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