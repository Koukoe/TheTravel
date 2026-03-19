using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    /// <summary>
    /// 存储当前UI的栈
    /// </summary>
    public Stack<BasePanel> stack_ui;
    /// <summary>
    /// 存储Panel的名称与物体的对应关系
    /// </summary>
    public Dictionary<string, GameObject> dict_uiobject;
    /// <summary>
    /// 当前场景对应的Canvas
    /// </summary>
    public GameObject CanvasObj;
    private static UIManager instance;
    /// <summary>
    /// 获得UIManager的单例
    /// </summary>
    /// <returns></returns>
    public static UIManager GetInstance()
    {
        if (instance == null)
        {
            Debug.Log("UIManager实例不存在");
            return instance;
        }
        else
        {
            return instance;
        }
    }
    public UIManager()
    {
        instance = this;
    }

    public GameObject GetSingleObject(UIType uIType)
    {
       if (dict_uiobject.ContainsKey(uIType.Name))
        {
            return dict_uiobject[uIType.Name];
        }

        if(CanvasObj == null)
        {
            Debug.LogError("UIManager未找到Canvas!");
            return null;
        }

        GameObject gameObject=GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(uIType.Path), CanvasObj.transform);
        return gameObject;
    }
/// <summary>
/// 将UI压入栈中
/// </summary>
/// <param name="basePanel">目标Panel</param>
    public void Push(BasePanel basePanel)
    {
        Debug.Log($"{basePanel.uiType.Name}被Push进stack");
        if (stack_ui.Count > 0)
        {
            stack_ui.Peek().OnDisable();
        }

        GameObject ui_object = GetSingleObject(basePanel.uiType);
        dict_uiobject.Add(basePanel.uiType.Name, ui_object);
        basePanel.ActiveObj = ui_object;

        if (stack_ui.Count ==0)
        {
            stack_ui.Push(basePanel);
        }
        else
        {
            if (stack_ui.Peek().uiType.Name != basePanel.uiType.Name)
            {
                stack_ui.Push(basePanel);
            }
        }

        basePanel.Onstart();
    }
/// <summary>
/// 将UI从栈中弹出
/// </summary>
/// <param name="isload">isload为真时Pop全部，为假时Pop栈顶</param>
    public void Pop(bool isload)
    {
        if (isload==true)
        {
            if (stack_ui.Count > 0)
            {
                stack_ui.Peak().OnDisable();
                stack_ui.Peak().OnDestroy();
                GameObject.Destroy(dict_uiobject[stack_ui.Peek().uiType.Name]);
                dict_uiobject.Remove(stack_ui.Peek().uiType.Name);
                stack_ui.Pop();
                Pop (true);
            }
        }
        else
        {
            if (stack_ui.Count > 0)
            {
                stack_ui.Peek().OnDisable();
                stack_ui.Peek().OnDestroy();
                GameObject.Destroy(dict_uiobject[stack_ui.Peek().uiType.Name]);
                dict_uiobject.Remove(stack_ui.Peek().uiType.Name);
                stack_ui.Pop();

                if (stack_ui.Count > 0)
                {
                    stack_ui.Peek().OnEnable();
                }
            }
        }
    }
}
