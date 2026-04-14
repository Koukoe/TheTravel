using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueOptionsPanel : BasePanel
{
    private const string TemplateNodeName = "DlgOptBtn";

    private Button templateButton;
    private readonly List<Button> runtimeButtons = new List<Button>();

    public override void OnOpen()
    {
        base.OnOpen();
        ResolveTemplate();
        RefreshOptions();
    }

    public override void OnClose()
    {
        base.OnClose();
        ClearRuntimeButtons();
    }

    public void RefreshOptions()
    {
        ResolveTemplate();
        ClearRuntimeButtons();

        if (templateButton == null)
        {
            return;
        }

        List<DialogueOption> options = DialogueManager.Instance.GetCurrentOptions();
        for (int i = 0; i < options.Count; i++)
        {
            DialogueOption option = options[i];
            if (option == null)
            {
                continue;
            }

            Button button = Instantiate(templateButton, templateButton.transform.parent);
            button.gameObject.name = $"DlgOptBtn_{i}";
            button.gameObject.SetActive(true);

            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = option.content;
            }

            int optionIndex = i;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnOptionClicked(optionIndex));
            runtimeButtons.Add(button);
        }
    }

    private void OnOptionClicked(int optionIndex)
    {
        if (UIManager.Instance.IsTransitioning)
        {
            return;
        }

        DialogueManager.Instance.SelectOption(optionIndex);
    }

    private void ResolveTemplate()
    {
        if (templateButton != null)
        {
            return;
        }

        Transform templateNode = transform.Find(TemplateNodeName);
        if (templateNode == null)
        {
            Debug.LogError("DialogueOptionsPanel 缺少名为子物体模板, 请检查TemplateNodeName是否正确");
            return;
        }

        templateButton = templateNode.GetComponent<Button>();
        if (templateButton == null)
        {
            Debug.LogError("DlgOptBtn 模板缺少 Button 组件");
            return;
        }

        templateButton.gameObject.SetActive(false);
    }

    private void ClearRuntimeButtons()
    {
        for (int i = 0; i < runtimeButtons.Count; i++)
        {
            Button button = runtimeButtons[i];
            if (button == null)
            {
                continue;
            }

            button.onClick.RemoveAllListeners();
            Destroy(button.gameObject);
        }

        runtimeButtons.Clear();
    }
}