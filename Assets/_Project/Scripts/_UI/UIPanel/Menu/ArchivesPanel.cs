using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

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
    private int previousAudioIndex = -1; // 追踪切换音效的索引

    private Texture2D[] _activeTextures = new Texture2D[9];  // 追踪硬盘图片纹理

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

    protected void OnEnable()
    {
        previousAudioIndex = -1; // 面板打开时重置音效追踪
        RefreshAllSlots().Forget();
    }

    protected void OnDisable() => ClearLoadedTextures();

    private async UniTaskVoid RefreshAllSlots()
    {
        // 刷新前先清理旧缓存
        ClearLoadedTextures();

        for (int i = 0; i < slotSources.Count; i++)
        {
            if (DataArchivesSystem.IsSlotOccupied(i))
            {
                string fileName = $"thumb_{i}.jpg";
                Texture2D tex = await ImageLoader.LoadTextureAsync(fileName);

                _activeTextures[i] = tex;

                string description = DataArchivesSystem.GetInfo(i);
                slotSources[i].RefreshDisplay(tex, description);
            }
            else
            {
                slotSources[i].RefreshDisplay(null, " ");
            }
        }
    }

    private void ClearLoadedTextures()
    {
        for (int i = 0; i < _activeTextures.Length; i++)
        {
            if (_activeTextures[i] != null)
            {
                Destroy(_activeTextures[i]);
                _activeTextures[i] = null;
            }
        }
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

        // 切换音效逻辑
        if (selectedIndex != previousAudioIndex)
        {
            if (previousAudioIndex != -1 && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("Select_button_UI");
            }
            previousAudioIndex = selectedIndex;
        }

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
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("UI_sound");
        }

        if (isSaveMode)
        {
            if (DataArchivesSystem.IsSlotOccupied(i))
            {
                var panel = UIManager.Instance.Push<ConfirmPanel>("ConfirmPanel");
                panel.Setup(onConfirm: () => SaveGame(i).Forget(), title: "", content: "");
            }
            else { SaveGame(i).Forget(); }
        }
        else if (DataArchivesSystem.IsSlotOccupied(i))
        {
            var panel = UIManager.Instance.Push<ConfirmPanel>("ConfirmPanel");
            panel.Setup(onConfirm: () => LoadGame(i), title: "", content: "");
        }
    }

    private async UniTaskVoid SaveGame(int id)
    {
        Debug.Log($"保存到档位 {id}");

        Texture2D newThumb = await GameFlowManager.Instance.SaveGame(id);


        // 清理旧的纹理内存并将新图加入内存追踪列表
        if (_activeTextures[id] != null)
        {
            Destroy(_activeTextures[id]);
            _activeTextures[id] = null;
        }

        _activeTextures[id] = newThumb;

        // 刷新 UI 显示
        string info = DataArchivesSystem.GetInfo(id);
        slotSources[id].RefreshDisplay(newThumb, info);
    }

    private void LoadGame(int id)
    {
        Debug.Log($"读取档位 {id}");
        GameFlowManager.Instance.LoadGame(id).Forget();
    }

    protected override GameObject DefaultFocused() => arcsBtn.Count > 0 ? arcsBtn[0].gameObject : null;
}