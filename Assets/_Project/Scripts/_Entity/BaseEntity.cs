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

    public virtual void BindState(string guid)
    {
        _guid = guid;
        _state = GameFlowManager.Instance.PlayingData.GetState<T>(_guid);

        if (_state is ActorState actor)
        {
            if (actor.position.HasValue)
            {
                transform.position = actor.position.Value;
                transform.eulerAngles = actor.rotation ?? Vector3.zero;
            }
            actor.scene = gameObject.scene.name;
        }

        if (string.IsNullOrEmpty(_state.name))
        {
            if (!string.IsNullOrEmpty(defaultName))
                _state.name = defaultName;
        }
        OnStateBound();
    }

    protected abstract void OnStateBound();

    public T GetState() => _state;

    public virtual void ReturnToPool()
    {
        OnBeforeReturn();
        PoolManager.Release(gameObject);
    }

    protected virtual void OnBeforeReturn() { }
}