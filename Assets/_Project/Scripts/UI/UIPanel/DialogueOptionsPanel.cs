using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
        ResolveTemplate();
        RefreshOptions();
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

    // 用来恢复焦点, 比如打开别的面板再切回来的时候
    private void LateUpdate()
    {
        if (!gameObject.activeInHierarchy || EventSystem.current == null || runtimeButtons.Count == 0)
        {
            return;
        }

        GameObject current = EventSystem.current.currentSelectedGameObject;
        if (current == null)
        {
            TryRestoreFocus();
            return;
        }

        if (!current.transform.IsChildOf(transform))
        {
            // 栈面板关闭后，如果焦点不在对话选项面板，则拉回到上次选中项
            if (UIManager.Instance != null && UIManager.Instance.Count == 0)
            {
                TryRestoreFocus();
            }
            return;
        }

        Button selectedButton = current.GetComponent<Button>();
        if (selectedButton == null)
        {
            selectedButton = current.GetComponentInParent<Button>();
        }

        if (selectedButton == null)
        {
            return;
        }

        int index = runtimeButtons.IndexOf(selectedButton);
        if (index >= 0)
        {
            focusedOptionIndex = index;
        }
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

    private void TryRestoreFocus()
    {
        if (UIManager.Instance.IsTransitioning)
        {
            return;
        }

        SelectFirstOption();
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
