using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AmbSourceObject : MonoBehaviour
{
    [SerializeField] private string ambName;
    [SerializeField] private bool is3D = true;
    private GameObject _cache;

    public void PlayAmbient()
    {
        if (is3D)
        {
            Vector3 worldPos = transform.position;
            _cache = AudioManager.Instance.PlayAmbient(ambName, worldPos);
        }
        else
        {
            _cache = AudioManager.Instance.PlayAmbient(ambName);
        }

    }

    public void StopAmbient()
    {
        AudioManager.Instance.StopAmbient(_cache);
    }
}
