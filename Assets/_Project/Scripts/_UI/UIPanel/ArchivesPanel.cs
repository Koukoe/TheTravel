using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ArchivesPanel : MenuPanel
{
    [SerializeField] private List<Button> arcsBtn;
    [SerializeField] private Button backBtn;
    [SerializeField] private Text titleText;

    [SerializeField] private float yGap = 225f;
    [SerializeField] private float zIncrement = 100f;
    [SerializeField] private float unselectedAlpha = 0.5f;
    [SerializeField] private float hideAlpha = 0.25f;
    [SerializeField] private float smoothTime = 0.15f;

    private bool isSaveMode;
    [SerializeField] private List<UIArchiveSlotSource> slotSources = new List<UIArchiveSlotSource>();
    private int lastSelectedIndex = 0;  // 焦点记忆

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < arcsBtn.Count; i++)
        {
            int index = i;
            arcsBtn[i].onClick.AddListener(() => OnSlotClicked(index));
        }

        backBtn?.onClick.AddListener(OnBackClicked);

        for (int i = 0; i < slotSources.Count; i++)
        {
            if (DataArchivesSystem.IsSlotOccupied(i))
            {
                // 切换对应截图、显示对应信息之类的
            }
        }
    }

    public void Init(bool isSave = true)
    {
        this.isSaveMode = isSave;
        if (titleText != null) titleText.text = isSaveMode ? "保存进度" : "载入进度";
    }

    private void Update()
    {
        GameObject selectedGO = EventSystem.current?.currentSelectedGameObject;
        int selectedIndex = arcsBtn.FindIndex(btn => btn.gameObject == selectedGO);

        if (selectedIndex == -1)
        {
            for (int i = 0; i < slotSources.Count; i++)
            {
                // 维持 Y 轴
                int indexOffset = i - lastSelectedIndex;
                float targetY = -indexOffset * yGap;
                float targetZ = 0f;
                slotSources[i].SetTarget(new Vector3(0, targetY, targetZ), hideAlpha, smoothTime * 2);
            }
            return;
        }

        lastSelectedIndex = selectedIndex;

        for (int i = 0; i < slotSources.Count; i++)
        {
            int indexOffset = i - selectedIndex;
            int distance = Mathf.Abs(indexOffset);

            // 展开
            float targetY = -indexOffset * yGap;
            float targetZ = distance * zIncrement;

            float targetAlpha = (distance == 0) ? 1f : unselectedAlpha;

            slotSources[i].SetTarget(new Vector3(0, targetY, targetZ), targetAlpha, smoothTime);
        }
    }

    private void OnSlotClicked(int i)
    {
        if (isSaveMode)
        {
            if (DataArchivesSystem.IsSlotOccupied(i))
            {
                var panel = UIManager.Instance.Push<ConfirmPanel>("ConfirmPanel");
                panel.Setup(onConfirm: () => SaveGame(i), title: "", content: "");
            }
            else { SaveGame(i); }
        }
        else if (DataArchivesSystem.IsSlotOccupied(i))
        {
            var panel = UIManager.Instance.Push<ConfirmPanel>("ConfirmPanel");
            panel.Setup(onConfirm: () => LoadGame(i), title: "", content: "");
        }
    }

    private void SaveGame(int id)
    {
        Debug.Log($"保存到档位 {id}");
        GameFlowManager.Instance.SaveGame(id);

        // 切换对应截图、显示对应信息之类的
    }
    private void LoadGame(int id)
    {
        Debug.Log($"读取档位 {id}");
        GameFlowManager.Instance.LoadGame(id);
    }

    protected override GameObject DefaultFocused() => arcsBtn.Count > 0 ? arcsBtn[0].gameObject : null;
}