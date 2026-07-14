---
id: kd_541060ff-fce8-4bf6-a769-d5f7515a85e0
type: design
path: undead-survivor-dgame-migration-plan.md
title: undead-survivor-dgame-migration-plan
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
inheritAiConfig: true
createdAt: 1783867421830
updatedAt: 1783959422552
---

# undead-survivor-dgame-migration-plan

## Summary
Undead Survivor 迁移到 DGame Demo2 槽位的当前实施方案：当前项目不再按三消 Demo2 理解，Demo2 仅保留为对外入口与资源地址，实际玩法语义为 Undead Survivor；方案记录已落地入口、当前缺口、第一阶段闭环和后续框架化方向。

## Content
# Undead Survivor DGame 迁移方案

## 当前实况（2026-07-14）

当前项目已经进入 Undead Survivor 接入阶段，不再以旧 Demo2 三消玩法作为当前状态依据。`Demo2` 只作为 DGame 主界面入口、场景 location 和既有 UI 资源槽位保留；业务语义、代码命名和后续实现都以 `Survivor*` 为准。

已落地内容：

- `MainWindow` 的开始按钮已调用 `SurvivorFlowController.EnterAsync(this)`。
- `SurvivorFlowController` 已通过 `GameModule.SceneModule.LoadSceneAsync("Demo2", LoadSceneMode.Additive)` 加载场景，并在退出时卸载 `Demo2`、关闭局内 UI、回到 `MainWindow`。
- `SurvivorSession` 已作为局内生命周期入口，负责查找 `SurvivorBattleRoot/Player`、重置玩家、打开 HUD、退出和重开时清理 UI。
- `SurvivorPlayerController` 已提供基于 New Input System 的键盘/手柄移动输入和 Rigidbody2D 位移。
- `GameUnity/Assets/BundleAssets/Scenes/Demo2.unity` 已从 `CompleteScene.unity` 裁剪复用 `Ground`、`Player`、`Main Camera`、`Virtual Camera`、`Global Light 2D`、`Global Volume`，并统一挂到 `SurvivorBattleRoot` 下。
- `Virtual Camera` 已跟随 `SurvivorBattleRoot/Player`；`Main Camera` 保留 URP、Pixel Perfect、Cinemachine Brain 等表现组件，但已剔除 `AudioListener`，避免和启动场景重复。
- `ISurvivorUI`、`SurvivorHUDWindow`、`SurvivorResultWindow` 已接入 DGame 接口事件和 `UIWindow` 生命周期；当前 HUD/Result 临时复用 `Demo2HUDWindow`、`Demo2ResultWindow` 资源。
- `SurvivorEnemy`、`SurvivorBullet0`、`SurvivorBullet1` 已作为第一批 Prefab 落位到 `GameUnity/Assets/BundleAssets/Prefabs/Survivor`。
- 已从 `Demo2.unity` 移除旧 `Spawner` 和四个旧 `Reposition` 运行组件，保留刷怪点、Tilemap、Collider 和表现资源。
- 已从 `SurvivorEnemy.prefab` 移除旧 `Enemy` / `Reposition`，从 `SurvivorBullet0.prefab`、`SurvivorBullet1.prefab` 移除旧 `Bullet`，保留表现与碰撞资源。
- 已从 `Demo2.unity` 的 `Hand Left` / `Hand Right` 移除旧 `Hand`，保留 Transform 和 SpriteRenderer。
- 已扫描 `Demo2.unity`、`SurvivorEnemy.prefab`、`SurvivorBullet0.prefab`、`SurvivorBullet1.prefab`，确认没有 `Assets/Undead Survivor/Complete/Codes` 旧脚本 GUID 残留。
- AIBridge `compile unity` 通过，Play Mode 进入 `Demo2.unity` 后 Error 日志为空，旧 `Spawner.Awake` / `Reposition.OnTriggerExit2D` 空引用已消失。

当前关键缺口：

- 第一批 Enemy/Bullet Prefab 已迁入 `BundleAssets`，旧运行脚本已移除，但尚未接入新的 `SurvivorEnemyController`、`SurvivorProjectileController`、对象池预热和 Spawn/Recycle 语义。
- 当前只具备入口、场景加载、玩家移动和 HUD 文本刷新骨架；敌人、武器、经验、升级、暂停、胜负和结果流程尚未形成完整一局。
- `SurvivorSelectWindow`、`SurvivorLevelUpWindow` 尚未落地；当前 UI 仍是临时复用 Demo2 HUD/Result 资源。
- 如果工作区仍存在旧 `Demo2*` 三消代码或资源，只视为历史残留，不作为当前玩法设计依据；是否清理取决于它们是否影响编译、资源地址或认知成本。

## 目标

将 `Assets/Undead Survivor` 中的类 Vampire Survivors Demo 接入当前 DGame 框架，实现：

- 保留原 Demo 的核心玩法体验与主要美术表现。
- 使用 DGame 的启动链、YooAsset、SceneModule、UIModule、AudioModule、GameObjectPoolModule 和热更新业务分层。
- 将原有高度场景化、单例化的运行结构重建为可维护、可扩展的框架化实现。
- 采用分阶段完整重构方式，保证每个阶段都可运行、可验证。

## 非目标

以下内容不作为第一阶段目标：

- 不直接复用旧 `GameManager`、`PoolManager`、`AudioManager` 作为正式运行架构。
- 不把 Undead 业务代码放进 `DGame.Runtime`。
- 不在第一阶段一次性完成所有 Luban 数据化。
- 不要求首期对原 Demo 做完全逐脚本兼容迁移。

## 总体策略

采用“保留资产，重建架构”的迁移思路：

- 保留原 Demo 的 Sprite、Animator、Prefab、Tilemap、音频等资产。
- 不延续 `CompleteScene.unity` 中的整体运行方式。
- 以 DGame 的主启动链进入 Demo2 承载的幸存者玩法流程。
- 直接复用现有 Demo2 的入口、场景资源地址和业务槽位，不新建 Demo3，也不建立并行的独立 Survivor Demo 槽位。
- Demo2 原有三消语义已过期；当前项目只保留 `Demo2` 作为外部槽位和资源 location，玩法内容由 Undead Survivor 承接。
- 内部业务类型可继续使用 `Survivor*` 命名表达领域语义，同时保持对外入口和场景地址为 `Demo2`。
- 按 Demo2 的程序集组织方式建立 `GameBattle + GameLogic (+ GameProto)` 的职责边界。

## 运行架构

### 启动与流程

沿用当前 DGame 启动流程：

`GameStart.unity` → AOT Procedure → `GameStart.Entrance()` → 热更业务入口。

当前幸存者玩法入口控制器：

- `SurvivorFlowController`
- `SurvivorSession`

目标状态流转：

- `Loading`
- `CharacterSelect`
- `Playing`
- `LevelUpPaused`
- `Result`
- `Unloading`

说明：

- `SurvivorFlowController` 负责进入玩法、加载场景、打开窗口、退出与重开。
- `SurvivorSession` 负责局内运行时状态、系统协调和生命周期清理。
- 不再让场景中的某个 MonoBehaviour 兼任全局真相源。
- 当前 `SurvivorFlowController` 已完成 Additive 加载、激活、退出卸载和资源清理；`SurvivorSession` 仍处于骨架阶段，尚未拥有完整状态机。

### 场景组织

采用 Additive 战斗场景方案：

- 常驻：DGame 启动场景与 `UIRoot`
- Additive：资源 location 仍为 `Demo2`，文件为 `GameUnity/Assets/BundleAssets/Scenes/Demo2.unity`

战斗场景只保留局内必须内容，例如：

- Tilemap 地图
- 玩家出生点或场景锚点
- 敌人生成点体系
- 场景内表现对象

不保留以下旧场景责任：

- 场景内整套 Canvas/UI
- 场景级 EventSystem
- 旧 `GameManager`
- 旧 `PoolManager`
- 旧 `AudioManager`

需要避免与 DGame 常驻系统冲突的对象：

- `EventSystem`
- 重复 `AudioListener`
- 启动场景 Camera 职责冲突对象

当前必须先补齐的场景契约：

- 根节点：`SurvivorBattleRoot`
- 玩家节点：`SurvivorBattleRoot/Player`
- `Player` 上需要 `SurvivorPlayerController` 和 Rigidbody2D，并与摄像机/碰撞层配置匹配。
- 场景内可以有局内摄像机和表现用灯光，但不得携带重复 EventSystem 和场景 UI 根 Canvas。
- 当前 `Demo2.unity` 已满足根节点、玩家节点、玩家 Rigidbody2D、玩家控制脚本、CompleteScene 地图/灯光/Volume 复用和 Cinemachine 跟随配置；Play Mode 运行效果仍需在 Unity Editor 中确认。

### 分层职责

#### GameBattle

负责纯规则与数据计算，尽量不依赖 Unity API：

- 角色成长规则
- 武器/装备数值
- 伤害计算
- 升级候选抽取
- 波次与刷怪参数
- 经验与升级阈值
- 胜负判定条件

#### GameLogic

负责 Unity 表现与业务协调：

- `SurvivorFlowController`
- `SurvivorSession`
- 战斗场景桥接
- Player / Enemy / Bullet MonoBehaviour
- 输入桥接
- UI 打开与刷新
- 音频播放
- 场景加载与卸载
- 事件分发与订阅

#### GameProto

后续承接配置表：

- 角色配置
- 敌人配置
- 波次配置
- 经验配置
- Item/Weapon/Gear 配置
- 音效与文本配置

第一阶段允许先使用代码内临时数据结构复刻原玩法参数，后续再迁移到 Luban。

## 系统映射

原 Demo 到 DGame 的目标映射如下：

- `GameManager.instance` → `SurvivorSession`
- `PoolManager` → `IGameObjectPoolModule`
- `AudioManager` → `IAudioModule`
- 场景 Canvas HUD → `UIWindow` 体系
- 直接耦合 HUD 刷新 → `GameEvent` 事件驱动
- `SceneManager.LoadScene(0)` → `ISceneModule` 的加载/卸载/重进流程
- 原本硬编码数值 → 后续迁往 `GameProto/Luban`
- 原 `PlayerPrefs` 解锁 → DGame 存档系统

## 旧组件禁用/移除与重写方案

### AIBridge 诊断结论

2026-07-14 通过 AIBridge 在 `Assets/BundleAssets/Scenes/Demo2.unity` 中进入 Play Mode，已确认当前运行时错误不是编译错误，而是旧 Undead Demo 组件残留：

- `Goldmetal.UndeadSurvivor.Spawner.Awake()` 在 `Assets/Undead Survivor/Complete/Codes/Spawner.cs:19` 访问 `GameManager.instance.maxGameTime`，但当前 DGame 版 `Demo2` 不保留旧 `GameManager`，因此空引用。
- `Goldmetal.UndeadSurvivor.Reposition.OnTriggerExit2D()` 在 `Assets/Undead Survivor/Complete/Codes/Reposition.cs:21` 访问 `GameManager.instance.player`，同样依赖旧场景单例。

处理原则：

- 不补假的 `GameManager`、`PoolManager` 或 `AudioManager`。
- 不在旧脚本里加空判来静默跳过错误。
- 不把旧 `CompleteScene` 运行链搬进 DGame。
- 只保留旧 Demo 的美术、Prefab、Tilemap、Animator、碰撞器和数值经验；运行职责全部迁移到新的 `Survivor*` 代码。
- 对包含旧单例访问的 `Awake` 脚本，单纯禁用 MonoBehaviour 不足以止血；必须移除旧组件，因为 Unity 仍会调用 disabled MonoBehaviour 的 `Awake`。

### 禁用与移除清单

| 场景或资源位置 | 旧组件 | 当前问题 | 处理动作 | 新系统接管 |
|---|---|---|---|---|
| `SurvivorBattleRoot/Player/Spawner` | `Goldmetal.UndeadSurvivor.Spawner` | `Awake` 依赖 `GameManager.instance`，`Spawn()` 依赖旧 `PoolManager.Get(0)` | 已移除 `Spawner` MonoBehaviour；保留 `Spawner` Transform 与子 `Point` 作为刷怪点锚点 | `SurvivorSpawnSystem` 读取锚点，通过 `GameModule.GameObjectPool` 生成敌人 |
| `SurvivorBattleRoot/Ground/Tilemap (0..3)` | `Goldmetal.UndeadSurvivor.Reposition` | `OnTriggerExit2D` 依赖 `GameManager.instance.player` | 已移除 `Reposition` MonoBehaviour；保留 Tilemap、Collider 和表现资源 | `SurvivorMapLoopController` 根据玩家位置重排地图块；第一阶段也可暂缓地图循环 |
| `SurvivorBattleRoot/Player/Hand Left/Right` | `Goldmetal.UndeadSurvivor.Hand` | 旧手部表现脚本仍属于旧运行链残留 | 已移除 `Hand` MonoBehaviour；手部 Transform 和 SpriteRenderer 保留 | 后续由 `SurvivorWeaponSystem` 或独立表现控制器接管 |
| `BundleAssets/Prefabs/Survivor/SurvivorEnemy` | 旧 `Enemy` 及旧脚本依赖 | 依赖 `GameManager.instance`、旧音频、旧对象池和 `SetActive(false)` 回收语义 | 已移除旧 `Enemy` / `Reposition`；保留 Sprite、Animator、Collider、Rigidbody2D | 敌人由 Session 注入目标和参数，死亡后通过对象池回收 |
| `BundleAssets/Prefabs/Survivor/SurvivorBullet0/1` | 旧 `Bullet` | 依赖 Tag、旧穿透计数和 `SetActive(false)` 回收语义 | 已移除旧 `Bullet`；保留弹体表现和碰撞器 | 子弹命中 `SurvivorEnemyController` 后按穿透次数回收 |
| `Assets/Undead Survivor/Complete/Codes/*` | 旧 Demo 源码 | 可作为迁移参考，但不属于 DGame 正式运行链 | 第一阶段不删除源码；禁止挂到 `Demo2` 正式运行对象上 | 逐项迁移到 `GameLogic/Survivor` 与后续 `GameBattle` |

禁用与移除策略：

- 第一刀已升级为“移除旧 MonoBehaviour、保留场景锚点和表现资源”，因为 `Spawner.Awake` 即使在组件 disabled 时仍会执行。
- 后续新增 `Survivor*` 系统时，不复用旧脚本组件位置，只把保留的 Transform、Collider、Sprite、Animator 作为表现和锚点资源使用。
- 如果某个旧组件只作为参考源码存在，不挂在 `Demo2` 或运行 Prefab 上，可以保留到迁移完成后统一清理。

### 新系统职责拆分

#### `SurvivorSession`

当前已存在，后续扩展为局内唯一生命周期入口：

- 拥有 `Playing`、`PausedForLevelUp`、`Result`、`Destroying` 等状态。
- 持有 `SurvivorBattleContext`、`SurvivorSpawnSystem`、`SurvivorWeaponSystem`、`SurvivorProgressionSystem`。
- 统一创建和销毁对象池，退出或重开时清理 active 敌人、子弹和计时器。
- 通过 `ISurvivorUI` 发送 HUD、升级和结果事件，不让 UI 直接读场景单例。

#### `SurvivorBattleContext`

建议新增到 `Assets/Scripts/HotFix/GameLogic/Survivor/SurvivorBattleContext.cs`：

- 保存 `BattleRoot`、`Player`、刷怪点、对象池父节点等场景引用。
- 保存局内动态数据：时间、击杀、经验、等级、生命值、是否暂停。
- 对外暴露只读查询；状态变更通过 Session 或系统方法完成，避免散落全局可变状态。

#### `SurvivorSpawnSystem`

建议新增到 `Assets/Scripts/HotFix/GameLogic/Survivor/SurvivorSpawnSystem.cs`：

- 从保留的 `SurvivorBattleRoot/Player/Spawner` Transform 子节点收集刷怪点。
- 第一阶段使用代码内 `SurvivorSpawnWave` 临时数据复刻原 `SpawnData`：`spawnTime`、`spriteType`、`health`、`speed`。
- 根据 `SurvivorBattleContext.ElapsedTime` 选择当前波次。
- 使用 `GameModule.GameObjectPool.CreateGameObjectPoolAsync("SurvivorEnemy", ...)` 预热敌人池。
- 使用 `GameModule.GameObjectPool.SpawnAsync("SurvivorEnemy", parent, position, rotation, ct)` 生成敌人，并调用 `SurvivorEnemyController.ResetState(...)`。

#### `SurvivorEnemyController`

建议新增到 `Assets/Scripts/HotFix/GameLogic/Survivor/SurvivorEnemyController.cs`：

- 只负责单个敌人的表现和碰撞，不读取 `GameManager.instance`。
- `ResetState` 接收目标 Transform、血量、速度、动画类型和死亡回调。
- `FixedUpdate` 朝玩家移动；暂停或死亡时直接返回。
- `ApplyDamage` 扣血、播放受击表现；死亡时通知 Session 增加击杀和经验，再调用对象池 `Recycle`。
- 不直接调用旧 `AudioManager`；音效由 Session 或后续 `SurvivorAudioSystem` 通过 `GameModule.AudioModule` 处理。

#### `SurvivorProjectileController`

建议新增到 `Assets/Scripts/HotFix/GameLogic/Survivor/SurvivorProjectileController.cs`：

- `ResetState` 接收伤害、穿透次数、方向、速度和回收回调。
- 命中 `SurvivorEnemyController` 后调用 `ApplyDamage`。
- 穿透次数耗尽或离开玩家区域后，通过 `GameModule.GameObjectPool.Recycle(gameObject)` 回收。
- 不使用 `SetActive(false)` 作为业务回收语义。

#### `SurvivorWeaponSystem`

建议新增到 `Assets/Scripts/HotFix/GameLogic/Survivor/SurvivorWeaponSystem.cs`：

- 维护当前武器、攻击间隔、伤害、数量和穿透参数。
- 使用 `Physics2D` 扫描或 Session 维护的敌人列表寻找目标。
- 使用对象池生成 `SurvivorBullet0` / `SurvivorBullet1`。
- 第一阶段先实现一种自动远程攻击，后续再补环绕武器、装备加成和多武器组合。

#### `SurvivorMapLoopController`

建议新增到 `Assets/Scripts/HotFix/GameLogic/Survivor/SurvivorMapLoopController.cs`：

- 接管旧 `Reposition` 的地图块循环效果。
- 构造时注入玩家 Transform 和 Tilemap Transform 列表。
- 当玩家与 Tilemap 中心距离超过阈值时，将对应 Tilemap 平移一个固定地图尺寸。
- 第一阶段如果可玩闭环不依赖无限地图，可以先禁用旧 `Reposition` 并暂缓实现该控制器。

#### `SurvivorProgressionSystem`

建议新增到 `Assets/Scripts/HotFix/GameLogic/Survivor/SurvivorProgressionSystem.cs`：

- 管理经验、等级、升级阈值和升级候选。
- 死亡或 300 秒胜利时通知 Session 进入结果状态。
- 升级时让 Session 暂停战斗系统并打开 `SurvivorLevelUpWindow`。
- 第一阶段可使用代码内临时配置，第二阶段迁移到 Luban。

### 实施顺序

1. 已完成：场景止血，移除 `Spawner`、四个 `Reposition` 组件，保留 Transform、Tilemap、Collider 和美术资源。
2. 已完成：Prefab 止血，移除 `SurvivorEnemy` 上旧 `Enemy/Reposition` 和 `SurvivorBullet0/1` 上旧 `Bullet`。
3. 已完成：移除 Hand Left/Right 上的旧 `Hand`，目标场景和 Survivor Prefab 已无旧 Undead Survivor 脚本 GUID。
4. 已完成：AIBridge Play Mode 验证不再出现旧 `Spawner.Awake` / `Reposition.OnTriggerExit2D` 空引用。
5. 对象池接入：为 `SurvivorEnemy`、`SurvivorBullet0`、`SurvivorBullet1` 创建池，退出和重开时销毁池。
6. 敌人循环：实现 `SurvivorSpawnSystem` + `SurvivorEnemyController`，完成生成、追踪、受伤、死亡、回收。
7. 武器循环：实现 `SurvivorWeaponSystem` + `SurvivorProjectileController`，完成自动攻击和子弹回收。
8. 成长闭环：实现经验、升级暂停、恢复、胜负和结果窗口。
9. 地图循环：如 Play Mode 验证发现玩家很快离开地图，再实现 `SurvivorMapLoopController`；否则延后到第一阶段闭环后。

### 验收标准

- Unity 编译通过，Error 日志为空。
- `Demo2.unity` Play Mode 不再触发旧 `Goldmetal.UndeadSurvivor.*` 运行脚本错误。
- 禁用旧组件后，玩家移动、摄像机跟随、HUD 返回仍可运行。
- 新敌人由 DGame 对象池生成和回收，不依赖旧 `PoolManager.Get(index)`。
- 新战斗流程不依赖旧 `GameManager.instance`、`AudioManager.instance`、`SceneManager.LoadScene(0)`。

## UI 方案

原场景 UI 拆分为独立窗口资源，当前已落地 HUD/Result 骨架：

- `SurvivorHUDWindow`：已存在，当前临时复用 `Demo2HUDWindow` 资源，监听 `ISurvivorUI_Event.OnHudChanged`。
- `SurvivorResultWindow`：已存在，当前临时复用 `Demo2ResultWindow` 资源，监听 `ISurvivorUI_Event.OnResultChanged`。
- `SurvivorSelectWindow`：未落地。
- `SurvivorLevelUpWindow`：未落地。

约束：

- HUD、升级、结果不再依附于战斗场景根 Canvas。
- 高频短生命周期表现（如伤害数字）不走 `UIWindow`，应考虑对象池化表现对象。
- UI 更新通过事件或 Session 数据驱动，不直接到处读取场景单例。
- 临时复用 `Demo2HUDWindow` / `Demo2ResultWindow` 是第一阶段过渡做法；进入正式 UI 拆分时，需要改为 Survivor 语义资源或至少清理旧三消字段命名。

## 输入方案

统一使用 New Input System 桥接，不再混用旧 `Input` 接口。

原则：

- 角色移动输入进入 `GameLogic` 的玩家控制桥。
- 自动攻击逻辑由 Session/武器系统驱动，不依赖按钮触发。
- 若保留移动摇杆，则作为 UI 输入表现层接入。
- 当前 `SurvivorPlayerController` 直接读取 `Keyboard.current` 和 `Gamepad.current`；这是第一阶段移动闭环实现。若后续项目要统一输入抽象，应收口到 DGame 已有 `GameModule.Input` / `GameModule.InputModule`，不要回退到旧 `UnityEngine.Input`。

## 对象池方案

使用 `IGameObjectPoolModule` 替代旧数组索引池。

适合池化的对象：

- Enemy
- Bullet / Projectile
- 掉落表现
- 特效表现
- 伤害数字或短生命周期物体

要求：

- 以稳定资源地址区分池对象，不再依赖数组下标协议。
- Spawn/Recycle 时由业务显式重置状态，不依赖旧对象残留状态。
- 迁移前先把 `Enemy.prefab`、`Bullet 0.prefab`、`Bullet 1.prefab` 等需要运行时实例化的资源迁入 `BundleAssets`，并确定不含空格和旧数组语义的 location 命名。
- 使用 `GameModule.GameObjectPool.CreateGameObjectPoolAsync` 预热，使用 `SpawnAsync` / `Recycle` / `DestroyPool` 管理生命周期。

## 音频方案

使用 `IAudioModule` 替代旧 `AudioManager`。

原则：

- BGM、战斗音效、UI 音效统一走 DGame 音频模块。
- 高频 SFX 需要做池化或限流，避免大量敌人受击时抢占过多通道。
- 升级暂停时的滤波或表现效果，后续作为独立表现设计处理，不依赖旧场景上 `Camera.main` 的特殊组件查找。

## 数据化策略

### 第一阶段

先在新架构中复刻原 Demo 的关键参数：

- 4 个角色
- 2 个敌人阶段
- 5 个 Item/Weapon/Gear
- 原经验曲线
- 300 秒胜利条件
- 三选一升级

### 第二阶段

迁移到 Luban：

- 角色成长
- 波次表
- Item/Weapon/Gear 升级曲线
- 敌人参数
- 文本与音频引用

## 第一阶段交付目标

第一阶段要达成“完整可玩一局”的最小闭环：

- [x] 可从 DGame 主流程进入幸存者玩法入口代码路径。
- [x] `Demo2` 场景具备 `SurvivorBattleRoot/Player` 基础契约，并已复用 `CompleteScene` 的 Ground、Player、Main Camera、Virtual Camera、Global Light 2D、Global Volume。
- [x] 旧 `Spawner`、四个旧 `Reposition` 组件已从 `Demo2` 正式运行链移除，并通过 Play Mode 验证不再报旧脚本空引用。
- [ ] 玩家可移动，并能被摄像机稳定跟随。（代码和场景已接入，Play Mode 待验证）
- [ ] 敌人可生成、追踪、受伤、死亡、回收。
- [ ] 武器可自动攻击。
- [ ] 经验、升级、暂停、恢复可正常运行。
- [ ] 300 秒胜利 / 死亡失败可进入结果界面。
- [ ] 退出与重开通过新 Flow 实现，不依赖 `LoadScene(0)`。
- [x] 旧场景 Canvas、EventSystem、GameManager、PoolManager、AudioManager 未进入当前 `Demo2` 正式运行链。

## 推荐实施顺序

### 阶段 1：流程与场景骨架

- 已完成：幸存者玩法入口流程、Additive 加载、Session 骨架、HUD/Result 骨架。
- 已完成：用 `CompleteScene.unity` 裁剪重建 `Demo2.unity`，复用 Ground、Player、Main Camera、Virtual Camera、Global Light 2D、Global Volume；旧场景 Canvas、EventSystem、GameManager、PoolManager、AudioManager 未复制。
- 已完成：迁入第一批 Enemy/Bullet Prefab 到 `BundleAssets/Prefabs/Survivor`，建立稳定资源文件名。
- 已完成：移除 `SurvivorBattleRoot/Player/Spawner` 上的旧 `Spawner` 和四个 Tilemap 上的旧 `Reposition`，保留刷怪点和地图表现资源。
- 已完成：移除 `SurvivorEnemy`、`SurvivorBullet0`、`SurvivorBullet1` 上的旧运行脚本，移除 `Hand Left/Right` 上的旧 `Hand`，保留表现和碰撞资源。
- 待完成：确认进入、退出、重开在 Play Mode 下可稳定通过。
- 待完成：将第一批 Prefab 接入对象池预热与 Spawn/Recycle，并替换旧脚本依赖。

### 阶段 2：局内基础循环

- 玩家移动
- 摄像机跟随
- 地图循环
- 敌人生成与追踪
- 伤害与死亡回收

### 阶段 3：战斗成长闭环

- 自动攻击
- 经验与升级
- 三选一升级
- 胜负判定
- 结果界面

### 阶段 4：框架化完善

- UI 拆窗完成
- 音频模块替换完成
- 对象池预热与回收完善
- 存档/解锁迁移
- 配置表迁移
- 性能与 GC 优化

## 已知风险

- 原 `CompleteScene` 含重复 `EventSystem`、Camera/AudioListener 与整套场景 UI，不能直接作为最终运行场景照搬。
- 原逻辑高度依赖数组顺序、层级索引和单例调用，直接搬脚本会把技术债引入新架构。
- 原代码位于 `Assembly-CSharp`，不能直接作为当前 DGame 热更新结构的最终业务边界。
- 资源接入 YooAsset 前需要先处理文件重名和地址冲突风险。
- 原 Demo 某些对象回收与重置依赖隐式生命周期，迁移到新对象池后必须显式梳理 Spawn/Recycle 语义。
- `Demo2` 场景骨架已补齐，旧 `Spawner` / `Reposition` 运行组件已移除并通过 AIBridge Play Mode 止血验证；后续风险转移到新 `Survivor*` 系统尚未接管刷怪、地图循环、敌人、子弹和成长逻辑。
- 旧 `Demo2*` 三消命名残留会增加认知成本；后续应按是否影响编译、资源地址和维护判断清理范围，不要在迁移中继续扩展旧语义。

## 当前决策

- 迁移策略采用“分阶段完整重构”。
- 首期目标是先做 DGame 版可完整游玩一局的最小闭环。
- 首期保留原玩法参数与体验，不要求同时完成所有配置表迁移。
- Undead Survivor 直接使用 Demo2 制作：复用 Demo2 入口与场景资源地址，不新增 Demo3 或并行 Demo 槽位。
- 内部领域代码使用 `Survivor*` 命名，对外仍以 `Demo2` 作为承载入口和场景 location。
- 旧 Demo2 三消事实不再作为当前项目依据；若工作区仍存在相关文件，只按历史残留处理。
- 后续实现以该方案为准推进。
