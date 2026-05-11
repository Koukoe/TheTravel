using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    int Priority { get; }
    Transform InteractTransform { get; }

    InteractTipWorld InteractTip { get; set; }
    Vector3 TipOffset { get; }

    bool CanInteract();
    void DoInteract();
}

public interface IInteractablePC : IInteractable
{

    /// <summary>
    /// 请在 Priority 变化处写 OnPriorityChanged.?Invoke
    /// </summary>
    event System.Action<IInteractable> OnPriorityChanged;
}

public interface ITriggerable
{
    void OnEntered();
    void OnExited();
}

