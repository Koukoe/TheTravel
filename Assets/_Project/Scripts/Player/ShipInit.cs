using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipInit : MonoBehaviour
{
    public static ShipInit Instance { get; private set; }
    public Transform shipTransform;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        shipTransform = transform;
    }
}
