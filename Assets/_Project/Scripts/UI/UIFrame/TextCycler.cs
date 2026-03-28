using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;

// 增加 IMoveHandler 接口
public class TextCycler : MonoBehaviour, ISelectHandler, IDeselectHandler, IMoveHandler
{
    public TextMeshProUGUI valueText;
    public string[] options = { };

    // 当值改变时通知外部（比如通知 AudioPanel 保存数据）
    public UnityEvent<int> onValueChanged;

    private int currentIndex = 0;
    private bool isSelected = false;

    // --- 外部初始化接口 ---
    public void SetIndex(int index, bool triggerEvent = false)
    {
        currentIndex = Mathf.Clamp(index, 0, options.Length - 1);
        UpdateDisplay();
        if (triggerEvent) onValueChanged?.Invoke(currentIndex);
    }

    public int GetIndex() => currentIndex;

    // --- 输入处理 ---
    public void OnSelect(BaseEventData eventData) => isSelected = true;
    public void OnDeselect(BaseEventData eventData) => isSelected = false;

    // EventSystem 自动调用的移动逻辑
    public void OnMove(AxisEventData eventData)
    {
        if (!isSelected || options.Length == 0) return;

        // 判定左右移动
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

            // 告诉 EventSystem 这一步我已经处理了，不要再传给别的组件
            eventData.Use();
        }
    }

    private void UpdateDisplay()
    {
        if (valueText != null && options.Length > 0)
        {
            // 使用富文本稍微修饰一下选中感
            valueText.text = $"<  {options[currentIndex]}  >";
        }
    }

    // 当被 PoolManager 回收时，重置选中状态
    void OnDisable()
    {
        isSelected = false;
    }
}