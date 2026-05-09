using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TaskManager))]
public class TaskManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TaskManager manager = (TaskManager)target;

        EditorGUILayout.Space(15);

        GUI.color = Color.yellow;
        if (GUILayout.Button("Finish All Active Tasks", GUILayout.Height(30)))
        {
            if (Application.isPlaying)
            {
                manager.FinishAllActiveTasks();
            }
            else
            {
                Debug.LogWarning("必须在运行模式（Play Mode）下点击才能生效！");
            }
        }
        GUI.color = Color.white;
    }
}


[CustomEditor(typeof(TaskBasic), true)] // 对所有子类生效
public class TaskBasicEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TaskBasic script = (TaskBasic)target;

        EditorGUILayout.Space(5);

        GUI.enabled = Application.isPlaying && !script.isDone;

        if (GUILayout.Button("Finish This Task Target", GUILayout.Height(25)))
        {
            script.FinishTask();
        }

        GUI.enabled = true;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("我不让你点嘿嘿", MessageType.None);
        }
    }
}