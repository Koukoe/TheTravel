using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class colorfulTown : MonoBehaviour, IInteractable
{
    public SceneTeleport sceneTeleport;
    public int Priority => 1;
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
