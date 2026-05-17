using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXSourceObject : MonoBehaviour
{
    [SerializeField] private string sfxName;
    [SerializeField] private bool is3D;

    public void PlayAmbient()
    {
        if (is3D)
        {
            Vector3 worldPos = transform.position;
            AudioManager.Instance.PlaySFX(sfxName, worldPos);
        }
        else
        {
            AudioManager.Instance.PlaySFX(sfxName);
        }

    }
}
