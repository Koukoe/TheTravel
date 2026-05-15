using System;
using UnityEngine;

[Serializable]
public abstract class BaseState
{
    [field: SerializeField] public string guid { get; private set; }
    public string name;
    public virtual void Init(string id)
    {
        if (string.IsNullOrEmpty(guid))
        {
            guid = id;
        }
        else if (guid != id)
        {
            Debug.LogError($"[State] 试图修改已存在的 GUID：{guid}为: {id}");
        }
    }
    protected void SetGUID(string id) => guid = id;

    public abstract BaseState Clone();

    public abstract void Copyfrom(BaseState targetState);
    [NonSerialized] public Action OnDataChanged;  // 当前场景的变化委托，无需保存

    public void ScenedNotifyChanged() => OnDataChanged?.Invoke();
}

/// <summary> 活体/角色状态（NPC） </summary>
[Serializable]
public class ActorState : BaseState
{
    // 序列化用的私有字段
    [SerializeField] private Vector3 serializedPosition;
    [SerializeField] private Vector3 serializedRotation;
    [SerializeField] private bool hasPosition;
    [SerializeField] private bool hasRotation;

    // 公开的可空属性
    public Vector3? position
    {
        get => hasPosition ? serializedPosition : null;
        set
        {
            hasPosition = value.HasValue;
            serializedPosition = value ?? Vector3.zero;
        }
    }

    public Vector3? rotation
    {
        get => hasRotation ? serializedRotation : null;
        set
        {
            hasRotation = value.HasValue;
            serializedRotation = value ?? Vector3.zero;
        }
    }

    public bool isVisible;

    [SerializeField] private string Scene; // 改为 SerializeField 以支持序列化
    public string scene
    {
        get => Scene;
        set => Scene = value;
    }

    public AnimState animState;

    public override void Init(string id)
    {
        if (!string.IsNullOrEmpty(guid)) return;
        base.Init(id);
        isVisible = true;
        scene = "";
    }

    public enum AnimState
    {
        IDLE,
        WALK,
        RUN,
        SIT
    }

    public override BaseState Clone()
    {
        var clone = new ActorState()
        {
            name = name,
            hasPosition = this.hasPosition,
            hasRotation = this.hasRotation,
            serializedPosition = this.serializedPosition,
            serializedRotation = this.serializedRotation,
            isVisible = isVisible,
            scene = scene,
            animState = animState
        };
        clone.SetGUID(guid);
        return clone;
    }

    public override void Copyfrom(BaseState targetState)
    {
        var state = targetState as ActorState;
        if (state != null)
        {
            hasPosition = state.hasPosition;
            hasRotation = state.hasRotation;
            serializedPosition = state.serializedPosition;
            serializedRotation = state.serializedRotation;
            isVisible = state.isVisible;
            scene = state.scene;
            animState = state.animState;
        }
    }
}

/// <summary> 环境/交互物状态（门、机关） </summary>
[Serializable]
public class InteractionState : BaseState
{
    public bool isTriggered;
    public int stateIndex;
    public bool isInteracble;
    public override void Init(string id)
    {
        if (!string.IsNullOrEmpty(guid)) return;
        base.Init(id);
        isTriggered = false;
        isInteracble = false;
        stateIndex = 0;
    }

    public override BaseState Clone()
    {
        var clone = new InteractionState()
        {
            name = name,
            isTriggered = isTriggered,
            stateIndex = stateIndex,
            isInteracble = isInteracble
        };
        clone.SetGUID(guid);
        return clone;
    }

    public override void Copyfrom(BaseState targetState)
    {
        var state = targetState as InteractionState;
        if (state != null)
        {
            isTriggered = state.isTriggered;
            stateIndex = state.stateIndex;
            isInteracble = state.isInteracble;
        }
    }
}

/// <summary> 物品/道具状态（图鉴） </summary>
[Serializable]
public class ItemState : BaseState
{
    public bool isPicked;
    public override void Init(string id)
    {
        if (!string.IsNullOrEmpty(guid)) return;
        base.Init(id);
        isPicked = false;
    }

    public override BaseState Clone()
    {
        var clone = new ItemState()
        {
            name = name,
            isPicked = isPicked
        };
        clone.SetGUID(guid);
        return clone;
    }

    public override void Copyfrom(BaseState targetState)
    {
        var state = targetState as ItemState;
        if (state != null)
        {
            isPicked = state.isPicked;
        }
    }
}

/// <summary> 真实场景状态 </summary>
[Serializable]
public class RealSceneState : BaseState
{
    // 记录进入场景的传送点名字
    public string targetPortalGuid;

    // 记录玩家离开场景时的最后坐标
    public Vector3? lastExitPosition;
    public Quaternion? lastExitRotation;

    // isInitialized 不从存档传播，场景加载时由 SceneManager 重新设置
    public bool isInitialized = false;

    public override void Init(string id)
    {
        if (!string.IsNullOrEmpty(guid)) return;
        base.Init(id);
        targetPortalGuid = null;
        lastExitPosition = null;
        isInitialized = false;
    }

    public override BaseState Clone()
    {
        var clone = new RealSceneState()
        {
            name = name,
            targetPortalGuid = targetPortalGuid,
            lastExitPosition = lastExitPosition,
            // isInitialized — 不传播，场景加载时重新设置
        };
        clone.SetGUID(guid);
        return clone;
    }
    public override void Copyfrom(BaseState targetState)
    {
        var state = targetState as RealSceneState;
        if (state != null)
        {
            targetPortalGuid = state.targetPortalGuid;
            lastExitPosition = state.lastExitPosition;
            lastExitRotation = state.lastExitRotation;
            // isInitialized — 不传播
        }
    }
}

[Serializable]
public class TaskGoalState : BaseState
{
    public bool isReached;

    public override void Init(string id)
    {
        if (!string.IsNullOrEmpty(guid)) return;
        base.Init(id);
        isReached = false;
    }

    public override BaseState Clone()
    {
        var clone = new TaskGoalState() { name = name, isReached = isReached };
        clone.SetGUID(guid);
        return clone;
    }

    public override void Copyfrom(BaseState targetState)
    {
        if (targetState is TaskGoalState state) isReached = state.isReached;
    }
}