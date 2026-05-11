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
        if (UIManager.Instance != null)
        {
            UIManager.Instance.Show("InteractTipCamera");
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
        if (_candidates.Count == 0 || GetTarget() == null)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.Hide("InteractTipCamera");
            }
        }
    }
}
