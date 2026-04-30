using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 控制角色行为，旋转，移动
/// </summary>
public class TestPlayerController : MonoBehaviour
{
    public float walkSpeed = 2;
    public float runSpeed = 6;

    private CharacterController characterController;
    private Animator animator;

    private TestPlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new TestPlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.TestPlayer.Enable();
    }

    private void OnDisable()
    {
        inputActions.TestPlayer.Disable();
    }
    void Start()
    {
        characterController= GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        //获取移动方向 
        Vector2 input = inputActions.TestPlayer.Move.ReadValue<Vector2>();
        //判断是否奔跑
        bool isRunning = inputActions.TestPlayer.Run.IsPressed();

        Vector2 inputDir = input.normalized;
       
        //输入控制角色旋转移动
        if (inputDir != Vector2.zero)
        {
            transform.eulerAngles = Vector3.up * (Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg);
        }

        float speed = (isRunning ? runSpeed : walkSpeed) * inputDir.magnitude;
        Vector3 velocity = transform.forward * speed;
        characterController.Move(velocity*Time.deltaTime);

        //控制动画播放
        float animationSpeedPercent = ((isRunning) ? 1.0f : 0.5f) * inputDir.magnitude;
        animator.SetFloat("SpeedPercent", animationSpeedPercent);
    }
}
