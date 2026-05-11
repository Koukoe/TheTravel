using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    [SerializeField] private Volume blurVolume;
    [SerializeField] private CanvasGroup sceneTrans;

    private void Awake()
    {
        Instance = this;
    }

    public void SetBackgroundBlur(bool enable)
    {
        float target = enable ? 1f : 0f;

        StopAllCoroutines();
        StartCoroutine(FadeBlur(target));
    }

    public async UniTask FadeOut(float duration = 0.25f)
    {
        sceneTrans.gameObject.SetActive(true);
        await sceneTrans.DOFade(1f, duration).AsyncWaitForCompletion();
    }

    public async UniTask FadeIn(float duration = 0.25f)
    {
        await sceneTrans.DOFade(0f, duration).AsyncWaitForCompletion();
        sceneTrans.gameObject.SetActive(false);
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
