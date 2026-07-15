using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

// 挂载在水体上，为落水物体生成波浪，且可查询指定坐标处的水深
public class WaterController : MonoBehaviour
{
    private Material _waterMat;
    
    // 传给shader的键名，提前hash可略微优化性能
    private static readonly int DiffWavesID = Shader.PropertyToID("_DiffWaves");
    private static readonly int DiffWavesCountID = Shader.PropertyToID("_DiffWavesCount");
    
    // 计算扩散波持续时间的参数
    private float _diffPower;
    private float _disTime;
    
    // 计算水波高度的参数
    private float _waveHeight;
    private float _waveLength;
    private float _waveSpeed;
    
    // 扩散水波结构体，对应shader中的DiffWave
    private struct DiffWave
    {
        public Vector2 pos;
        public float power;
        public float startTime;
    };
    
    private ComputeBuffer _waveBuffer;
    private List<DiffWave> _waves = new();
    
    private void Awake()
    {
        _waterMat = GetComponent<Renderer>().material;
        _diffPower = _waterMat.GetFloat("_DiffPower");
        _disTime = _waterMat.GetFloat("_DisTime");
        _waveHeight = _waterMat.GetFloat("_WaveHeight");
        _waveLength = _waterMat.GetFloat("_WaveLength");
        _waveSpeed = _waterMat.GetFloat("_WaveSpeed");
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
        if (rb == null) return;
        
        var v2 = rb.velocity.sqrMagnitude;
        var m = rb.mass;
        var p = Mathf.Sqrt(v2 * m);
        var pos = other.transform.position;
        var posInSelf =  transform.InverseTransformPoint(pos);
        
        SetMat(posInSelf, p);
    }
    
    IEnumerator DelayedRemove(DiffWave ndf, float delay)
    {
        yield return new WaitForSeconds(delay);
        _waves.Remove(ndf);
    }

    private void SetMat(Vector3 pos, float power)
    {
        // 填充数据
        var ndf = new DiffWave()
        {
            pos = new Vector2(pos.x, pos.z),
            power = power,
            startTime = Time.time,
        };
        _waves.Add(ndf);
        // 波结束时移除
        StartCoroutine(DelayedRemove(ndf, power * _diffPower * _disTime));
        int count = _waves.Count;

        // 计算单个结构体字节大小
        int stride = Marshal.SizeOf(typeof(DiffWave));

        // 创建ComputeBuffer
        _waveBuffer?.Release();
        _waveBuffer = new(count, stride);
        _waveBuffer.SetData(_waves);

        // 设置材质参数
        _waterMat.SetBuffer(DiffWavesID, _waveBuffer);
        _waterMat.SetInt(DiffWavesCountID, count);
    }

    // 释放资源
    private void OnDestroy()
    {
        _waveBuffer?.Release();
    }
    
    // 获取指定点的水面高度（与shader一致的算法）
    public float GetWaveHeight(Vector2 pos)
    {
        float x = pos.x;
        float z = pos.y;
        float t = Time.time;

        // ---- 1. 基础波 ----
        float height = 0f;

        height += Mathf.Sin((x + z) / _waveLength + t * _waveSpeed) * _waveHeight;
        height += Mathf.Sin((x * 1f + z * 2f) / _waveLength + t * 2f * _waveSpeed) * 0.2f * _waveHeight;
        height += Mathf.Sin((x * 0.1f + z * 1.2f) / _waveLength + t * 3f * _waveSpeed) * 0.1f * _waveHeight;

        // ---- 2. 扩散波 ----
        if (_waves != null)
        {
            float diffWaveSpeed = 10f;

            for (int i = 0; i < _waves.Count; i++)
            {
                DiffWave fw = _waves[i];

                float wavePower = fw.power * _diffPower;
                float dt = t - fw.startTime;

                float Dist = diffWaveSpeed * dt + wavePower * 0.2f;

                float dist = Vector2.Distance(new Vector2(x, z), fw.pos);

                if (dist <= Dist)
                {
                    float d = Dist - dist;

                    float maxTime = wavePower * _disTime;
                    float p = 1f - Mathf.Clamp01(dt / maxTime);

                    float maxRange = wavePower * 3f;

                    if (d <= maxRange)
                    {
                        float inner = 1f - d / maxRange;
                        float deltaY = Mathf.Sin(d * 3f / wavePower) * wavePower * inner * p * 0.4f;
                        height += deltaY;
                    }
                }
            }
        }

        return height;
    }
}
