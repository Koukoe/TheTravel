using System;
using System.Collections;
using UnityEngine;

public class DialogueUIController
{
    private readonly MonoBehaviour host;

    private Coroutine openRoutine;
    private Coroutine closeRoutine;

    private const string DialoguePanelName = "DialoguePanel";
    private const string DialogueOptionsPanelName = "DialogueOptionsPanel";

    public DialogueUIController(MonoBehaviour coroutineHost)
    {
        host = coroutineHost;
    }

    public void OpenDialoguePanelWithCleanup(Action<BasePanel> onReady)
    {
        if (openRoutine != null)
        {
            host.StopCoroutine(openRoutine);
            openRoutine = null;
        }

        openRoutine = host.StartCoroutine(OpenDialoguePanelRoutine(onReady));
    }

    public void CloseDialoguePanels()
    {
        if (closeRoutine != null)
        {
            host.StopCoroutine(closeRoutine);
            closeRoutine = null;
        }

        closeRoutine = host.StartCoroutine(CloseDialoguePanelsRoutine());
    }

    public void RefreshOptionsPanel(DialogueEntry entry)
    {
        bool hasOptions = entry != null && entry.options != null && entry.options.Count > 0;
        if (!hasOptions)
        {
            HideOptionsPanelIfOpened();
            return;
        }

        BasePanel panel = UIManager.Instance.Peek();
        if (!(panel is DialogueOptionsPanel))
        {
            panel = UIManager.Instance.Push(DialogueOptionsPanelName);
        }

        if (panel is DialogueOptionsPanel optionsPanel)
        {
            optionsPanel.RefreshOptions();
        }
    }

    public void HideOptionsPanelIfOpened()
    {
        if (UIManager.Instance != null && UIManager.Instance.Peek() is DialogueOptionsPanel)
        {
            UIManager.Instance.Pop();
        }
    }

    public void StopAll()
    {
        if (openRoutine != null)
        {
            host.StopCoroutine(openRoutine);
            openRoutine = null;
        }

        if (closeRoutine != null)
        {
            host.StopCoroutine(closeRoutine);
            closeRoutine = null;
        }
    }

    private IEnumerator OpenDialoguePanelRoutine(Action<BasePanel> onReady)
    {
        while (UIManager.Instance != null && UIManager.Instance.IsTransitioning)
        {
            yield return null;
        }

        yield return CleanupResidualDialoguePanels();

        BasePanel panel = UIManager.Instance.Peek();
        if (!(panel is DialoguePanel))
        {
            panel = UIManager.Instance.Push(DialoguePanelName);
        }

        while (UIManager.Instance != null && UIManager.Instance.IsTransitioning)
        {
            yield return null;
        }

        openRoutine = null;
        onReady?.Invoke(panel);
    }

    private IEnumerator CleanupResidualDialoguePanels()
    {
        if (UIManager.Instance == null)
        {
            yield break;
        }

        while (UIManager.Instance.Peek() is DialogueOptionsPanel || UIManager.Instance.Peek() is DialoguePanel)
        {
            UIManager.Instance.Pop();

            while (UIManager.Instance.IsTransitioning)
            {
                yield return null;
            }
        }
    }

    private IEnumerator CloseDialoguePanelsRoutine()
    {
        if (UIManager.Instance == null)
        {
            yield break;
        }

        if (UIManager.Instance.Peek() is DialogueOptionsPanel)
        {
            UIManager.Instance.Pop();
        }

        while (UIManager.Instance.IsTransitioning)
        {
            yield return null;
        }

        if (UIManager.Instance.Peek() is DialoguePanel)
        {
            UIManager.Instance.Pop();
        }

        closeRoutine = null;
    }
}
