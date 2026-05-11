using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class townEntity : MonoBehaviour, IInteractable
{
    public SceneTeleport sceneTeleport;
    public int Priority => 1;

    public Transform InteractTransform => gameObject.transform;
    public InteractTipWorld InteractTip { get; set; }
    public Vector3 TipOffset => _tipOffset;

    [SerializeField]
    protected Vector3 _tipOffset = new Vector3(0, 2f, 0);

    public bool CanInteract()
    {
        return true;
    }

    public void DoInteract()
    {
        sceneTeleport = GetComponent<SceneTeleport>();
        if (sceneTeleport != null)
        {
            _ = sceneTeleport.DoorTP();
        }
        else
        {
            Debug.Log("No scene teleport found");
        }
    }
}
