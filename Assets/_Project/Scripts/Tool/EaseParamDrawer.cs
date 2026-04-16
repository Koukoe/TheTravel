using UnityEngine;
using UnityEditor;
using System;
[CustomPropertyDrawer(typeof(EaseParam))]
public class EaseParamDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var typeProp = property.FindPropertyRelative("type");
        var curveProp = property.FindPropertyRelative("curve");
        EasingUtils.EaseType type = (EasingUtils.EaseType)typeProp.enumValueIndex;

        // 使用 PrefixLabel 自动处理左侧的属性名（如 "Speed Ease"）
        Rect contentRect = EditorGUI.PrefixLabel(position, label);

        // 布局：左侧 60% 为下拉菜单，右侧剩余部分为预览/编辑区
        float splitWidth = contentRect.width * 0.6f;
        Rect typeRect = new Rect(contentRect.x, contentRect.y, splitWidth, contentRect.height);
        Rect curveRect = new Rect(contentRect.x + splitWidth + 5, contentRect.y, contentRect.width - splitWidth - 5, contentRect.height);

        // 1. 绘制左侧下拉框
        EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);

        // 2. 绘制右侧区域
        if (type == EasingUtils.EaseType.CustomCurve)
        {
            // --- 自定义模式：显示绿色曲线编辑框 (可点、可捏、可拖拽资源) ---
            curveProp.animationCurveValue = EditorGUI.CurveField(curveRect, curveProp.animationCurveValue, Color.green, new Rect(0, 0, 1, 1));
        }
        else
        {
            // --- 预设模式：完全沿用你之前的蓝色预览线绘制方式 ---
            DrawPresetPreview(curveRect, type);
        }

        EditorGUI.EndProperty();
    }

    private void DrawPresetPreview(Rect rect, EasingUtils.EaseType type)
    {
        // 绘制深色背景
        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f, 1f));

        Handles.BeginGUI();
        Handles.color = new Color(0.3f, 0.7f, 1f, 1f); // 之前的蓝色

        Vector3[] points = new Vector3[11];
        for (int i = 0; i <= 10; i++)
        {
            float t = i / 10f;
            float val = EasingUtils.GetValue(type, t);
            // 映射逻辑完全保持不变
            points[i] = new Vector3(
                rect.x + t * rect.width,
                rect.yMax - val * rect.height,
                0);
        }
        // 绘制平滑线
        Handles.DrawAAPolyLine(2f, points);
        Handles.EndGUI();

        // 绘制预览框的灰色边框
        DrawRectOutline(rect, new Color(0.3f, 0.3f, 0.3f));
    }

    private void DrawRectOutline(Rect rect, Color color)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y, 1, rect.height), color);
    }
}