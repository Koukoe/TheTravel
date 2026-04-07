using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;
using System.Runtime.Serialization;
using System;

public class UIOptionToggle : MonoBehaviour, ISelectHandler, IDeselectHandler, IMoveHandler
{
    public TextMeshProUGUI valueText;
    public string[] options = { };

    public UnityEvent<int> onValueChanged;  // 广播档位

    [SerializeField] private int currentIndex = 0;
    private bool isSelected = false;

    public void SetIndex(int index, bool triggerEvent = false)
    {
        currentIndex = Mathf.Clamp(index, 0, options.Length - 1);
        UpdateDisplay();
        if (triggerEvent) onValueChanged?.Invoke(currentIndex);
    }

    public int GetIndex() => currentIndex;

    public void OnSelect(BaseEventData eventData) => isSelected = true;
    public void OnDeselect(BaseEventData eventData) => isSelected = false;

    public void OnMove(AxisEventData eventData)  // 检测方向键、摇杆与十字键
    {
        if (!isSelected || options.Length == 0) return;

        // 截获左右移动
        if (Mathf.Abs(eventData.moveVector.x) > 0.5f)
        {
            int direction = eventData.moveVector.x > 0 ? 1 : -1;
            int lastIndex = currentIndex;
            currentIndex = Mathf.Clamp(currentIndex + direction, 0, options.Length - 1);

            if (currentIndex != lastIndex)
            {
                UpdateDisplay();
                onValueChanged?.Invoke(currentIndex); // 触发通知
            }

            // 告诉 EventSystem 不要将左右键信号传给别的组件
            eventData.Use();
        }
    }

    private void UpdateDisplay()
    {
        if (valueText != null && options.Length > 0)
        {
            valueText.text = $"{options[currentIndex]}";
        }
    }

    void OnEnable() => isSelected = false;

    void OnDisable() => isSelected = false;
}