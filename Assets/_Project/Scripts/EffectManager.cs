using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    [SerializeField] private Volume blurVolume;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }

    public void SetBackgroundBlur(bool enable)
    {
        float target = enable ? 1f : 0f;

        StopAllCoroutines();
        StartCoroutine(FadeBlur(target));
    }

    private System.Collections.IEnumerator FadeBlur(float targetWeight)
    {
        float startWeight = blurVolume.weight;
        float time = 0;
        while (time < 0.5f)
        {
            time += Time.deltaTime;
            blurVolume.weight = Mathf.Lerp(startWeight, targetWeight, time / 0.5f);
            yield return null;
        }
        blurVolume.weight = targetWeight;
    }
}
