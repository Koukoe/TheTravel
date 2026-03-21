using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    [Serializable]
    public struct PoolSettings
    {
        public string name;
        public GameObject prefab;
        public int prewarmCount;
        public int maxSize;
    }

    public bool isGlobal;
    [SerializeField] private List<PoolSettings> configs;

    private Dictionary<string, PoolSettings> _lib;
    private Dictionary<GameObject, IObjectPool<GameObject>> _pools;
    private Dictionary<GameObject, IObjectPool<GameObject>> _instanceMap;

    public static PoolManager Global;
    public static PoolManager Scene;

    private void Awake()
    {
        int count = configs.Count;

        _lib = new Dictionary<string, PoolSettings>(count);
        _pools = new Dictionary<GameObject, IObjectPool<GameObject>>(count);

        int totalCapacity = 0;
        for (int i = 0; i < configs.Count; i++)
        {
            totalCapacity += Mathf.Max(configs[i].prewarmCount, 4);
        }
        _instanceMap = new Dictionary<GameObject, IObjectPool<GameObject>>(totalCapacity);

        if (isGlobal)
        {
            if (Global != null) { Destroy(gameObject); return; }
            Global = this; DontDestroyOnLoad(gameObject);
        }
        else
        {
            Scene = this;
        }

        // 转化 configs 为 _lib
        for (int i = 0; i < configs.Count; i++)
        {
            var c = configs[i];
            if (c.prefab != null && !string.IsNullOrEmpty(c.name)) _lib[c.name] = c;
        }
    }

    private void Start()
    {
        for (int i = 0; i < configs.Count; i++)
            if (configs[i].prewarmCount > 0) Prewarm(configs[i].name, configs[i].prewarmCount);
    }

    /// <summary>
    /// 根据 Name 从池中获取一个对象。
    /// 如果对应的池子尚未建立，则会自动根据配置表初始化。
    /// </summary>
    /// <param name="n">在 Inspector 清单中定义的唯一标识符名字</param>
    /// <returns>返回一个激活的对象实例；若未注册则返回 null</returns>
    public GameObject Get(string n)
    {
        if (!_lib.TryGetValue(n, out var conf)) return null;

        if (!_pools.TryGetValue(conf.prefab, out var pool))
        {
            pool = new ObjectPool<GameObject>(
                () => Instantiate(conf.prefab, transform),
                obj => obj.SetActive(true),
                obj => obj.SetActive(false),
                obj =>
                {
                    _instanceMap.Remove(obj);
                    Destroy(obj);
                },
                false,
                conf.prewarmCount > 0 ? conf.prewarmCount : 8,
                conf.maxSize > 0 ? conf.maxSize : 128
            );
            _pools.Add(conf.prefab, pool);
        }
        var inst = pool.Get();
        _instanceMap[inst] = pool;
        return inst;
    }

    /// <summary>
    /// 自动识别并归还对象到对应的池子（优先匹配场景池）。
    /// 如果对象不属于任何池子，则直接物理销毁。
    /// </summary>
    /// <param name="obj">需要回收的 GameObject 实例</param>
    public static void Release(GameObject obj)
    {
        if (obj == null) return;

        if (Scene != null && Scene._instanceMap.TryGetValue(obj, out var sPool))
        {
            sPool.Release(obj);
        }
        else if (Global != null && Global._instanceMap.TryGetValue(obj, out var gPool))
        {
            gPool.Release(obj);
        }
        else
        {
            Destroy(obj);  // 找不到对应池则销毁
        }
    }

    public void Prewarm(string n, int c)
    {
        if (c <= 0) return;

        for (int i = 0; i < c; i++)
        {
            _prewarmList.Add(Get(n));
        }

        for (int i = 0; i < _prewarmList.Count; i++)
        {
            Release(_prewarmList[i]);
        }

        _prewarmList.Clear();
    }

    // 预分配空间的私有列表
    private readonly List<GameObject> _prewarmList = new List<GameObject>(128);
}