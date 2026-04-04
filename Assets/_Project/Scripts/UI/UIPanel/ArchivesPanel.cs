using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.CompilerServices;

public class ArchivesPanel : MenuPanel
{
    [SerializeField] private List<Button> arcsBtn;
    [SerializeField] private Button backBtn;

    protected override void Awake()
    {
        base.Awake();

        backBtn?.onClick.AddListener(() =>
        {
            UIManager.Instance.Pop();
        });

    }
    public void Init(bool isSave = true)
    {
        for (int i = 0; i < arcsBtn.Count; i++)
        {
            int index = i;  // 解决闭包问题
            Button btn = arcsBtn[i];
            if (btn == null) continue;

            btn.onClick.RemoveAllListeners();  // 防止重复绑定

            if (isSave)
                btn.onClick.AddListener(() => OnSaveClicked(index));
            else
                btn.onClick.AddListener(() => OnLoadClicked(index));
        }
    }

    public void OnSaveClicked(int id)
    {

    }

    public void OnLoadClicked(int id)
    {

    }

    protected override GameObject DefaultFocused() => arcsBtn.Count > 0 ? arcsBtn[0].gameObject : null;
}
