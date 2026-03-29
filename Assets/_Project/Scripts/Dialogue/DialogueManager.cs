using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class CharacterProfile
    {
        // 角色ID，需与JSON中的character字段一致
        public string charID;
        // 显示在UI上的名字
        public string charName;
    }

    [Header("对话数据")]
    [SerializeField] private TextAsset dialogueJson;

    [Header("角色配置")]
    [SerializeField] private List<CharacterProfile> characters = new List<CharacterProfile>();

    [Header("UI 绑定")]
    // 对话面板物体
    [SerializeField] GameObject dialoguePanel;
    // 显示名字的文本组件
    [SerializeField] private TMP_Text nameText;
    // 显示对话正文的文本组件
    [SerializeField] private TMP_Text contentText;

    [Header("设置")]
    // 是否在脚本启动时自动开始对话
    [SerializeField] private bool playOnStart = false;

    // 解析后的完整数据库
    private DialogueDatabase database;
    // 当前播放到的对话索引
    private int currentIndex = -1;
    // 角色ID到配置信息的映射字典
    private Dictionary<string, CharacterProfile> characterMap = new Dictionary<string, CharacterProfile>();

    private void Start()
    {
        // 初始化角色映射表
        BuildCharacterMap();
        // 加载并解析 JSON
        DialogueLoad();

        if (playOnStart)
        {
            DialogueStart();
        }
    }

    // 构建角色字典，方便通过角色ID快速查找配置
    private void BuildCharacterMap()
    {
        characterMap.Clear();

        if (characters == null)
        {
            return;
        }

        foreach (var profile in characters)
        {
            if (profile == null || string.IsNullOrEmpty(profile.charID))
            {
                continue;
            }

            if (!characterMap.ContainsKey(profile.charID))
            {
                characterMap.Add(profile.charID, profile);
            }
            else
            {
                Debug.LogWarning($"角色ID重复: {profile.charID}");
            }
        }
    }

    // 读取并解析JSON对话数据
    public void DialogueLoad()
    {
        if (dialogueJson == null)
        {
            Debug.LogError("未指定对话JSON文件。");
            return;
        }

        database = JsonUtility.FromJson<DialogueDatabase>(dialogueJson.text);

        if (database == null || database.dialogues == null)
        {
            Debug.LogError("JSON解析失败, 请检查格式或字段名是否匹配。");
        }
    }

    // 开始对话并显示第一句
    public void DialogueStart()
    {
        if (database == null || database.dialogues == null || database.dialogues.Count == 0)
        {
            Debug.LogWarning("对话数据为空，无法开始。");
            return;
        }

        if (dialoguePanel == null)
        {
            Debug.LogError("未绑定对话面板对象。");
            return;
        }
        dialoguePanel.SetActive(true);

        currentIndex = 0;
        ShowCurrent();
    }

    // 播放下一句对话，对话未开始或已结束则重新开始
    public void DialogueNext()
    {
        if (database == null || database.dialogues == null)
        {
            return;
        }

        if (currentIndex < 0)
        {
            DialogueStart();
            return;
        }

        currentIndex++;

        if (currentIndex >= database.dialogues.Count)
        {
            DialogueEnd();
            return;
        }

        ShowCurrent();
    }

    // 结束对话并清空显示
    public void DialogueEnd()
    {
        currentIndex = -1;

        if (contentText != null) contentText.text = string.Empty;
        if (nameText != null) nameText.text = string.Empty;

        if (dialoguePanel == null)
        {
            Debug.LogError("未绑定对话面板对象。");
            return;
        }
        dialoguePanel.SetActive(false);
    }

    // 显示当前索引的内容
    private void ShowCurrent()
    {
        if (database == null || currentIndex < 0 || currentIndex >= database.dialogues.Count)
        {
            return;
        }

        DialogueEntry entry = database.dialogues[currentIndex];

        // 更新文本内容
        if (contentText != null)
        {
            contentText.text = entry.content;
        }

        // 更新角色信息
        UpdateUIWithCharacter(entry.character);
    }

    // 根据角色ID更新UI上的名字
    private void UpdateUIWithCharacter(string charID)
    {
        if (string.IsNullOrEmpty(charID))
        {
            ClearCharacterUI();
            return;
        }

        if (characterMap.TryGetValue(charID, out CharacterProfile profile))
        {
            if (nameText != null) nameText.text = profile.charName;
        }
        else
        {
            // 如果没找到配置，则直接显示 ID
            if (nameText != null) nameText.text = charID;
            Debug.LogWarning($"未找到角色 ID 为 {charID} 的配置。");
        }
    }

    // 清除角色 UI 显示
    private void ClearCharacterUI()
    {
        if (nameText != null) nameText.text = string.Empty;
    }
}
