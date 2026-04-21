using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5.0f;

    public static Camera mainCam;
    public static PlayerController Instance { get; private set; }

    private bool shipMove;  // 先放着

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
        InputManager.Instance.PlayerDynamicActions.Look.performed += Look;
        InputManager.Instance.PlayerDynamicActions.Zoom.performed += Zoom;
        InputManager.Instance.PlayerStaticActions.Book.performed += Book;
    }

    private void OnDisable()
    {
        InputManager.Instance.PlayerDynamicActions.Interact.performed -= Interact;
        InputManager.Instance.PlayerDynamicActions.Look.performed -= Look;
        InputManager.Instance.PlayerDynamicActions.Zoom.performed -= Zoom;
        InputManager.Instance.PlayerStaticActions.Book.performed -= Book;
    }

    void Update()
    {
        Vector2 inputVector = InputManager.Instance.GetMove();
        Vector3 movement = new Vector3(inputVector.x, 0, inputVector.y);

        transform.position += movement * moveSpeed * Time.deltaTime;
    }

    private void Interact(InputAction.CallbackContext context) { }

    private void Look(InputAction.CallbackContext context) { }

    private void Zoom(InputAction.CallbackContext context) { }

    private void Book(InputAction.CallbackContext context)
    {
        if (InputManager.Instance.UIActions.enabled) { UIManager.Instance.Pop(); }
        else
        {
            UIManager.Instance.Push("BookPanel");
            InputManager.Instance.SwitchUIMode(false);
        }

    }
}
