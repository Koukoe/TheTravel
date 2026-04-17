using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5.0f;

    private void Awake()
    {
    }

    private void OnEnable()
    {
        InputManager.Instance.PlayerDynamicActions.Interact.performed += Interact;
    }

    private void OnDisable()
    {
        InputManager.Instance.PlayerDynamicActions.Interact.performed -= Interact;
    }

    void Update()
    {
        Vector2 inputVector = InputManager.Instance.GetMove();
        Vector3 movement = new Vector3(inputVector.x, 0, inputVector.y);

        transform.position += movement * moveSpeed * Time.deltaTime;
    }

    private void Interact(InputAction.CallbackContext context) { }
}
