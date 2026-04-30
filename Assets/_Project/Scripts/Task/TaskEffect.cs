using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TaskEffect
{
    public EffectType effectType;
    public GameObject targetObject;
    public Behaviour targetComponent;
    public Vector3 targetPosition;

    private Vector3 reservePosition;
    public void ApplyEffect()
    {
        switch (effectType)
        {
            case EffectType.changePosition:
                // Change the position of the target object
                reservePosition = targetObject.transform.position;
                targetObject.transform.position = targetPosition;
                /*
                朝向什么的时候看情况
                */
                break;
            case EffectType.enableObject:
                // Enable the target object
                targetObject.SetActive(true);
                break;
            case EffectType.disableObject:
                // Disable the target object
                targetObject.SetActive(false);
                break;
            case EffectType.enableComponent:
                // Enable the target component
                targetComponent.enabled = true;
                break;
            case EffectType.disableComponent:
                // Disable the target component
                targetComponent.enabled = false;
                break;
        }
    }

    public void RevertEffect()
    {
        switch (effectType)
        {
            case EffectType.changePosition:
                // Revert the position of the target object
                targetObject.transform.position = reservePosition;
                break;
            case EffectType.enableObject:
                // Disable the target object
                targetObject.SetActive(false);
                break;
            case EffectType.disableObject:
                // Enable the target object
                targetObject.SetActive(true);
                break;
            case EffectType.enableComponent:
                // Disable the target component
                targetComponent.enabled = false;
                break;
            case EffectType.disableComponent:
                // Enable the target component
                targetComponent.enabled = true;
                break;
        }
    }
}
public enum EffectType
{
    changePosition,
    enableObject,
    disableObject,
    enableComponent,
    disableComponent,
}