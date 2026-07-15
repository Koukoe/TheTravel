using UnityEngine;

// 摄像机脚本，用于查看反射效果。wasd移动，左键上升，右键下降
public class SimpleFPSController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;          // 水平移动速度

    [Header("视角设置")]
    public float mouseSensitivity = 2f;   // 鼠标灵敏度
    public float verticalClamp = 80f;     // 俯仰角限制（度）

    [Header("垂直移动")]
    public float verticalSpeed = 5f;      // 上升/下降速度

    private float rotationX = 0f;         // 俯仰角度
    private float rotationY = 0f;         // 偏航角度

    void Start()
    {
        // 锁定并隐藏鼠标光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ---------- 视角旋转 ----------
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -verticalClamp, verticalClamp);
        rotationY += mouseX;

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);

        // ---------- WASD 水平移动 ----------
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = (transform.forward * vertical + transform.right * horizontal).normalized;
        move.y = 0f;

        transform.position += move * moveSpeed * Time.deltaTime;

        // ---------- 鼠标左键上升 / 右键下降 ----------
        if (Input.GetMouseButton(0)) // 左键按住上升
        {
            transform.position += Vector3.up * verticalSpeed * Time.deltaTime;
        }
        if (Input.GetMouseButton(1)) // 右键按住下降
        {
            transform.position += Vector3.down * verticalSpeed * Time.deltaTime;
        }
    }
}