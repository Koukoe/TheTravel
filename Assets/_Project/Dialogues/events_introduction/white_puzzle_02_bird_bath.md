# white_puzzle_02_bird_bath 事件流程说明

**实现目的**：玩家在 White_NPC01 旁发现干涸的鸟浴池和黄金圣杯，通过“取杯→装水→倒水”完成解谜，触发鸟浴池喷水特效并引来小鸟。黄金圣杯最终变为普通木杯。

---

## 涉及物体

| 物体 | 说明 |
|------|------|
| `bird_bath` | 干涸的鸟浴池，初始状态无水 |
| `golden_cup` | 黄金圣杯，初始放置在鸟浴池上 |
| `golden_cup_filled` | 装满水的黄金圣杯（装水后获得） |
| `wooden_cup` | 普通木杯，解谜成功后获得（替代黄金圣杯） |
| `fountain` | 镇子中央的喷泉，可互动装水 |

---

## 步骤流程

### 阶段一：拿起圣杯

1. 玩家靠近鸟浴池，交互 `golden_cup`。
2. 播放拾取动画/音效（`item_pickup`）。
3. 显示对话 `bird_bath_puzzle_1.json`（主角内心独白）。
4. 获得道具 `golden_cup`（`GetItem`），`golden_cup` 物体从场景隐藏（`HideNpc`）。
5. 鸟浴池进入“等待装水”状态。

### 阶段二：装水

1. 玩家携带 `golden_cup` 前往 `fountain` 处交互。
2. 条件检查：背包中是否有 `golden_cup`。
   - 有 → 播放装水动画/音效（`fill_water`）。
   - 无 → 提示“也许需要找个容器来装水”。
3. 移除 `golden_cup`（`LoseItem`），获得 `golden_cup_filled`（`GetItem`）。

### 阶段三：倒水

1. 玩家回到 `bird_bath` 处交互。
2. 条件检查：背包中是否有 `golden_cup_filled`。
   - 有 → 进入步骤 3。
   - 无 → 提示“这里有个干涸的鸟浴池”。
3. 播放倒水动画/音效（`pour_water`）。
4. 移除 `golden_cup_filled`（`LoseItem`），获得空的 `golden_cup`（`GetItem`），表示水已倒掉。

### 阶段四：解谜成功

1. 鸟浴池播放喷水特效（`bird_bath_water_effect`）。
2. 播放水花音效（`water_splash`）。
3. 触发小鸟飞来动画（`birds_flying_in`）。
4. 播放鸟鸣音效（`birds_chirping`）。
5. 移除空黄金圣杯（`LoseItem("golden_cup")`），获得普通木杯（`GetItem("wooden_cup")`）。
6. 鸟浴池状态锁定为“已解谜”，不可重复触发。

---

## 伪代码

```js
let cupTaken = false;
let cupFilled = false;
let puzzleSolved = false;

// 阶段一：拿起圣杯
function onGoldenCupInteract() {
    if (cupTaken || puzzleSolved) return;

    playSfx("item_pickup");
    showDialogue("bird_bath_puzzle_1.json", () => {
        GetItem("golden_cup");
        HideNpc("golden_cup");
        cupTaken = true;
    });
}

// 阶段二：装水
function onFountainInteract() {
    if (!cupTaken || cupFilled || puzzleSolved) {
        if (!cupTaken) showHint("也许需要找个容器来装水");
        return;
    }

    playSfx("fill_water");
    LoseItem("golden_cup");
    GetItem("golden_cup_filled");
    cupFilled = true;
}

// 阶段三：倒水
function onBirdBathInteract() {
    if (puzzleSolved) return;

    if (!cupFilled) {
        if (!cupTaken) showHint("这里有个干涸的鸟浴池，上面好像有个杯子");
        else showHint("也许需要往里面倒点水");
        return;
    }

    playSfx("pour_water");
    LoseItem("golden_cup_filled");
    GetItem("golden_cup");  // 倒空后杯子变回空圣杯
    cupFilled = false;
    onPuzzleSolved();
}

// 阶段四：解谜成功
function onPuzzleSolved() {
    puzzleSolved = true;

    playActionAndResume("bird_bath_water_effect", () => {
        playSfx("water_splash");
        playActionAndResume("birds_flying_in", () => {
            playSfx("birds_chirping");
            // 黄金圣杯变为木杯
            LoseItem("golden_cup");
            GetItem("wooden_cup");
        });
    });
}
-说明与扩展
-黄金圣杯在流程中物品 ID 变化：golden_cup → golden_cup_filled → golden_cup → 最终失去，获得 wooden_cup。

-所有 GetItem / LoseItem 字段需与物品系统对齐道具 ID。

-喷水特效 bird_bath_water_effect 和小鸟动画 birds_flying_in 需动画师配合制作。

-白鸽或小鸟模型可预先隐藏在场景中，解谜成功时通过 ShowNpc 显示。

-该解谜不直接推进任何 NPC 对话索引，后续对话阶段推进由其他事件或对话控制。