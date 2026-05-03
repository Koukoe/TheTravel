using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TaskGoal
{
    public TaskGoalType taskGoalType;
    [SerializeField] private bool isDone;
    public string targetId;
    [Header("是否应该触发")]
    public bool GoalTrigger;
    [Header("是否应该获得")]
    public bool GoalItem;
    [Header("角色目标位置")]
    public Vector3 GoalPosition;
    public Vector3 GoalRotation;
    [Header("位置和旋转的容差")]
    public float positionTolerance = 0.1f;
    [Range(0, 360)]
    public float rotationTolerance = 0.1f;

    [Header("查找对话")]
    public string targetDialogueId = null;
    [Header("对话索引")]
    public int index = 0;

    BaseState targetState;
    private void checkGoal()
    {
        switch (taskGoalType)
        {
            case TaskGoalType.TRIGGER:
                // Check if the trigger has been activated
                targetState = GameFlowManager.Instance.PlayingData.GetState<InteractionState>(targetId);
                if (targetState == null)
                {
                    Debug.Log("Target state not found");
                }
                if (targetState is InteractionState interactionState && interactionState.isTriggered == GoalTrigger)
                {
                    IsDone = true;
                    Debug.Log(targetId + "检测成功");
                }
                else
                {
                    IsDone = false;
                    Debug.Log(targetId + "检测失败");
                }
                break;
            case TaskGoalType.ITEM:
                // Check if the required amount of normal items has been collected
                targetState = GameFlowManager.Instance.PlayingData.GetState<ItemState>(targetId);
                if (targetState == null)
                {
                    Debug.Log("Target state not found");
                }
                if (targetState is ItemState itemState && itemState.isPicked == GoalItem)
                {
                    IsDone = true;
                    Debug.Log(targetId + "检测成功");
                }
                else
                {
                    IsDone = false;
                    Debug.Log(targetId + "检测失败");
                }
                break;
            case TaskGoalType.ACTOR:
                // Check if the required amount of book items has been collected
                targetState = GameFlowManager.Instance.PlayingData.GetState<ActorState>(targetId);
                if (targetState == null)
                {
                    Debug.Log("Target state not found");
                }
                if (targetState is ActorState actorState &&
                            actorState.position.HasValue &&
                            actorState.rotation.HasValue &&
                            checkActor(actorState.position.Value, actorState.rotation.Value))
                {
                    IsDone = true;
                    Debug.Log(targetId + "检测成功");
                }
                else
                {
                    IsDone = false;
                    Debug.Log(targetId + "检测失败");
                }
                break;
            case TaskGoalType.DIALOGUE:
                // Check if the DIALOGUE FINISHED
                if (DialogueManager.Instance.IsDialogueIndexCompleted(targetDialogueId, index))
                {
                    IsDone = true;
                    Debug.Log(targetDialogueId + " " + index + "检测成功");
                }
                else
                {
                    IsDone = false;
                    Debug.Log(targetDialogueId + " " + index + "检测失败");
                }
                break;

        }
    }

    private bool checkActor(Vector3 targetPostion, Vector3 targetRotation)
    {
        return Vector3.Distance(GoalPosition, targetPostion) < positionTolerance &&
               Mathf.Abs(Quaternion.Angle(Quaternion.Euler(GoalRotation), Quaternion.Euler(targetRotation))) < rotationTolerance;
    }

    public bool IsDone
    {
        get
        {
            checkGoal();
            return isDone;
        }
        set
        {
            isDone = value;
            if (isDone)
            {
                // Perform actions when the task is completed
                Debug.Log("Task completed!");
            }
        }
    }
}

public enum TaskGoalType
{
    TRIGGER,
    ITEM,
    ACTOR,
    DIALOGUE
}
