# white_puzzle_01_stone_puzzle 事件流程说明

**实现目的**：玩家在石碑前按正确顺序敲击石头，完成解谜后触发石碑特效，并推进石碑对话阶段，使下次互动显示新对话（`White_StoneTablet_02.json`）。该事件由石头点击驱动，不通过对话 effects 触发。

---

## 事件参数

| 参数名          | 类型           | 说明                                     |
| --------------- | -------------- | ---------------------------------------- |
| `puzzleId`      | string（可选） | 解谜标识，固定为 `"stone_tablet_melody"` |
| `stoneSequence` | int[]          | 正确敲击顺序，例如 `[1, 3, 2, 4]`        |

---

## 步骤流程

1. 玩家点击某块石头，石头播放对应音调（音效 ID 如 `stone_note_1`）。
2. 系统将该石头 ID 追加到玩家输入序列末尾。
3. 将当前输入序列与正确序列 `stoneSequence` 逐位比对：
   - 若长度超限或某位不匹配 → 清空输入，播放错误音效（`stone_wrong`），石头恢复初始。
   - 若完全匹配 → 解谜成功，进入步骤 4。
4. 解谜成功：
   - 石碑播放成功特效（调用 `PlayActionAndResume` 或其他特效接口）。
   - 播放成功音效（`puzzle_solved`）。
   - 推进石碑对话索引至 `1`（加载 `White_StoneTablet_02.json`）。
   - 锁定所有石头，禁止再次交互。

---

## 伪代码

```js
// 石头配置
const stones = [
    { id: 1, noteSfx: "stone_note_1", obj: stoneObj1 },
    { id: 2, noteSfx: "stone_note_2", obj: stoneObj2 },
    { id: 3, noteSfx: "stone_note_3", obj: stoneObj3 },
    { id: 4, noteSfx: "stone_note_4", obj: stoneObj4 }
];

const correctSequence = [1, 3, 2, 4];
let playerInput = [];
let puzzleSolved = false;

function onStoneClicked(stoneId) {
    if (puzzleSolved) return;

    playSfx(getStoneSfx(stoneId));
    playerInput.push(stoneId);

    const idx = playerInput.length - 1;
    if (playerInput[idx] !== correctSequence[idx]) {
        playerInput = [];
        playSfx("stone_wrong");
        return;
    }

    if (playerInput.length === correctSequence.length) {
        puzzleSolved = true;
        onPuzzleSolved();
    }
}

function onPuzzleSolved() {
    playActionAndResume("stone_tablet_effect", () => {
        // 推进石碑对话索引
        stoneTabletDialogueOnObj.SetDialogueIndex(1);
        playSfx("puzzle_solved");
        stones.forEach(s => s.obj.enabled = false);
    });
}

//这写的是啥我看不懂一点woc
//不过大概内容就是纯白小镇的第一个解谜的流程，在断桥旁边的石碑旁边会有一堆大小不一的石头，玩家与石头互动就会发出音调不同的敲击声，玩家按照顺序互动后石碑就会产生特效，然后和石碑的对话状态跳进02
