using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TaskEffect
{
    public EffectType effectType;
    public string targetGUID;
    [HideInInspector] public BaseState state;
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
                state.Copyfrom(targetInteractionState);
                break;
            case EffectType.ITEMSTATE:
                state = GameFlowManager.Instance.PlayingData.GetState<ItemState>(targetGUID);

                snapShotState = state.Clone();
                state.Copyfrom(targetItemState);
                break;
            case EffectType.ACTORSTATE:
                state = GameFlowManager.Instance.PlayingData.GetState<ActorState>(targetGUID);

                snapShotState = state.Clone();
                state.Copyfrom(targetActorState);
                break;
        }

        // 刷新此物体在当前场景状态
        state.ScenedNotifyChanged();
    }

    public void RevertEffect()
    {
        if (snapShotState == null) return;

        switch (effectType)
        {
            case EffectType.INTERACTABLESTATE:
                state = GameFlowManager.Instance.PlayingData.GetState<InteractionState>(targetGUID);
                state.Copyfrom(snapShotState as InteractionState);
                break;
            case EffectType.ITEMSTATE:
                state = GameFlowManager.Instance.PlayingData.GetState<ItemState>(targetGUID);
                state.Copyfrom(snapShotState as ItemState);
                break;
            case EffectType.ACTORSTATE:
                state = GameFlowManager.Instance.PlayingData.GetState<ActorState>(targetGUID);
                state.Copyfrom(snapShotState as ActorState);
                break;
        }

        // 刷新此物体在当前场景状态
        state?.ScenedNotifyChanged();
    }
}
public enum EffectType
{
    INTERACTABLESTATE,
    ITEMSTATE,
    ACTORSTATE
}