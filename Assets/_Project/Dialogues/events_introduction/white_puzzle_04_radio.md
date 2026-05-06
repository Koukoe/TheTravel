# radio_puzzle 事件流程说明

**实现目的**：玩家与镇上的大型收音机互动，通过调节旋钮匹配波形图，修复收音机并播放古典乐，最终获得唱片道具。该谜题是获取“旧世界声音记录”的关键步骤。

---

## 涉及物体

| 物体              | 说明                                         |
| ----------------- | -------------------------------------------- |
| `radio_body`      | 大型收音机主体，初始覆盖藤蔓                 |
| `radio_vine`      | 覆盖收音机的藤蔓，首次互动后消失             |
| `radio_display`   | 收音机显示屏，显示固定波形与当前波形         |
| `radio_knob_freq` | 频率旋钮，每次互动旋转 90°，改变当前波形频率 |
| `radio_knob_amp`  | 振幅旋钮，每次互动旋转 90°，改变当前波形振幅 |
| `radio_disc`      | 唱片道具，解谜成功后从收音机缝隙弹出         |

---

## 步骤流程

### 阶段一：清理藤蔓

1. 玩家首次与 `radio_body` 交互。
2. 播放清理藤蔓动画/音效（`vine_clear`）。
3. `radio_vine` 从场景隐藏（`HideNpc`）。
4. 显示 `radio_display`，激活 `radio_knob_freq` 与 `radio_knob_amp`（`ShowNpc`）。
5. 显示屏亮起，显示固定波形图与初始当前波形图。

### 阶段二：调节旋钮

1. 玩家每次与 `radio_knob_freq` 交互：
   - 旋钮旋转 90°（动画 `knob_turn`）。
   - 播放旋钮音效（`knob_click`）。
   - 当前波形图频率按档位更新（共 4 档，循环）。
2. 玩家每次与 `radio_knob_amp` 交互：
   - 旋钮旋转 90°（动画 `knob_turn`）。
   - 播放旋钮音效（`knob_click`）。
   - 当前波形图振幅按档位更新（共 4 档，循环）。
3. 每次调节后，显示屏刷新当前波形图。

### 阶段三：波形匹配判定

1. 系统持续比对当前波形与固定波形：
   - 频率与振幅均匹配 → 解谜成功，进入阶段四。
   - 不匹配 → 继续等待玩家调节。
2. 匹配条件：`currentFreqIndex == targetFreqIndex && currentAmpIndex == targetAmpIndex`。

### 阶段四：解谜成功

1. 播放古典乐（`PlayBgm` → `classical_music`，淡入 1.0 秒）。
2. 收音机播放特效动画（`radio_effect`）。
3. 唱片从收音机缝隙弹出（`ShowNpc` → `radio_disc`）。
4. 播放弹出音效（`disc_pop`）。
5. 玩家获得道具 `radio_disc`（`GetItem`），`radio_disc` 物体隐藏（`HideNpc`）。
6. 显示屏变暗（隐藏或材质切换），旋钮锁定不可交互。
7. 音乐可继续播放或逐渐停止（按需求配置）。

---

## 伪代码

```js
// 旋钮档位配置
const KNOB_STATES = 4; // 0, 1, 2, 3 四档
let currentFreqIndex = 0;
let currentAmpIndex = 0;
const targetFreqIndex = 2; // 策划配置固定波形目标
const targetAmpIndex = 1;

let vineCleared = false;
let puzzleSolved = false;

// 阶段一：清理藤蔓
function onRadioBodyInteract() {
    if (puzzleSolved) return;

    if (!vineCleared) {
        playSfx("vine_clear");
        HideNpc("radio_vine");
        ShowNpc("radio_display");
        ShowNpc("radio_knob_freq");
        ShowNpc("radio_knob_amp");
        vineCleared = true;
        refreshDisplay();
        return;
    }

    // 藤蔓已清除 → 提示
    showHint("也许需要调节一下旋钮。");
}

// 阶段二：调节频率旋钮
function onKnobFreqInteract() {
    if (!vineCleared || puzzleSolved) return;

    playSfx("knob_click");
    playAction("knob_turn", "radio_knob_freq");
    currentFreqIndex = (currentFreqIndex + 1) % KNOB_STATES;
    refreshDisplay();
    checkMatch();
}

// 阶段二：调节振幅旋钮
function onKnobAmpInteract() {
    if (!vineCleared || puzzleSolved) return;

    playSfx("knob_click");
    playAction("knob_turn", "radio_knob_amp");
    currentAmpIndex = (currentAmpIndex + 1) % KNOB_STATES;
    refreshDisplay();
    checkMatch();
}

// 阶段三：波形匹配判定
function checkMatch() {
    if (currentFreqIndex === targetFreqIndex && currentAmpIndex === targetAmpIndex) {
        onPuzzleSolved();
    }
}

// 阶段四：解谜成功
function onPuzzleSolved() {
    puzzleSolved = true;

    // 播放古典乐
    playBgm("classical_music", 1.0);
    // 收音机特效
    playActionAndResume("radio_effect", () => {
        // 弹出唱片
        ShowNpc("radio_disc");
        playSfx("disc_pop");
        GetItem("radio_disc");
        HideNpc("radio_disc");
        // 显示屏变暗
        HideNpc("radio_display");
        // 锁定旋钮
        disableKnobs();
    });
}

// 刷新显示屏波形
function refreshDisplay() {
    updateWaveformDisplay(currentFreqIndex, currentAmpIndex, targetFreqIndex, targetAmpIndex);
}


//说明与扩展
//旋钮每次旋转 90°，对应 4 档（0°/90°/180°/270°），档位值分别为 0,1,2,3。

//固定波形目标值（targetFreqIndex、targetAmpIndex）由策划配置，可设为随机或固定。

//显示屏波形刷新需美术提供不同档位的波形图素材，或由程序动态生成。

//古典乐 classical_music 的 BGM ID 需与音频资源表对齐。

//唱片道具 radio_disc 为后续剧情关键物品，需与物品系统对齐 ID。

//谜题完成后旋钮锁定，防止重复交互；如需重置，可在特定事件中调用 resetPuzzle()。

//若音乐需在唱片弹出后停止，可在 onPuzzleSolved 末尾添加 StopBgm 效果。

//为什么收音机需要插入唱片，我也不知道
