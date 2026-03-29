using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIAnimation : MonoBehaviour
{
    public float duration = 0.5f;
    public EaseParam ease;

    // 核心接口
    public abstract void PlayEnter(System.Action onComplete);
    public abstract void PlayExit(System.Action onComplete);
}