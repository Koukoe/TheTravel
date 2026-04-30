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
}

/// <summary> 活体/角色状态（NPC） </summary>
[Serializable]
public class ActorState : BaseState
{
    public Vector3? position = null;
    public Vector3? rotation = null;
    public string scene;

    public override void Init(string id)
    {
        if (!string.IsNullOrEmpty(guid)) return;
        base.Init(id);
        scene = "";
    }
}

/// <summary> 环境/交互物状态（门、机关） </summary>
[Serializable]
public abstract class InteractionState : BaseState
{
    public bool isTriggered;
    public int stateIndex;
    public override void Init(string id)
    {
        if (!string.IsNullOrEmpty(guid)) return;
        base.Init(id);
        isTriggered = false;
        stateIndex = 0;
    }
}

/// <summary> 物品/道具状态（图鉴） </summary>
[Serializable]
public abstract class ItemState : BaseState
{
    public bool isPicked;
    public override void Init(string id)
    {
        if (!string.IsNullOrEmpty(guid)) return;
        base.Init(id);
        isPicked = false;
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
    public override void Init(string id)
    {
        if (!string.IsNullOrEmpty(guid)) return;
        base.Init(id);
        targetPortalGuid = null;
        lastExitPosition = null;
    }
}