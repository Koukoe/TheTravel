| 实现情况   | 功能             | 写法 Template                                            | 说明                                   |
| ---------- | ---------------- | -------------------------------------------------------- | -------------------------------------- |
|            | NPC移动          | {"MoveNpc": {"npc": "xxx", "to": "yyy"}}                 | 让某NPC去某个位置/区域                 |
| 接口       | NPC播放动画      | {"PlayActionAndResume": {"npc": "xxx", "action": "yyy"}} | 某NPC进行特殊动作/动画                 |
|            | 获得道具         | {"GetItem": "itemID"}                                    | 主角获得道具（itemID需约定）           |
|            | 失去道具         | {"LoseItem": "itemID"}                                   | 主角失去道具                           |
|            | NPC消失/隐藏     | {"HideNpc": "xxx"}                                       | 让某NPC从场景消失                      |
|            | NPC出现/显示     | {"ShowNpc": "xxx"}                                       | 让某NPC出现在场景（这个需要吗？）      |
| 索引更方便 | 设置对话阶段     | {"SetNpcDialogueStage": 阶段数字}                        | 跳到新的对话阶段                       |
| 已实现     | 设置对话文件索引 | {"SetDialogueIndex": "索引数字"}                         | 下次对话使用的对话文件索引             |
| 未测试     | 播放音乐         | { "PlayBgm": "bgm_id", "PlayBgmFade": "1.0" }            | 播放/切歌                              |
| 未测试     | 停止音乐         | { "StopBgmTarget": "All", "StopBgmFade": "1.0" }         | 停止音乐                               |
| 能切过去   | 切换场景         | {"GotoScene": "sceneID"}                                 | 传送到新场景/地图                      |
|            | 触发剧情事件     | {"TriggerEvent": "eventID", ...}                         | 触发一个故事/离港等主流程事件          |
| 未测试     | 播放音效（SFX）  | {"PlaySfx": "sfxID"}                                     | 播放短音效，如“船铃”                   |
|            | 播放动画/过场    | {"PlayCutscene": "cutsceneID"}                           | 播放离港等剧情动画，动画名需与资源统一 |

好折磨，不想更新这个表了
