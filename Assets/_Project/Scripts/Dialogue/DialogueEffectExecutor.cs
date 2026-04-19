using System;
using System.Collections.Generic;
using UnityEngine;

public static class DialogueEffectExecutor
{
    public static void ApplyEffects(List<DialogueEffect> effects, DialogueOnObj dialogueSource)
    {
        if (effects == null || effects.Count == 0)
        {
            return;
        }

        for (int i = 0; i < effects.Count; i++)
        {
            DialogueEffect effect = effects[i];
            if (effect == null)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(effect.SetDialogueIndex))
            {
                if (dialogueSource == null)
                {
                    Debug.LogWarning("当前对话没有来源物体, 无法设置对话索引");
                    continue;
                }

                if (!int.TryParse(effect.SetDialogueIndex.Trim(), out int nextDialogueIndex))
                {
                    Debug.LogWarning($"SetDialogueIndex 参数不是有效整数: {effect.SetDialogueIndex}");
                    continue;
                }

                dialogueSource.SetDialogueIndex(nextDialogueIndex);
                continue;
            }

            Debug.LogWarning("检测到未配置字段的对话效果");
        }
    }
}
