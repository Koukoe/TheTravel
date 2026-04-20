using System;
using System.Collections.Generic;

// 对话数据库容器
[Serializable]
public class DialogueDatabase
{
    // 与 JSON 中的 "dialogues" 字段名匹配
    public List<DialogueEntry> dialogues = new List<DialogueEntry>();
}

// 对话选项
[Serializable]
public class DialogueOption
{
    // 选项文本内容
    public string content;
    // 选项对应的下一句对话 ID，为空表示按照顺序继续，"END" 表示对话结束
    public string nextId;
    // 选项效果列表, 用列表是因为以后可能会有别的效果
    public List<DialogueEffect> effects = new List<DialogueEffect>();
}

// 选项效果
[Serializable]
public class DialogueEffect
{
    // 将对话来源物体的 dialogueIndex 设置为指定值
    // 写法: { "SetDialogueIndex": "2" }
    public string SetDialogueIndex;

    // 对话中途隐藏对话框并播放角色动作，动作完成后恢复对话
    // 写法: { "PlayActionAndResume": "npc_wave_01" }
    public string PlayActionAndResume;
}

// 单条对话条目
[Serializable]
public class DialogueEntry
{
    // 对话 ID
    public string id;
    // 下一句对话的 ID，为空表示按照顺序继续，"END" 表示对话结束
    public string nextId;
    // 对话条目效果，进入该句时触发
    public List<DialogueEffect> effects = new List<DialogueEffect>();
    // 选项列表，null 或空列表则表示没有选项
    public List<DialogueOption> options = new List<DialogueOption>();
    // 角色 ID (对应 Manager 中的配置)
    public string character;
    // 对话文本内容
    public string content;
}
