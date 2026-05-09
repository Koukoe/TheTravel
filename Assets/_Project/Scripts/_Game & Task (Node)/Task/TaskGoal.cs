using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class TaskGoal
{
    public TaskGoalType taskGoalType;
    private bool isDone;
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

    [Header("对应脚本协程，协程函数统一命名为TaskIEnumerator")]
    public TaskBasic targetScript;

    private BaseState targetState;
    private bool isChecking = false;
    private UniTaskCompletionSource<bool> pendingCheck;

    private async UniTask<bool> CheckGoalAsync()
    {
        if (isChecking)
        {
            if (pendingCheck != null)
                return await pendingCheck.Task;
            return isDone;
        }

        isChecking = true;
        pendingCheck = new UniTaskCompletionSource<bool>();

        try
        {
            switch (taskGoalType)
            {
                case TaskGoalType.TRIGGER:
                    CheckTrigger();
                    break;
                case TaskGoalType.ITEM:
                    CheckItem();
                    break;
                case TaskGoalType.ACTOR:
                    CheckActor();
                    break;
                case TaskGoalType.DIALOGUE:
                    CheckDialogue();
                    break;
                case TaskGoalType.SCRIPT:
                    await CheckScript();
                    break;
            }
        }
        finally
        {
            isChecking = false;
        }

        pendingCheck.TrySetResult(isDone);
        return isDone;
    }

    private void CheckTrigger()
    {
        targetState = GameFlowManager.Instance?.PlayingData?.GetState<InteractionState>(targetId);
        if (targetState is InteractionState interactionState)
        {
            IsDone = interactionState.isTriggered == GoalTrigger;
            Debug.Log($"{targetId} 触发检测: {(IsDone ? "成功" : "失败")}");
        }
        else
        {
            Debug.Log($"Target state not found: {targetId}");
            IsDone = false;
        }
    }

    private void CheckItem()
    {
        targetState = GameFlowManager.Instance?.PlayingData?.GetState<ItemState>(targetId);
        if (targetState is ItemState itemState)
        {
            IsDone = itemState.isPicked == GoalItem;
            Debug.Log($"{targetId} 物品检测: {(IsDone ? "成功" : "失败")}");
        }
        else
        {
            Debug.Log($"Target state not found: {targetId}");
            IsDone = false;
        }
    }

    private void CheckActor()
    {
        targetState = GameFlowManager.Instance?.PlayingData?.GetState<ActorState>(targetId);
        if (targetState is ActorState actorState &&
            actorState.position.HasValue &&
            actorState.rotation.HasValue)
        {
            IsDone = CheckActorPosition(actorState.position.Value, actorState.rotation.Value);
            Debug.Log($"{targetId} 角色位置检测: {(IsDone ? "成功" : "失败")}");
        }
        else
        {
            Debug.Log($"Target state not found or missing position/rotation: {targetId}");
            IsDone = false;
        }
    }

    private void CheckDialogue()
    {
        if (DialogueManager.Instance != null)
        {
            IsDone = DialogueManager.Instance.IsDialogueIndexCompleted(targetDialogueId, index);
            Debug.Log($"{targetDialogueId} {index} 对话检测: {(IsDone ? "成功" : "失败")}");
        }
        else
        {
            IsDone = false;
        }
    }

    private async UniTask CheckScript()
    {
        if (targetScript != null)
        {
            await targetScript.TaskIEnumerator();
            IsDone = targetScript.isDone;
        }
        else
        {
            IsDone = false;
        }
    }

    private bool CheckActorPosition(Vector3 targetPosition, Vector3 targetRotation)
    {
        float positionDistance = Vector3.Distance(GoalPosition, targetPosition);
        float rotationAngle = Quaternion.Angle(Quaternion.Euler(GoalRotation), Quaternion.Euler(targetRotation));

        return positionDistance < positionTolerance && rotationAngle < rotationTolerance;
    }

    public bool IsDone
    {
        get
        {
            if (isDone) return true;

            // 异步检查（非阻塞，立即返回当前状态）
            if (!isChecking)
            {
                CheckGoalAsync().Forget();
            }

            return isDone;
        }
        set
        {
            isDone = value;
            if (isDone)
            {
                Debug.Log("Task completed!");
            }
        }
    }

    /// <summary>
    /// 异步获取是否完成（会等待检查完成）
    /// </summary>
    public async UniTask<bool> IsDoneAsync()
    {
        if (isDone) return true;
        return await CheckGoalAsync();
    }
}

public enum TaskGoalType
{
    TRIGGER,
    ITEM,
    ACTOR,
    DIALOGUE,
    SCRIPT
}