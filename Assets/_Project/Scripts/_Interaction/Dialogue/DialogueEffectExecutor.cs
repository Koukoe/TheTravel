using System;
using System.Collections.Generic;
using UnityEngine;

public static class DialogueEffectExecutor
{
    public static bool TryApplyBlockingEffects(
        List<DialogueEffect> effects,
        DialogueOnObj dialogueSource,
        Action<string, Action> playActionAndResume,
        Action onCompleted)
    {
        if (effects == null || effects.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < effects.Count; i++)
        {
            DialogueEffect effect = effects[i];
            if (effect == null)
            {
                continue;
            }

            // 暂停对话并执行指定动作, 动作完成后继续对话
            if (!string.IsNullOrWhiteSpace(effect.PlayActionAndResume))
            {
                string actionId = effect.PlayActionAndResume.Trim();
                if (playActionAndResume == null)
                {
                    Debug.LogWarning($"PlayActionAndResume 未配置处理器: {actionId}");
                    onCompleted?.Invoke();
                    return true;
                }

                playActionAndResume(actionId, onCompleted);
                return true;
            }
        }

        return false;
    }

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

            // 跳过 blocking effects（由 TryApplyBlockingEffects 处理）
            if (!string.IsNullOrWhiteSpace(effect.PlayActionAndResume))
            {
                continue;
            }

            // 设置对话索引
            if (!string.IsNullOrWhiteSpace(effect.SetDialogueIndex))
            {
                if (!int.TryParse(effect.SetDialogueIndex.Trim(), out int nextDialogueIndex))
                {
                    Debug.LogWarning($"SetDialogueIndex 参数不是有效整数: {effect.SetDialogueIndex}");
                    continue;
                }

                if (dialogueSource != null && string.IsNullOrWhiteSpace(effect.SDItarget))
                {
                    // 如果没有显式 SDItarget，则优先通过来源物体设置索引。
                    // DialogueOnObj.SetDialogueIndex 会在有 dialogueGuid 时同步写入 DataArchive。
                    dialogueSource.SetDialogueIndex(nextDialogueIndex);
                    continue;
                }

                string targetDialogueId = !string.IsNullOrWhiteSpace(effect.SDItarget)
                    ? effect.SDItarget.Trim()
                    : dialogueSource != null ? dialogueSource.DialogueGuid : string.Empty;

                if (string.IsNullOrWhiteSpace(targetDialogueId))
                {
                    Debug.LogWarning("SetDialogueIndex 未指定目标对话 ID, 且当前对话来源也没有可用的 DialogueGuid");
                    continue;
                }

                if (DialogueManager.Instance == null)
                {
                    Debug.LogWarning("DialogueManager 未初始化, 无法设置对话索引");
                    continue;
                }

                DialogueManager.Instance.SetDialogueIndex(targetDialogueId, nextDialogueIndex);
                continue;
            }

            // 跳转场景
            if (!string.IsNullOrWhiteSpace(effect.GotoScene))
            {
                if (GameSceneManager.Instance == null)
                {
                    Debug.LogWarning("GameSceneManager 未初始化, 无法跳转场景");
                    continue;
                }

                string sceneName = effect.GotoScene.Trim();
                _ = GameSceneManager.Instance.LoadMain(sceneName);
                return;
            }


            // 播放BGM
            if (!string.IsNullOrWhiteSpace(effect.PlayBgm))
            {
                if (AudioManager.Instance == null)
                {
                    Debug.LogWarning("AudioManager 未初始化, 无法播放 BGM");
                    continue;
                }

                float fade = ParseFadeOrDefault(effect.PlayBgmFade, 1.0f, "PlayBgmFade");
                AudioManager.Instance.PlayBGM(effect.PlayBgm.Trim(), fade);
                continue;
            }

            // 播放SFX
            if (!string.IsNullOrWhiteSpace(effect.PlaySfx))
            {
                if (AudioManager.Instance == null)
                {
                    Debug.LogWarning("AudioManager 未初始化, 无法播放 SFX");
                    continue;
                }

                AudioManager.Instance.PlaySFX(effect.PlaySfx.Trim());
                continue;
            }

            // 停止BGM
            bool hasStopBgm = !string.IsNullOrWhiteSpace(effect.StopBgmTarget) ||
                              !string.IsNullOrWhiteSpace(effect.StopBgmFade);
            if (hasStopBgm)
            {
                if (AudioManager.Instance == null)
                {
                    Debug.LogWarning("AudioManager 未初始化, 无法停止 BGM");
                    continue;
                }

                StopTarget target = ParseStopTargetOrDefault(effect.StopBgmTarget, StopTarget.All);
                float fade = ParseFadeOrDefault(effect.StopBgmFade, 1.0f, "StopBgmFade");
                AudioManager.Instance.StopBGM(target, fade);
                continue;
            }

            Debug.LogWarning("检测到未配置字段的对话效果");
        }
    }

    private static StopTarget ParseStopTargetOrDefault(string rawValue, StopTarget defaultValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return defaultValue;
        }

        if (Enum.TryParse(rawValue.Trim(), true, out StopTarget target))
        {
            return target;
        }

        Debug.LogWarning($"StopBgmTarget 非法: {rawValue}, 将使用默认值 {defaultValue}");
        return defaultValue;
    }

    private static float ParseFadeOrDefault(string rawValue, float defaultValue, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return defaultValue;
        }

        if (float.TryParse(rawValue.Trim(), out float fade))
        {
            return fade;
        }

        Debug.LogWarning($"{fieldName} 非法: {rawValue}, 将使用默认值 {defaultValue}");
        return defaultValue;
    }
}
