using System;
using UnityEngine;

public abstract class UIListener : MonoBehaviour, IUIListener
{
    public abstract void Open();
    public abstract void Resume();
    public abstract void Close(Action onFinished);
    public abstract void Suspend(Action onFinished);
    public abstract void Abort();
}

public interface IUIListener
{
    void Open();
    void Resume();
    void Close(Action onFinished);
    void Suspend(Action onFinished);

    void Abort();
}