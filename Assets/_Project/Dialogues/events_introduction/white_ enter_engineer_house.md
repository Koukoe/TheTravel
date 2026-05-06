# enter_engineer_house 事件流程说明

**实现目的**：玩家完成河流分开仪式后，前往工程师家与门互动，进入室内自动触发与工程师和老船长的对话。

---

## 涉及物体

| 物体                      | 说明                         |
| ------------------------- | ---------------------------- |
| `engineer_house_door`     | 工程师家的门，玩家交互入口   |
| `engineer_house_interior` | 工程师家室内场景/区域        |
| `engineer`                | 工程师 NPC                   |
| `captain`                 | 老船长，此前已瞬移至工程师家 |

---

## 步骤流程

### 阶段一：与门交互

1. 玩家靠近工程师家，与 `engineer_house_door` 交互。
2. 播放开门音效（`door_open`）。
3. 播放开门动画（`door_open_anim`）。

### 阶段二：进入室内

1. 禁用玩家输入（`LockPlayerInput`）。
2. 摄像机过渡（淡入淡出或推进至室内视角）。
3. 玩家角色移动至室内指定位置（`room_entry_position`）。
4. 恢复玩家输入（`UnlockPlayerInput`）。

### 阶段三：触发对话

1. 自动弹出对话框，加载对话文件 `White_engineer_house.json`。
2. 对话内容待策划提供。
3. 对话结束后的剧情推进由该 JSON 内部的 `nextId` / `effects` 控制。

---

## 伪代码

```js
let doorOpened = false;

function onEngineerHouseDoorInteract() {
    if (doorOpened) {
        // 已进入过，直接加载对话
        DialogueManager.Instance.StartWith("White_engineer_house.json", engineerHouseSource);
        return;
    }

    doorOpened = true;
    playSfx("door_open");
    playActionAndResume("door_open_anim", () => {
        LockPlayerInput();
        movePlayerTo("room_entry_position");
        moveCamera("room_view", () => {
            UnlockPlayerInput();
            DialogueManager.Instance.StartWith("White_engineer_house.json", engineerHouseSource);
        });
    });
}
