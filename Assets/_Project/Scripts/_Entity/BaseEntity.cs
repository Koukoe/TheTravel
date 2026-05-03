using System;
using UnityEngine;

public abstract class StaticStateEntity<T> : MonoBehaviour where T : BaseState, new()
{
    public string guid;
    public string defaultName;
    protected T _state;

    protected virtual void Start()
    {
        _state = GameFlowManager.Instance.PlayingData.GetState<T>(guid);
        if (string.IsNullOrEmpty(_state.name))
        {
            if (!string.IsNullOrEmpty(defaultName))
                _state.name = defaultName;
        }

        OnStateBound();
    }


    protected abstract void OnStateBound();
}

public abstract class PoolStateEntity<T> : MonoBehaviour where T : BaseState, new()
{
    protected T _state;
    public string defaultName;
    protected string _guid;
    public string BoundGuid => _guid;

    public virtual void BindState(string guid)
    {
        _guid = guid;
        _state = GameFlowManager.Instance.PlayingData.GetState<T>(_guid);

        if (_state is ActorState actor)
        {
            ApplyActorState(actor);
            actor.scene = gameObject.scene.name;
        }

        if (string.IsNullOrEmpty(_state.name))
        {
            if (!string.IsNullOrEmpty(defaultName))
                _state.name = defaultName;
        }
        OnStateBound();
    }

    protected virtual void ApplyActorState(ActorState actor)
    {
        if (actor == null)
        {
            return;
        }

        if (actor.position.HasValue)
        {
            transform.position = actor.position.Value;
            transform.eulerAngles = actor.rotation ?? Vector3.zero;
        }

        gameObject.SetActive(actor.isVisible);
    }

    protected virtual void SyncActorStateFromTransform()
    {
        if (!(_state is ActorState actor))
        {
            return;
        }

        actor.position = transform.position;
        actor.rotation = transform.eulerAngles;
        actor.scene = gameObject.scene.name;
        actor.isVisible = gameObject.activeSelf;
    }

    protected abstract void OnStateBound();

    public T GetState() => _state;

    public bool TryApplyActorStateImmediate(ActorState actor)
    {
        if (actor == null || !(_state is ActorState currentActor))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(_guid) && !string.Equals(_guid, actor.guid, StringComparison.Ordinal))
        {
            return false;
        }

        currentActor.position = actor.position;
        currentActor.rotation = actor.rotation;
        currentActor.scene = actor.scene;
        currentActor.isVisible = actor.isVisible;
        ApplyActorState(currentActor);
        return true;
    }

    public virtual void ReturnToPool()
    {
        OnBeforeReturn();
        PoolManager.Release(gameObject);
    }

    protected virtual void OnBeforeReturn()
    {
        SyncActorStateFromTransform();
    }
}