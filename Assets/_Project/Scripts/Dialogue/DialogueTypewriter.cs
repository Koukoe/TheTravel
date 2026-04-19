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

    public bool IsTyping { get; private set; }

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
        onTypingCompleted = null;
    }

    private IEnumerator TypeRoutine()
    {
        for (int i = 0; i < fullContent.Length; i++)
        {
            if (targetText != null)
            {
                targetText.text += fullContent[i];
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
        InvokeCompleted();
    }

    private void InvokeCompleted()
    {
        Action callback = onTypingCompleted;
        onTypingCompleted = null;
        callback?.Invoke();
    }
}
