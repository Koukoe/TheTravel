using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArchivesPanel : MenuPanel
{
    [SerializeField] private Button defaultBtn;
    [SerializeField] private Button backBtn;

    protected override void Awake()
    {
        base.Awake();

        backBtn?.onClick.AddListener(() =>
        {
            UIManager.Instance.Pop();
        });

    }

    protected override GameObject DefaultFocused() => backBtn != null ? backBtn.gameObject : null;
}
