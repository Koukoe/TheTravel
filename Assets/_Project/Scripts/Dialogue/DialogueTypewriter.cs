using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueTypewriter
{
    private readonly MonoBehaviour host;
    private Coroutine typingRoutine;
    private TMP_Text targetText;
    private string fullContent = string.Empty;
    private float charInterval = 0.03f;
    private Action onTypingCompleted;
    private int nextCharIndex;

    public bool IsTyping { get; private set; }
    public bool IsPaused { get; private set; }

    public DialogueTypewriter(MonoBehaviour coroutineHost)
    {
        host = coroutineHost;
    }

    public void Configure(TMP_Text text, float interval)
    {
        targetText = text;
        charInterval = Mathf.Max(0f, interval);
    }

    public void Play(string content, Action onCompleted)
    {
        Stop();

        fullContent = content ?? string.Empty;
        onTypingCompleted = onCompleted;
        nextCharIndex = 0;
        IsPaused = false;

        if (targetText == null)
        {
            IsTyping = false;
            InvokeCompleted();
            return;
        }

        if (fullContent.Length == 0)
        {
            targetText.text = string.Empty;
            IsTyping = false;
            InvokeCompleted();
            return;
        }

        targetText.text = string.Empty;
        IsTyping = true;
        typingRoutine = host.StartCoroutine(TypeRoutine());
    }

    public void Pause()
    {
        if (!IsTyping)
        {
            return;
        }

        if (typingRoutine != null)
        {
            host.StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        IsTyping = false;
        IsPaused = true;
    }

    public void Resume()
    {
        if (!IsPaused)
        {
            return;
        }

        IsPaused = false;
        IsTyping = true;
        typingRoutine = host.StartCoroutine(TypeRoutine());
    }

    public void CompleteNow()
    {
        if (!IsTyping)
        {
            return;
        }

        if (typingRoutine != null)
        {
            host.StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        if (targetText != null)
        {
            targetText.text = fullContent;
        }

        IsTyping = false;
        IsPaused = false;
        nextCharIndex = fullContent.Length;
        InvokeCompleted();
    }

    public void Stop()
    {
        if (typingRoutine != null)
        {
            host.StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        IsTyping = false;
        IsPaused = false;
        nextCharIndex = 0;
        onTypingCompleted = null;
    }

    private IEnumerator TypeRoutine()
    {
        for (int i = nextCharIndex; i < fullContent.Length; i++)
        {
            if (targetText != null)
            {
                targetText.text += fullContent[i];
            }

            nextCharIndex = i + 1;

            if (nextCharIndex >= fullContent.Length)
            {
                break;
            }

            if (charInterval > 0f)
            {
                yield return new WaitForSeconds(charInterval);
            }
            else
            {
                yield return null;
            }
        }

        typingRoutine = null;
        IsTyping = false;
        IsPaused = false;
        nextCharIndex = fullContent.Length;
        InvokeCompleted();
    }

    private void InvokeCompleted()
    {
        Action callback = onTypingCompleted;
        onTypingCompleted = null;
        callback?.Invoke();
    }
}
