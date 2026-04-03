using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class BasePanel : MonoBehaviour
{
    [SerializeField] private CanvasRender canvasRenderMode = CanvasRender.SYS2D;
    public CanvasRender CanvasRenderMode => canvasRenderMode;

    public float delay = 0f;

    private bool _isTransitioning = false;
    private Coroutine _delayRoutine;

    [SerializeReference] public UIListener[] _listeners;

    protected virtual void Awake() { }

    public virtual void Open(Action onAllFinished = null)
    {
        Abort();
        _isTransitioning = true;

        gameObject.SetActive(true);

        foreach (var listener in _listeners)
        {
            var mono = listener as MonoBehaviour;
            if (mono != null && mono.gameObject.activeInHierarchy) listener.Open();
        }

        _delayRoutine = StartCoroutine(ExcuteAfterDelay(() =>
        {
            _isTransitioning = false;
            _delayRoutine = null;
            OnOpen();
            onAllFinished?.Invoke();
        }));
    }

    public virtual void Resume(Action onAllFinished = null)
    {
        Abort();
        _isTransitioning = true;

        foreach (var listener in _listeners)
        {
            var mono = listener as MonoBehaviour;
            if (mono != null && mono.gameObject.activeInHierarchy) listener.Resume();
        }

        _delayRoutine = StartCoroutine(ExcuteAfterDelay(() =>
        {
            _isTransitioning = false;
            _delayRoutine = null;
            OnResume();
            onAllFinished?.Invoke();
        }));
    }

    public virtual void Close(Action onAllFinished = null)
    {
        Abort();
        _isTransitioning = true;

        ExecuteWithCallback((l, done) => l.Close(done), () =>
        {
            _isTransitioning = false;
            OnClose();
            gameObject.SetActive(false);
            onAllFinished?.Invoke();
        });
    }

    public virtual void Suspend(Action onAllFinished = null)
    {
        Abort();
        _isTransitioning = true;

        ExecuteWithCallback((l, done) => l.Suspend(done), () =>
        {
            _isTransitioning = false;
            OnSuspend();
            onAllFinished?.Invoke();
        });
    }

    private void ExecuteWithCallback(Action<UIListener, Action> actionRef, Action on)
    {
        // 无监听组件
        if (_listeners == null || _listeners.Length == 0)
        {
            _isTransitioning = false;
            on?.Invoke();
            return;
        }

        int total = _listeners.Length;
        int completedCount = 0;

        Action onItemDone = () =>
        {
            completedCount++;

            if (completedCount >= total && _isTransitioning) { on?.Invoke(); }
        };

        foreach (var listener in _listeners)
        {
            var mono = listener as MonoBehaviour;
            if (mono == null || !mono.gameObject.activeInHierarchy)
            {
                onItemDone();
                continue;
            }
            actionRef(listener, onItemDone);
        }
    }


    private IEnumerator ExcuteAfterDelay(Action on)
    {
        if (delay > 0)
            yield return new WaitForSecondsRealtime(delay);
        else
            yield return null;

        on?.Invoke();
    }

    public void Abort(bool cleanListeners = false)
    {
        if (!_isTransitioning) return;

        // 停止协程
        if (_delayRoutine != null)
        {
            StopCoroutine(_delayRoutine);
            _delayRoutine = null;
        }

        // 表现层打断
        if (cleanListeners)
        {
            foreach (var listener in _listeners)
            {
                var mono = listener as MonoBehaviour;
                if (mono != null && mono.gameObject.activeInHierarchy) listener.Abort(); // 强制停止
            }
        }

        _isTransitioning = false;
    }


    public virtual void OnOpen() { }
    public virtual void OnClose() { }
    public virtual void OnSuspend() { }
    public virtual void OnResume() { }

}