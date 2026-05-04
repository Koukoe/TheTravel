using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TaskEffect
{
    public EffectType effectType;
    public string targetGUID;
    public BaseState state;
    public InteractionState targetInteractionState;
    public ItemState targetItemState;
    public ActorState targetActorState;

    private BaseState snapShotState;
    public void ApplyEffect()
    {
        switch (effectType)
        {
            case EffectType.INTERACTABLESTATE:
                state = GameFlowManager.Instance.PlayingData.GetState<InteractionState>(targetGUID);

                snapShotState = state.Clone();
                state = targetInteractionState;
                break;
            case EffectType.ITEMSTATE:
                state = GameFlowManager.Instance.PlayingData.GetState<ItemState>(targetGUID);

                snapShotState = state.Clone();
                state = targetItemState;
                break;
            case EffectType.ACTORSTATE:
                state = GameFlowManager.Instance.PlayingData.GetState<ActorState>(targetGUID);

                snapShotState = state.Clone();
                state = targetActorState;
                break;
        }

        if (true)//如果物体在当前场景
        {
            //刷新此物体状态
        }
    }

    public void RevertEffect()
    {
        switch (effectType)
        {
            case EffectType.INTERACTABLESTATE:
                state = GameFlowManager.Instance.PlayingData.GetState<InteractionState>(targetGUID);
                state = snapShotState as InteractionState;
                break;
            case EffectType.ITEMSTATE:
                state = GameFlowManager.Instance.PlayingData.GetState<ItemState>(targetGUID);
                state = snapShotState as ItemState;
                break;
            case EffectType.ACTORSTATE:
                state = GameFlowManager.Instance.PlayingData.GetState<ActorState>(targetGUID);
                state = snapShotState as ActorState;
                break;
        }

        if (true)//如果物体在当前场景
        {
            //刷新此物体状态
        }
    }
}
public enum EffectType
{
    INTERACTABLESTATE,
    ITEMSTATE,
    ACTORSTATE
}