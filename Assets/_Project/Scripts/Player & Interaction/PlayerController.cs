using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    public InteractionDetector detector;
    public static PlayerController Instance { get; private set; }

    public bool moveEnable = true;
    public static Camera mainCam;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        InputManager.Instance.PlayerDynamicActions.Interact.performed += Interact;
        InputManager.Instance.PlayerStaticActions.Book.performed += Book;
        moveEnable = true;
    }

    private void OnDisable()
    {
        InputManager.Instance.PlayerDynamicActions.Interact.performed -= Interact;
        InputManager.Instance.PlayerStaticActions.Book.performed -= Book;
        moveEnable = false;
    }

    private void Interact(InputAction.CallbackContext context)
    {
        detector.GetTarget()?.DoInteract();
    }

    private void Book(InputAction.CallbackContext context)
    {
        if (InputManager.Instance.UIActions.enabled) { UIManager.Instance.Pop(); }
        else
        {
            UIManager.Instance.Push("BookPanel");
            InputManager.Instance.SwitchUIMode(false);
        }
    }

    void Update()
    {
        if (moveEnable) Playermove.Instance.Move();
    }
}
