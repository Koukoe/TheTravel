using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    private static UIManager instance;
    public static UIManager GetInstance()
    {
        if (instance == null) instance = new UIManager();
        return instance;
    }

    public Stack<BasePanel> stack_ui = new Stack<BasePanel>();
    public Dictionary<string, GameObject> dict_uiobject = new Dictionary<string, GameObject>();
    public GameObject CanvasObj;

    public UIManager()
    {
        instance = this;
    }

    // 内部方法：从你的 PoolManager 获取对象
    private GameObject GetSingleObject(UIType uIType)
    {
        if (CanvasObj == null)
        {
            CanvasObj = UIMethods.GetInstance().FindCanvas();
        }

        // 调用你的 PoolManager.Global.Get
        GameObject gameObject = PoolManager.Global.Get(uIType.Name);
        
        if (gameObject != null)
        {
            gameObject.transform.SetParent(CanvasObj.transform, false);
        }
        else
        {
            Debug.LogError($"PoolManager中未找到名为 {uIType.Name} 的配置，请检查Inspector面板！");
        }
        
        return gameObject;
    }

    public void Push(BasePanel basePanel)
    {
        Debug.Log($"{basePanel.uiType.Name} 被推入栈");

        // 1. 如果栈里有东西，先禁用当前的顶层 UI
        if (stack_ui.Count > 0)
        {
            stack_ui.Peek().OnDisable();
            stack_ui.Peek().ActiveObj.SetActive(false); 
        }

        // 2. 从对象池获取新物体
        GameObject ui_object = GetSingleObject(basePanel.uiType);
        basePanel.ActiveObj = ui_object;

        // 3. 记录到字典并压入栈
        if (!dict_uiobject.ContainsKey(basePanel.uiType.Name))
        {
            dict_uiobject.Add(basePanel.uiType.Name, ui_object);
        }
        
        stack_ui.Push(basePanel);

        // 4. 执行生命周期
        ui_object.SetActive(true);
        basePanel.OnEnable();

    }

    public void Pop(bool isAll)
    {
        if (stack_ui.Count <= 0) return;

        if (isAll)
        {
            while (stack_ui.Count > 0)
            {
                CloseTopPanel();
            }
        }
        else
        {
            CloseTopPanel();
            // 恢复下层 UI
            if (stack_ui.Count > 0)
            {
                BasePanel nextPanel = stack_ui.Peek();
                nextPanel.ActiveObj.SetActive(true);
                nextPanel.OnEnable();
            }
        }
    }

    private void CloseTopPanel()
    {
        BasePanel topPanel = stack_ui.Pop();
        topPanel.OnDisable();
    

        // 使用你的静态方法释放回对象池
        PoolManager.Release(topPanel.ActiveObj);
        
        if (dict_uiobject.ContainsKey(topPanel.uiType.Name))
        {
            dict_uiobject.Remove(topPanel.uiType.Name);
        }
    }
}