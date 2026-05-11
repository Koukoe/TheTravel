using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class PlayerDetectorTipWorld : PlayerDetector
{
    private IInteractable _lastBestTarget;

    private void OnTriggerEnter(Collider other)
    {
        // 自动触发逻辑
        other.GetComponentInParent<ITriggerable>()?.OnEntered();

        var interactable = other.GetComponentInParent<IInteractable>();
        if (interactable != null && !_candidates.Contains(interactable))
        {
            _candidates.Add(interactable);

            // 如果是可变优先级的接口，则绑定优先级改变事件
            if (interactable is IInteractablePC observable)
            {
                observable.OnPriorityChanged += OnTargetPriorityChanged;
            }

            // 生成 UI 并刷新当前高亮目标
            ShowTip(interactable);
            RefreshFocus();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 自动触发逻辑
        other.GetComponentInParent<ITriggerable>()?.OnExited();

        var interactable = other.GetComponentInParent<IInteractable>();
        if (interactable != null)
        {
            // 解除监听
            if (interactable is IInteractablePC observable)
            {
                observable.OnPriorityChanged -= OnTargetPriorityChanged;
            }

            HideTip(interactable);
            _candidates.Remove(interactable);

            // 重新计算优先级
            RefreshFocus();
        }
    }

    private void OnTargetPriorityChanged(IInteractable obj)
    {
        RefreshFocus();
    }

    private void RefreshFocus()
    {
        IInteractable currentBest = GetTarget();

        // 状态切换处理
        if (_lastBestTarget == currentBest) return;

        // 旧目标恢复普通状态
        if (_lastBestTarget != null && _lastBestTarget.InteractTip != null)
        {
            _lastBestTarget.InteractTip.SetFocus(false);
        }

        // 新目标进入高亮状态
        if (currentBest != null && currentBest.InteractTip != null)
        {
            currentBest.InteractTip.SetFocus(true);
        }

        _lastBestTarget = currentBest;
    }

    private void ShowTip(IInteractable target)
    {
        var tip = UIManager.Instance.Show("InteractTipWorld", false) as InteractTipWorld;

        if (tip != null)
        {
            tip.Bind(target);  // 绑定父物体
            target.InteractTip = tip;  // 互相引用
        }
    }

    private void HideTip(IInteractable target)
    {
        if (target.InteractTip != null)
        {
            UIManager.Instance.Hide(target.InteractTip);
            target.InteractTip = null;
        }
    }

    public override IInteractable GetTarget()
    {
        _candidates.RemoveAll(x => x == null || (x as MonoBehaviour) == null);  // 趁机清理 Destory 的物体

        // 过滤掉不能交互的，按优先级降序排
        return _candidates
            .Where(x =>
            {
                var mono = x as MonoBehaviour;
                return mono != null
                && mono.gameObject.activeInHierarchy
                && mono.enabled
                && x.CanInteract();
            })  // 排除 Disable 的与不可交互的物体
            .OrderByDescending(x => x.Priority)  // 优先级最高优先，同优先级选最近看先接触谁
            .FirstOrDefault();
    }
}

*/
