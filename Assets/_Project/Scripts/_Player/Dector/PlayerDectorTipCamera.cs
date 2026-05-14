using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetectorTipCamera : PlayerDetector
{

    private void OnTriggerEnter(Collider other)
    {
        // 自动触发逻辑
        other.GetComponentInParent<ITriggerable>()?.OnEntered();

        var interactable = other.GetComponentInParent<IInteractable>();
        if (interactable != null && !_candidates.Contains(interactable))
        {
            _candidates.Add(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 自动触发逻辑
        other.GetComponentInParent<ITriggerable>()?.OnExited();

        var interactable = other.GetComponentInParent<IInteractable>();
        if (interactable != null)
        {
            _candidates.Remove(interactable);
        }
    }

    private void Update()
    {
        if (UIManager.Instance == null) return;

        if (GetTarget() != null)
        {
            UIManager.Instance.Show("InteractTipCamera");
        }
        else
        {
            UIManager.Instance.Hide("InteractTipCamera");
        }
    }
}
