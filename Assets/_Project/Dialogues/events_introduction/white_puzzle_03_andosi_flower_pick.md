# andosi_flower_pick 事件流程说明

**实现目的**：玩家接受 White_NPC03 的委托后，前往镇子北边的乱石堆采摘安朵丝花。采花完成后，返回与 White_NPC03 对话，推进至 `White_NPC03_3.json`。

---

## 涉及物体

| 物体 | 说明 |
|------|------|
| `andosi_flower` | 安朵丝花，生长在镇子北边的乱石堆中 |
| `antony_button` | 安东尼的扣子，完成任务后由 White_NPC03 给予 |

---

## 步骤流程

### 阶段一：接受委托

1. 玩家与 White_NPC03 对话至 `White_NPC03_2.json` 结束。
2. 对话中提到“到镇子北边的乱石堆里采一株安朵丝”。
3. 此时激活乱石堆处的 `andosi_flower` 物体（可交互）。

### 阶段二：采摘安朵丝

1. 玩家前往镇子北边乱石堆，与 `andosi_flower` 交互。
2. 播放采摘动画/音效（`pick_flower`）。
3. 玩家获得道具 `andosi_flower`（`GetItem`）。
4. `andosi_flower` 物体从场景隐藏（`HideNpc`）。

### 阶段三：交还花朵

1. 玩家携带 `andosi_flower` 返回与 White_NPC03 对话。
2. 条件检查：背包中是否有 `andosi_flower`。
   - 有 → 加载 `White_NPC03_3.json`，进入交花流程。
   - 无 → 加载默认对话（提示还没采到花）。
3. 对话中交出 `andosi_flower`（`LoseItem`），获得 `antony_button`（`GetItem`）。

---

## 伪代码

```js
let questAccepted = false;
let flowerPicked = false;

// White_NPC03_2.json 对话结束时
function onQuestAccepted() {
    questAccepted = true;
    ShowNpc("andosi_flower"); // 激活花朵物体
}

// 阶段二：采摘安朵丝
function onAndosiFlowerInteract() {
    if (!questAccepted || flowerPicked) return;

    playSfx("pick_flower");
    GetItem("andosi_flower");
    HideNpc("andosi_flower");
    flowerPicked = true;
}

// 阶段三：与 White_NPC03 对话
function onWhiteNpc03Interact() {
    if (questAccepted && flowerPicked) {
        // 已采花 → 加载交花对话
        DialogueManager.Instance.StartWith("White_NPC03_3.json", whiteNpc03Source);
    } else if (questAccepted && !flowerPicked) {
        // 已接任务但未采花 → 提示
        showHint("镇子北边的乱石堆，应该就在那里。");
    } else {
        // 未接任务 → 加载默认对话
        DialogueManager.Instance.StartWith("White_NPC03_2.json", whiteNpc03Source);
    }
}

//其实这个只算一个小任务，不能算是一个puzzle，不过管他的