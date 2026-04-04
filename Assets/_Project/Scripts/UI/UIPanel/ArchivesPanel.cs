using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ArchivesPanel : MenuPanel
{
    [SerializeField] private List<Button> arcBtn;
    [SerializeField] private Button backBtn;

    protected override void Awake()
    {
        base.Awake();

        backBtn?.onClick.AddListener(() =>
        {
            UIManager.Instance.Pop();
        });

    }

    protected override GameObject DefaultFocused() => arcBtn.Count > 0 ? arcBtn[0].gameObject : null;
}
