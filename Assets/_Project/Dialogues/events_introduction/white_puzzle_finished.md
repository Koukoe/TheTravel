# fountain_ritual 事件流程说明

**实现目的**：玩家将从各处解谜和任务中获得的四件物品（海螺、木杯、纽扣、唱片）全部投入镇子中央的喷泉后，触发河流分开的神迹，老船长得以过河并瞬移至工程师家，主线剧情推进至新阶段。

---

## 涉及物体

| 物体 | 说明 |
|------|------|
| `fountain` | 镇子中央的喷泉，投入四件物品的交互点 |
| `shell_item` | 海螺，来自石碑解谜（`stone_puzzle`） |
| `wooden_cup` | 木杯，来自鸟浴池解谜（`bird_bath_puzzle`） |
| `antony_button` | 纽扣，来自安朵丝花任务（`andosi_flower_pick`） |
| `radio_disc` | 唱片，来自收音机解谜（`radio_puzzle`） |
| `river` | 分隔小镇的河流，过场动画中会分开 |
| `captain` | 老船长，过场动画中跑过河床 |
| `engineer_house` | 工程师的家，老船长的瞬移目的地 |

---

## 步骤流程

### 阶段一：投入物品

1. 玩家与 `fountain` 交互。
2. 条件检查：背包中是否同时拥有 `shell_item`、`wooden_cup`、`antony_button`、`radio_disc`。
   - 全部拥有 → 进入阶段二。
   - 缺少任意物品 → 提示“也许需要投入一些特殊的东西......”。
3. 依次移除四件道具：
   - `LoseItem("shell_item")`
   - `LoseItem("wooden_cup")`
   - `LoseItem("antony_button")`
   - `LoseItem("radio_disc")`

### 阶段二：喷泉反应

1. 播放投掷物品音效（`item_splash`，可重复 4 次或一次播放）。
2. 喷泉播放发光/升腾特效（`fountain_glow_effect`）。
3. 喷泉特效持续 2-3 秒后，触发阶段三。

### 阶段三：河流分开

1. 禁用玩家输入（`LockPlayerInput`）。
2. 摄像机从当前位置平滑移动至河流上空（`MoveCameraToRiver`）。
3. 播放河流从中间分开的动画（`river_split`）。
4. 播放水流音效（`water_split_sfx`）。
5. 动画播放完毕后，河床露出。

### 阶段四：老船长过河

1. 老船长 `captain` 出现在河边（如已隐藏则 `ShowNpc("captain")`）。
2. 老船长播放跑步动画，沿河床从河边跑向河对岸（`PlayActionAndResume: "captain_run_across_river"`）。
3. 播放跑步音效（`footsteps_sfx`）或背景音乐持续播放。
4. 当老船长离开摄像机可视范围后：
   - 将老船长瞬移至工程师家（`MoveNpc: "captain", position: "engineer_house_coords"`）。

### 阶段五：收尾

1. 河流保持分开状态。
2. 摄像机平滑移动回归至玩家角色（`MoveCameraToPlayer`）。
3. 恢复玩家输入（`UnlockPlayerInput`）。
4. 推进所有 White NPC 的对话阶段（见下方“对话阶段更新”）。
5. 事件结束，主线推进至与工程师/老船长汇合阶段。

---

## 伪代码

```js
const requiredItems = ["shell_item", "wooden_cup", "antony_button", "radio_disc"];
let ritualCompleted = false;

// 阶段一：投入物品
function onFountainInteract() {
    if (ritualCompleted) {
        showHint("喷泉已经恢复平静了。");
        return;
    }

    if (!hasAllItems(requiredItems)) {
        showHint("也许需要投入一些特殊的东西......");
        return;
    }

    // 移除四件道具
    requiredItems.forEach(item => LoseItem(item));
    playSfx("item_splash");
    startRitual();
}

// 阶段二 & 三 & 四 & 五：连续过场
function startRitual() {
    ritualCompleted = true;

    // 喷泉发光特效
    playActionAndResume("fountain_glow_effect", () => {
        // 锁定输入，摄像机移至河流
        LockPlayerInput();
        moveCamera("river_view", () => {
            // 播放河流分开动画
            playCutscene("river_split", () => {
                playSfx("water_split_sfx");

                // 老船长出现并跑过河床
                ShowNpc("captain");
                playActionAndResume("captain_run_across_river", () => {
                    playSfx("footsteps_sfx");

                    // 老船长离开视野后
                    onCaptainExitView();
                });
            });
        });
    });
}

// 老船长离开视野后的处理
function onCaptainExitView() {
    // 隐藏并瞬移至工程师家
    HideNpc("captain");
    MoveNpc("captain", "engineer_house_x,engineer_house_y,engineer_house_z", "0,0,0");
    
    // 摄像机回归玩家
    moveCamera("player_view", () => {
        UnlockPlayerInput();
        // 老船长在工程师家出现
        ShowNpc("captain");

         // 推进全镇 White NPC 对话阶段
        SetDialogueIndex("White_NPC01", 2);
        SetDialogueIndex("White_NPC02", 2);
        SetDialogueIndex("White_NPC03", 3);
        SetDialogueIndex("White_NPC04", 2);
        SetDialogueIndex("White_NPC05", 1);
    });
}

-说明与扩展
-四件道具的 ID 需与各解谜事件中的 GetItem 对应一致。

-河流分开动画 river_split 和跑步动画 captain_run_across_river 需动画师配合制作。

-摄像机控制需与场景中的摄像机点位（如 river_view、player_view）对齐，或由程序动态计算路径。

-瞬移坐标 engineer_house_coords 需与场景中工程师家门口位置一致。

-事件完成后，老船长位于工程师家，后续与工程师的对话可直接加载对应 JSON。

-若需在过场中播放背景音乐，可在 startRitual 开始时调用 PlayBgm，并在收尾后 StopBgm 或切换曲目。

-该事件为一次性主线事件，ritualCompleted 标记需持久化存档。