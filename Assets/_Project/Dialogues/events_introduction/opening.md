# opening_sequence 开场流程说明

**实现目的**：游戏启动后自动播放开场过场（海面场景、镜头推进、主角与铁公鸡对话），随后交由玩家操控主角与船舵互动并驾驶船只驶向科洛雷斯岛，接近岛屿时触发入岛动画，完成开场流程并进入下一阶段。

---

## 涉及资源

| 资源/物体 | 说明 |
|----------|------|
| `sea_scene` | 开场海面场景，包含小船、海面、天空等 |
| `ocean_ambient` | 环境音效（海浪声、海鸥叫声） |
| `cock` | 铁公鸡 NPC，位于船上 |
| `protagonist` | 主角，开场时靠在栏杆上 |
| `ship_wheel` | 船舵交互物体，玩家需与之互动 |
| `colores_island` | 科洛雷斯岛目标位置触发器 |
| `enter_island_cutscene` | 入岛动画资源 |

---

## 步骤流程

### 阶段一：自动过场（开场镜头与对话）

1. 场景加载完毕，播放环境音效（海浪声、海鸥叫声）。
2. 摄像机从远景平滑推进，聚焦至小船甲板，主角靠在栏杆上看海。
3. 自动弹出对话框，加载并播放 `opening1.json` 对话：
   - 铁公鸡催促主角启航。
   - 主角回应说想发呆。
   - 铁公鸡继续催促。
   - 主角妥协，对话结束。
4. 对话结束后，摄像机恢复至玩家跟随视角。
5. 玩家获得角色操控权，进入阶段二。

### 阶段二：与船舵互动

1. 玩家操控主角走到 `ship_wheel` 附近并交互。
2. 播放交互动画/音效（如抓握舵轮）。
3. 自动弹出对话框，加载并播放 `opening2.json` 对话：
   - 铁公鸡指示方向：“照着我指的方向前进吧！去科洛雷斯岛！”
4. 对话结束后，玩家获得船只操控权，进入阶段三。

### 阶段三：驾驶船只驶向岛屿

1. 玩家控制船只移动，海面导航开启。
2. 在场景中设置隐形的触发器区域，代表“接近科洛雷斯岛”的范围。
3. 当船只进入该触发器范围：
   - 禁用玩家输入（`LockPlayerInput`）。
   - 切换摄像机至过场视角。
   - 播放入岛动画（`enter_island_cutscene`）。
4. 动画播放完毕后：
   - 恢复玩家输入（`UnlockPlayerInput`）。
   - 开场流程结束，后续剧情或场景加载由主线脚本接管。

---

## 伪代码

```js
let phase = "opening_camera"; // opening_camera | waiting_wheel | waiting_sail | finished

function onSceneLoaded() {
    playAmbient("ocean_ambient");
    moveCamera("opening_shot", () => {
        // 镜头到位后开始对话
        startDialogue("opening1.json", onOpeningDialogueEnd);
    });
}

function onOpeningDialogueEnd() {
    moveCamera("player_follow");
    enablePlayerInput();
    phase = "waiting_wheel";
}

function onWheelInteract() {
    if (phase !== "waiting_wheel") return;

    playActionAndResume("grab_wheel", () => {
        startDialogue("opening2.json", onWheelDialogueEnd);
    });
}

function onWheelDialogueEnd() {
    phase = "waiting_sail";
    enableShipControl();
    showNavigationHint("向科洛雷斯岛前进！");
}

function onShipEnterIslandTrigger() {
    if (phase !== "waiting_sail") return;
    phase = "finished";

    disablePlayerInput();
    playCutscene("enter_island_cutscene", () => {
        enablePlayerInput();
        // 开场完成，触发后续主线
        triggerMainStory();
    });
}