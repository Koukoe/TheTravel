# 我的游戏项目

以下是剧情分支。

## 目录结构

- `dialogues/` —— 所有的对话json文件
- `character_map.json` —— 角色名映射表

## 编辑建议

- 对话文件会放进 `dialogues/` 文件夹，按照岛屿和流程进行对话的分类
- 角色名只用英文代码，实际显示会自动按 `character_map.json` 替换为中文

## 角色配置表说明

- 从上往下的顺序是：
-主要角色

-次要角色

-彩色小镇npc

-纯白小镇npc


-后面可能会根据同一个岛屿在流程上的不同状态引入不同的npc，不过这个不重要，只是想让你知道

## effects字段说明
-功能	写法 Template	说明

-NPC移动	{"MoveNpc": {"npc": "xxx", "to": "yyy"}}	让某NPC去某个位置/区域

-NPC播放动画	{"PlayActionAndResume": {"npc": "xxx", "action": "yyy"}}	某NPC进行特殊动作/动画

-获得道具	{"GetItem": "itemID"}	主角获得道具（itemID需约定）

-失去道具	{"LoseItem": "itemID"}	主角失去道具

-NPC消失/隐藏	{"HideNpc": "xxx"}	让某NPC从场景消失

-NPC出现/显示	{"ShowNpc": "xxx"}	让某NPC出现在场景（这个需要吗？）

-改变对话阶段 "SetDialogueIndex": "xxx"  改变多阶段对话的阶段，实现多次对话不同内容

-触发剧情事件	{"TriggerEvent": "eventID"}	触发主线、支线或特殊事件

-播放音乐	{"PlayBgm": "bgmID"}	播放/切歌

-停止音乐	{"StopBgm": true}	停止音乐

-切换场景	{"GotoScene": "sceneID"}	传送到新场景/地图

-触发剧情事件	{"TriggerEvent": "eventID", ...}	触发一个故事/离港等主流程事件

-播放音效（SFX）	{"PlaySfx": "sfxID"}	播放短音效，如“船铃”

-播放动画/过场	{"PlayCutscene": "cutsceneID"}	播放离港等剧情动画，动画名需与资源统一

-需要说明的是，"targetIsland": "islandID"的功能是让铁公鸡的方向始终指向目标岛屿