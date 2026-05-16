using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetWoolenCup : StaticStateEntity<InteractionState>, IInteractable
{
    public int Priority => 1;
    public Transform InteractTransform => gameObject.transform;
    public InteractTipWorld InteractTip { get; set; }
    public Vector3 TipOffset => _tipOffset;
    [Header("标签偏移")]
    [Tooltip("暂且没用")]
    [SerializeField]
    protected Vector3 _tipOffset = new Vector3(0, 2f, 0);

    public bool CanInteract()
    {
        return GameFlowManager.Instance.PlayingData.GetState<ItemState>("cupofwater").isPicked;
    }
    public void DoInteract()
    {
        GameFlowManager.Instance.PlayingData.GetState<ItemState>("woolencup").isPicked = true;
        Debug.Log("woolencup is picked up");
    }
    protected override void OnStateBound()
    {
    }
}
