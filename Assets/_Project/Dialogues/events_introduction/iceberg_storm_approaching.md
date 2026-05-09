# storm_approaching 氛围事件流程说明

**实现目的**：玩家驾驶船只接近冰山地图的过程中，动态触发暴风雨氛围——天空出现乌云并开始下雨，伴随随机闪电（50%远处闪电营造氛围，50%在船周安全范围内落点），提升航行途中的紧张感与沉浸感。该事件在航行阶段持续执行，直至抵达冰山或离开触发区域。

---

## 涉及资源

| 资源/物体 | 说明 |
|----------|------|
| `storm_clouds` | 乌云天空贴图/预制体，随靠近冰山逐渐显现 |
| `rain_particles` | 下雨粒子特效 |
| `thunder_sfx` | 雷电音效合集（远处雷声/近处劈落声） |
| `lightning_flash` | 闪电闪光特效（照亮场景） |
| `lightning_bolt` | 闪电劈落模型/管线（闪电柱） |
| `iceberg_trigger_zone` | 距离冰山一定范围的触发器区域 |
| `player_ship` | 玩家船只对象，作为闪电安全圈圆心 |

---

## 步骤流程

### 阶段一：天气渐变为暴风雨

1. 玩家船只驶入距离冰山一定范围的触发器区域。
2. 天空开始渐变切换至乌云贴图/天空盒，同时播放刮风音效（`wind_howl`）。
3. 延迟 1~2 秒后，启动下雨粒子特效（`rain_particles`），雨声渐入。
4. 乌云+下雨持续。

### 阶段二：闪电随机生成

1. 风暴持续期间，按固定时间间隔（如每 3~8 秒随机一次）触发闪电。
2. 每次闪电执行以下逻辑：
   - 生成随机数 `r ∈ [0, 1)`。
   - **50% 远处闪电**：`r < 0.5` → 从预设较远的安全区域随机选点生成闪电，播放远景雷声音效（`thunder_far`）。
   - **50% 船周闪电**：`r >= 0.5` → 在以玩家船只为圆心的圆环区域内随机选点生成闪电（圆环内/外径确保不会与船体穿模）。播放近处雷声音效（`thunder_close`）并伴随视野闪光（`lightning_flash`）。
3. 闪电生成流程：
   - 确定落点坐标，在落点生成 `lightning_bolt` 预制体（短时自动销毁）。
   - 同时播放对应音效与屏幕闪光。
   - 闪电结束后等待下次间隔。

### 阶段三：退出风暴

1. 当玩家船只驶出触发器区域（到达冰山泊位/离开范围）：
   - 停止新的闪电生成。
   - 渐变恢复天气（天空切换回正常、雨停、风停）。
   - 事件结束。

---

## 闪电生成规则

| 参数 | 说明 |
|------|------|
| 远处闪电占比 | 50%，落点范围：触发器外围至远处天边 |
| 船周闪电占比 | 50%，落点范围：以船只为圆心的圆环区域 |
| 圆环内径 | 玩家船体碰撞半径 + 安全距离（如半径 10m），保证闪电不穿模 |
| 圆环外径 | 内径 + 可视范围（如半径 80m），保证闪电镜头可见 |
| 间隔时间 | 3~8 秒随机（可根据体验调整） |

---

## 伪代码

```js
let stormActive = false;
let stormTimer = null;

// 阶段一：进入风暴区域
function onEnterStormTrigger() {
    if (stormActive) return;
    stormActive = true;

    // 天气渐变
    transitionSkyTo("storm_clouds");
    playAmbient("wind_howl");
    delay(1000, () => {
        startParticles("rain_particles");
        playAmbient("rain_loop");
    });

    // 开始闪电循环
    scheduleNextLightning();
}

// 阶段二：闪电调度
function scheduleNextLightning() {
    if (!stormActive) return;
    stormTimer = delay(randomRange(3000, 8000), () => {
        generateLightning();
        scheduleNextLightning();
    });
}

function generateLightning() {
    const r = random();
    if (r < 0.5) {
        // 50% 远处闪电
        const farPos = getRandomFarPosition();
        spawnLightningBolt(farPos);
        playSfx("thunder_far");
    } else {
        // 50% 船周闪电
        const nearPos = getRandomPositionInRing(
            shipPosition,
            innerRadius, // 内径
            outerRadius  // 外径
        );
        spawnLightningBolt(nearPos);
        playSfx("thunder_close");
        flashScreen("lightning_flash");
    }
}

function spawnLightningBolt(pos) {
    const bolt = instantiate("lightning_bolt", pos);
    delay(500, () => destroy(bolt));
}

// 阶段三：退出风暴区域
function onExitStormTrigger() {
    stormActive = false;
    if (stormTimer) clearTimer(stormTimer);

    stopParticles("rain_particles");
    stopAmbient("rain_loop");
    stopAmbient("wind_howl");
    transitionSkyTo("normal");
}