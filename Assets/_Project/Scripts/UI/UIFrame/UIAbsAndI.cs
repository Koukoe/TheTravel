using System;
using UnityEngine;

public abstract class UIListener : MonoBehaviour
{
    public abstract void Open();
    public abstract void Resume();
    public abstract void Close(Action onFinished);
    public abstract void Suspend(Action onFinished);

    public abstract void Abort();

    protected int _suspendStyle = 0;
    protected virtual int StyleListCount => 1;
    public virtual int SuspendStyle
    {
        get => _suspendStyle;
        set => _suspendStyle = Mathf.Clamp(value, 0, StyleListCount - 1);
    }
}

public interface IUIAppearanceSource
{
    bool IsProvider { get; }
    Vector3 PosOffset { get; }
    Vector3 AngleOffset { get; }
    Vector3 ScaleMult { get; }
    float AlphaMult { get; }
}