using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class SceneTeleportObject : MonoBehaviour, IInteractable
{
    public int Priority => 1;

    [Header("传送配置")]
    public string TargetSceneName;
    public string TargetSceneGuid;
    public string TargetPortal;

    public Transform InteractTransform => gameObject.transform;
    public InteractTipWorld InteractTip { get; set; }

    public Vector3 TipOffset => _tipOffset;

    [Header("标签偏移")]
    [Tooltip("暂且没用")]
    [SerializeField]
    protected Vector3 _tipOffset = new Vector3(0, 2f, 0);

    public bool CanInteract()
    {
        return true;
    }

    public void DoInteract()
    {
        try
        {
            GameFlowManager.Instance.PlayingData.GetState<RealSceneState>("TargetSceneGuid").targetPortalGuid = TargetPortal;
            GameSceneManager.Instance.LoadMain(TargetSceneName).Forget();
        }
        catch
        {
            Debug.Log("传送失败");
        }
    }
}
