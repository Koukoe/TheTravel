using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerDetectorTipCamera : PlayerDetector
{

    private void OnTriggerEnter(Collider other)
    {
        // 自动触发逻辑
        other.GetComponentInParent<ITriggerable>()?.OnEntered();

        // 获取物体上所有 IInteractable 组件，避免 GetComponentInParent 只返回第一个的问题
        var interactables = other.GetComponentsInParent<IInteractable>();
        foreach (var interactable in interactables)
        {
            if (interactable != null && !_candidates.Contains(interactable))
            {
                _candidates.Add(interactable);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 自动触发逻辑
        other.GetComponentInParent<ITriggerable>()?.OnExited();

        var interactables = other.GetComponentsInParent<IInteractable>();
        foreach (var interactable in interactables)
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
