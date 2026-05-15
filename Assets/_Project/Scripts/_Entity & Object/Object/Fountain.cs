using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fountain : StaticStateEntity<InteractionState>, IInteractable
{
    public List<string> itemsToPickUp = new List<string>();
    public List<string> triggerToActive = new List<string>();

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
        bool flag = true;
        foreach (var item in itemsToPickUp)
        {
            if (!GameFlowManager.Instance.PlayingData.GetState<ItemState>(item).isPicked)
            {
                flag = false;
                break;
            }
        }
        foreach (var trigger in triggerToActive)
        {
            if (!GameFlowManager.Instance.PlayingData.GetState<ActorState>(trigger).isVisible)
            {
                flag = false;
                break;
            }
        }
        return flag;
    }
    public void DoInteract()
    {
        try
        {
            FountainWaterRipple.Instance.CreateRipple();
        }
        catch
        {
            Debug.Log("Fountain诡异错误");
        }
    }
    protected override void OnStateBound()
    {
    }
}
