using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public class MelodicStoneSingleInteractable : MonoBehaviour, IInteractable
{

    [SerializeField] private int _priority;
    [SerializeField] private Vector3 _tipOffset;

    public int Priority => _priority;

    public Transform InteractTransform => transform;
    public InteractTipWorld InteractTip { get; set; }
    public Vector3 TipOffset => _tipOffset;

    public MelodicStonesEntity mainState;

    [SerializeField] private int _solfege;

    public bool CanInteract()
    {
        return mainState.GetState.isInteracble;
    }

    public void DoInteract()
    {
        mainState.JugdeSolfege(_solfege);
    }


}
