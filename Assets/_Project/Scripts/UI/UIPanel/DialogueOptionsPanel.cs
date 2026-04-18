using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DialogueOptionsPanel : MenuPanel
{
    private const string TemplateNodeName = "DlgOptBtn";

    private Button templateButton;
    private readonly List<Button> runtimeButtons = new List<Button>();
    private Coroutine focusRoutine;
    private int focusedOptionIndex = 0;

    public override void OnOpen()
    {
        base.OnOpen();
        // 进入选项时保持对话输入
        InputManager.Instance.SwitchPlayerMode(false);
        ResolveTemplate();
        RefreshOptions();
    }

    public override void OnResume()
    {
        base.OnResume();
        // 从其他面板恢复时保持对话输入
        InputManager.Instance.SwitchPlayerMode(false);
    }

    public override void OnClose()
    {
        base.OnClose();
        if (focusRoutine != null)
        {
            StopCoroutine(focusRoutine);
            focusRoutine = null;
        }
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

        ConfigureButtonNavigation();

        if (runtimeButtons.Count == 0)
        {
            focusedOptionIndex = 0;
        }
        else
        {
            focusedOptionIndex = Mathf.Clamp(focusedOptionIndex, 0, runtimeButtons.Count - 1);
        }

        BeginSelectFirstOption();
    }

    private void OnOptionClicked(int optionIndex)
    {
        if (UIManager.Instance.IsTransitioning)
        {
            return;
        }

        focusedOptionIndex = optionIndex;
        DialogueManager.Instance.SelectOption(optionIndex);
    }

    // 配置选项按钮的导航，防止焦点越界以及可以循环比较酷
    private void ConfigureButtonNavigation()
    {
        int count = runtimeButtons.Count;
        if (count == 0)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Button current = runtimeButtons[i];
            if (current == null)
            {
                continue;
            }

            Button up = runtimeButtons[(i - 1 + count) % count];
            Button down = runtimeButtons[(i + 1) % count];

            Navigation nav = current.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = up;
            nav.selectOnDown = down;
            nav.selectOnLeft = null;
            nav.selectOnRight = null;
            current.navigation = nav;
        }
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

    private void SelectFirstOption()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        if (runtimeButtons.Count == 0)
        {
            return;
        }

        int index = Mathf.Clamp(focusedOptionIndex, 0, runtimeButtons.Count - 1);
        Button target = runtimeButtons[index];
        if (target == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(target.gameObject);
    }

    protected override GameObject DefaultFocused()
    {
        if (runtimeButtons.Count > 0)
        {
            int index = Mathf.Clamp(focusedOptionIndex, 0, runtimeButtons.Count - 1);
            Button target = runtimeButtons[index];
            if (target != null)
            {
                return target.gameObject;
            }
        }
        return null;
    }

    // 防止submit推动对话的同时导致选项被按下，增加一帧的延迟
    private void BeginSelectFirstOption()
    {
        if (focusRoutine != null)
        {
            StopCoroutine(focusRoutine);
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        focusRoutine = StartCoroutine(SelectFirstOptionNextFrame());
    }

    private IEnumerator SelectFirstOptionNextFrame()
    {
        yield return null;
        focusRoutine = null;
        SelectFirstOption();
    }

}
