using System;
using System.Collections.Generic;

// 对话数据库容器
[Serializable]
public class DialogueDatabase
{
    // 与 JSON 中的 "dialogues" 字段名匹配
    public List<DialogueEntry> dialogues = new List<DialogueEntry>();
}

// 单条对话条目
[Serializable]
public class DialogueEntry
{
    // 对话 ID
    public string id;
    // 角色 ID (对应 Manager 中的配置)
    public string character;
    // 对话文本内容
    public string content;
}
