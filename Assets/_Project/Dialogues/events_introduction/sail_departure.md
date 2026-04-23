# sail_departure 事件流程说明

实现目的：玩家在对话中选择出发时，统一执行“铁公鸡喊话、船铃音效、离港动画、切大海地图”等流程。  
此流程作为全局通用“离港”事件，每次出发都可直接通过 TriggerEvent 调用。

---

## 事件参数

- targetIsland（string，可选）：指定要驶向的目的地岛屿英文ID

---

## 步骤流程

1. 铁公鸡显示对白：“出发——！！”
2. 播放船铃音效（ship_bell）
3. 播放离港动画（leave_port）
4. 动画播放期间，暂时禁用玩家输入
5. 动画播放结束后，切换场景至“sea_map”（大海地图界面），并携带目标岛屿参数

---

## 伪代码示例

```js
function sail_departure(targetIsland) {
    // 1. 台词弹窗，显示“铁公鸡：出发——！！”
    showDialogue({ character: "cock", content: "出发——！！" });

    // 2. 播放船铃音效
    playSfx("ship_bell");

    // 3. 播放离港动画
    playCutscene("leave_port", function onAnimOver() {
        // 4. 切换到大海地图，并用参数targetIsland指定目标
        gotoScene("sea_map", { target: targetIsland });
    });
}
```

---

## 说明和扩展

- 如需多语言，可统一维护台词ID资源。
- 动画名称、音效名请与资源表对齐。
- 若有交互禁止需求，可在动画开始与结束间配合LockPlayerInput/UnlockPlayerInput。

---

## 触发方法示例（对白json中）

```json
"effects": [
  { "TriggerEvent": "sail_departure", "targetIsland": "colores" }
]
```

---