using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class Playermove : MonoBehaviour
{
    public static Playermove Instance { get; private set; }
    public Transform PlayerTransform;
    public Rigidbody PlayerRigidbody;
    public Transform shipTransform;
    public GameObject ship;

    public GameObject playerModel;

    public SphereCollider dectector;

    public List<Collider> seaOffColliders = new List<Collider>();
    public List<Collider> seaOnColliders = new List<Collider>();
    [SerializeField] private Animator _animator;

    [Header("移动参数")]
    public float speed = 1f;
    public float shipSpeed = 1f;
    [Header("旋转参数")]
    public float PlayerRotateSpeed = 1f;
    public float ShipRotateSpeed = 1f;

    [SerializeField] private bool Onsea = false;

    public bool OnSea
    {
        get { return Onsea; }
        set
        {
            Onsea = value;
            PlayerController.mainCam.GetComponent<CameraMove>().changeSeaCam(value);
            changeState();
        }
    }

    void Start()
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
        if (PlayerTransform == null) PlayerTransform = GetComponent<Transform>();
        if (PlayerRigidbody == null) PlayerRigidbody = GetComponent<Rigidbody>();
        if (_animator == null) _animator = GetComponent<Animator>();
        if (ship == null) ship = GameObject.FindWithTag("ship");
        else
        {
            shipTransform = ship.transform;
        }
    }

    Vector3 GetMoveInput()
    {
        Vector2 inputVector = InputManager.Instance.GetMove();

        // Debug.Log(inputVector.x + "," + inputVector.y);
        if (inputVector == Vector2.zero) return Vector3.zero;

        Vector3 Camforward = PlayerController.mainCam.transform.forward;
        Vector3 Camright = PlayerController.mainCam.transform.right;
        Camforward.y = 0;
        Camright.y = 0;
        Camforward.Normalize();
        Camright.Normalize();
        Vector3 res = Camright * inputVector.x + Camforward * inputVector.y;
        return res.normalized;
    }

    private void PlayerMove()
    {
        Quaternion targetRotation;
        Vector3 input = GetMoveInput();

        float m = input.magnitude;
        _animator.SetFloat("moveAmount", m, 0.1f, Time.deltaTime);

        if (input != Vector3.zero)
        {

            targetRotation = Quaternion.LookRotation(input);
            PlayerTransform.rotation = Quaternion.RotateTowards(PlayerTransform.rotation, targetRotation, PlayerRotateSpeed);
        }

        // PlayerRigidbody.velocity = input * speed;
        Vector3 Velocity = PlayerRigidbody.velocity;
        Velocity.x = (input * speed).x;
        Velocity.z = (input * speed).z;
        PlayerRigidbody.velocity = Velocity;
    }

    private void ShipMove()
    {
        Quaternion targetRotation;
        Vector3 input = GetMoveInput();
        PlayerRigidbody.velocity = input * shipSpeed;

        if (input != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(input);
            PlayerTransform.rotation = Quaternion.RotateTowards(PlayerTransform.rotation, targetRotation, ShipRotateSpeed);
        }

        shipTransform.position = PlayerTransform.position;
        shipTransform.rotation = PlayerTransform.rotation;
    }

    public void Move()
    {
        if (Onsea)
        {
            ShipMove();
        }
        else
        {
            PlayerMove();
        }
    }

    private void changeState()
    {
        if (Onsea)
        {
            dectector.radius = 50f;
            foreach (var collidor in seaOffColliders)
            {
                collidor.enabled = false;
            }
            foreach (var collidor in seaOnColliders)
            {
                collidor.enabled = true;
            }

            playerModel.SetActive(false);
            PlayerRigidbody.useGravity = false;
            PlayerRigidbody.constraints &= ~RigidbodyConstraints.FreezePositionY;
        }
        else
        {
            dectector.radius = 2.5f;
            foreach (var collidor in seaOffColliders)
            {
                collidor.enabled = true;
            }
            foreach (var collidor in seaOnColliders)
            {
                collidor.enabled = false;
            }

            playerModel.SetActive(true);
            PlayerRigidbody.useGravity = true;
            PlayerRigidbody.constraints |= RigidbodyConstraints.FreezePositionY;
        }
    }

    //调试
    void OnDrawGizmos()
    {
        if (PlayerTransform == null) return;

        // 绘制玩家前方向量（绿色）
        Gizmos.color = Color.green;
        Gizmos.DrawRay(PlayerTransform.position, PlayerTransform.forward * 2f);

        // 绘制玩家右方向量（红色）
        Gizmos.color = Color.red;
        Gizmos.DrawRay(PlayerTransform.position, PlayerTransform.right * 1.5f);

        // 绘制玩家上方向量（蓝色）
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(PlayerTransform.position, PlayerTransform.up * 1f);

        // 可选：绘制一个球体标记中心点
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(PlayerTransform.position, 0.3f);
    }
}
