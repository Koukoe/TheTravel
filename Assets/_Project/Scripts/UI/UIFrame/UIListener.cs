using System;
using UnityEngine;

public abstract class UIListener : MonoBehaviour
{
    public bool needCallback = true;

    public abstract void Open();
    public abstract void Resume();
    public abstract void Close(Action onFinished);
    public abstract void Suspend(Action onFinished);
    public abstract void Abort();
}