using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class andosi : StaticStateEntity<ItemState>, IInteractable
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
        return true;
    }

    public void DoInteract()
    {
        try
        {
            GameFlowManager.Instance.PlayingData.GetState<ItemState>(_state.guid).isPicked = true;
        }
        catch
        {
            Debug.Log("andosi诡异错误");
        }
    }

    protected override void OnStateBound()
    {
        if (_state.isPicked) gameObject.SetActive(false);
        else gameObject.SetActive(true);
    }
}
