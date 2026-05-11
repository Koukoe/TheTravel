using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerDetector : MonoBehaviour
{
    // 进入范围且有接口的物体
    protected List<IInteractable> _candidates = new List<IInteractable>();
    public void ResetDetector()
    {
        _candidates.Clear();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.Hide("InteractTipCamera");
        }

        Debug.Log("Interaction Detector Reset.");
    }

    public virtual IInteractable GetTarget()
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
}




