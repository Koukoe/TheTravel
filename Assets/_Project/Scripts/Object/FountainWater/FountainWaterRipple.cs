using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FountainWaterRipple : MonoBehaviour
{
    public static FountainWaterRipple Instance { get; private set; }
    [Header("调试功能")]
    public bool isDebug = false;
    [Header("Ripple Settings")]
    [SerializeField] bool isRippling = false;
    [SerializeField] Vector3 rippleCenter;
    [SerializeField] float rippleDuration = 1f;

    private Material material;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("多个 FountainWaterRipple 实例，已销毁重复的");
            Destroy(gameObject);
        }
    }
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        material = renderer.material;

        material.SetVector("_RippleCenter", rippleCenter);
        material.SetFloat("_RippleTime", 0);
        material.SetInt("_RippleOn", isRippling ? 1 : 0);
    }

    IEnumerator OnRippling(float time)
    {
        float curTime = 0f;
        while (curTime < time)
        {
            curTime += Time.deltaTime;
            material.SetFloat("_RippleTime", curTime);
            yield return null;
        }
        isRippling = false;
        material.SetInt("_RippleOn", isRippling ? 1 : 0);
        material.SetFloat("_RippleTime", 0);
    }
    private void StartRipple()
    {
        if (isRippling) return;
        isRippling = true;
        material.SetInt("_RippleOn", isRippling ? 1 : 0);
        StartCoroutine(OnRippling(rippleDuration));
    }

    public static void CreateRipple()
    {
        if (Instance != null)
        {
            Instance.StartRipple();
        }
    }

    //Debug

    void Update()
    {
        if (isDebug)
        {
            StartRipple();
        }
    }

    void OnDrawGizmos()
    {
        // 只在选中物体或开启调试时显示
        if (!isDebug && !UnityEditor.Selection.activeGameObject == gameObject)
            return;

        // 绘制中心点
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(rippleCenter, 0.1f);

        // 绘制半径范围
        Gizmos.color = new Color(0, 1, 1, 0.3f);  // 半透明青色
        Gizmos.DrawWireSphere(rippleCenter, 1f);  // 1米半径的参考圆

        // 绘制连接线（从物体位置到涟漪中心）
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, rippleCenter);

        // 添加文字标签（需要 Handles API，见下方升级版）
    }
}
