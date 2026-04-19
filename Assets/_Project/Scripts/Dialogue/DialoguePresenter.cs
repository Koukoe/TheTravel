using System;
using TMPro;
using UnityEngine;

public class DialoguePresenter
{
    private readonly DialogueTypewriter typewriter;
    private readonly DialogueUIController uiController;
    private readonly Func<string, string> resolveCharacterName;

    private TMP_Text nameText;
    private TMP_Text contentText;

    public DialoguePresenter(DialogueTypewriter typewriter, DialogueUIController uiController, Func<string, string> resolveCharacterName)
    {
        this.typewriter = typewriter;
        this.uiController = uiController;
        this.resolveCharacterName = resolveCharacterName;
    }

    public bool Bind(Transform root, string nameNodeName, string contentNodeName)
    {
        if (root == null)
        {
            nameText = null;
            contentText = null;
            return false;
        }

        nameText = FindTextByNodeName(root, nameNodeName);
        contentText = FindTextByNodeName(root, contentNodeName);
        return nameText != null && contentText != null;
    }

    public void ShowEntry(DialogueEntry entry, float charInterval)
    {
        if (entry == null)
        {
            return;
        }

        uiController?.HideOptionsPanelIfOpened();
        UpdateName(entry.character);

        string content = entry.content ?? string.Empty;
        if (typewriter != null)
        {
            typewriter.Configure(contentText, Mathf.Max(0f, charInterval));
            typewriter.Play(content, () => ShowOptionsAfterTyping(entry));
            return;
        }

        if (contentText != null)
        {
            contentText.text = content;
        }

        ShowOptionsAfterTyping(entry);
    }

    public void Clear()
    {
        if (contentText != null)
        {
            contentText.text = string.Empty;
        }

        if (nameText != null)
        {
            nameText.text = string.Empty;
        }
    }

    private void ShowOptionsAfterTyping(DialogueEntry entry)
    {
        if (entry == null)
        {
            return;
        }

        uiController?.RefreshOptionsPanel(entry);
    }

    private void UpdateName(string charID)
    {
        if (nameText == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(charID))
        {
            nameText.text = string.Empty;
            return;
        }

        string displayName = resolveCharacterName != null ? resolveCharacterName(charID) : null;
        if (!string.IsNullOrEmpty(displayName))
        {
            nameText.text = displayName;
            return;
        }

        nameText.text = charID;
        Debug.LogWarning($"未找到角色 ID 为 {charID} 的配置");
    }

    private TMP_Text FindTextByNodeName(Transform root, string nodeName)
    {
        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].gameObject.name == nodeName)
            {
                return texts[i];
            }
        }

        return null;
    }
}
