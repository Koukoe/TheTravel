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
        InputManager.Instance.PlayerActions.Interact.performed += Interact;
    }

    private void OnDisable()
    {
        InputManager.Instance.PlayerActions.Interact.performed -= Interact;
    }

    void Update()
    {
        Vector2 inputVector = InputManager.Instance.GetMove();
        Vector3 movement = new Vector3(inputVector.x, inputVector.y, 0);

        transform.position += movement * moveSpeed * Time.deltaTime;
    }

    private void Interact(InputAction.CallbackContext context) { }
}
