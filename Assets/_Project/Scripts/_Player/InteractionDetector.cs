using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public interface IInteractable
{
    int Priority { get; }

    bool CanInteract();
    void DoInteract();
}



public class InteractionDetector : MonoBehaviour
{
    // 进入范围且有接口的物体
    private List<IInteractable> _candidates = new List<IInteractable>();

    private void OnTriggerEnter(Collider other)
    {
        var interactable = other.GetComponentInParent<IInteractable>();
        if (interactable != null && !_candidates.Contains(interactable))
        {
            _candidates.Add(interactable);
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.Show("InteractTip");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var interactable = other.GetComponentInParent<IInteractable>();
        if (interactable != null)
        {
            _candidates.Remove(interactable);
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.Hide("InteractTip");
        }
    }

    public IInteractable GetTarget()
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
            .OrderByDescending(x => x.Priority)  // 优先级最高优先
            .ThenBy(x => Vector3.Distance(transform.position, ((MonoBehaviour)x).transform.position))  // 同优先级选最近
            .FirstOrDefault();
    }

    private void UpdateUI()
    {
    }
}