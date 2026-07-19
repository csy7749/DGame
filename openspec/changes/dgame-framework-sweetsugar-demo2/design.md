## Context

用户已将 `Demo2.unity` 放入 `GameUnity/Assets/BundleAssets/Scenes/`。该目录已经被 YooAsset Scenes Collector 收集，使用 `AddressByFileName`，因此场景地址为 `Demo2`。当前场景只有基础 Camera，没有 SweetSugar 序列化引用。

SweetSugar 包含约 548 个脚本，原核心逻辑高度耦合 `Resources.Load`、`PlayerPrefs`、Coroutine、LeanTween、地图、广告和大量场景对象。直接复制整套代码到 HotFix 会把第三方架构和无关系统一起带入，并违反本变更的独立性目标。

DGame 已有三个适合的边界：

- `GameBattle`：热更战斗程序集，适合纯棋盘规则和状态机。
- `GameLogic`：热更业务程序集，已引用 `GameBattle`，适合场景流程、输入、表现和 UI。
- `BundleAssets`：YooAsset 热更资源根目录，适合 Demo2 场景、Prefab、Sprite 和 UI。

## Goals / Non-Goals

**Goals:**

- Demo2 的所有运行时代码都位于 HotFix，并由 HybridCLR 加载。
- Demo2 场景及其完整资源依赖闭包不引用 `Assets/SweetSugar`。
- 实现一个固定第一关的可玩三消闭环。
- 通过 DGame 主启动、场景、资源、UI、事件和音频模块管理 Demo2。
- 棋盘规则可在不加载 Unity 场景的情况下进行单元测试。

**Non-Goals:**

- 不复制 SweetSugar 全部脚本或实现其所有方块、目标、道具、动画和商业化功能。
- 不追求与 SweetSugar 第一关逐像素、逐帧一致。
- 不实现地图、多关卡、存档推进、广告、内购、社交或网络功能。
- 不修改或删除 SweetSugar 原目录和现有 `Demo.unity`。
- 首版配置不接入 Luban；固定第一关使用 HotFix 中的不可变定义，后续配置化另立变更。

## Decisions

### 1. 重建最小规则，不复制第三方运行时

SweetSugar 仅作为行为参考。允许人工参考匹配规则、关卡目标和美术风格，但新代码使用 Demo2 命名空间、DGame 生命周期和新的数据模型。禁止复制广告、地图、商店、存档、网络和兼容分支。

需要实现的最小规则：

- 固定尺寸棋盘和有限棋子类型。
- 仅允许正交相邻交换。
- 交换后必须产生匹配，否则回滚。
- 横向或纵向连续三个及以上构成匹配。
- 匹配合并后统一消除，按列下落并补充。
- 初始棋盘不得包含即时匹配，且必须存在可行交换。
- 四连生成一种基础特殊棋子，并验证其触发效果。
- 固定步数和固定目标数量，目标达成胜利，步数耗尽且目标未达成失败。

### 2. 规则层放 GameBattle

`GameBattle/Demo2` 只包含纯 C# 规则和不可变值对象，不引用 `MonoBehaviour`、`GameObject`、DGame UI 或资源模块。建议职责：

- `Demo2BoardState`：只读棋盘快照。
- `Demo2LevelDefinition`：固定第一关定义。
- `Demo2Move`、`Demo2Match`、`Demo2ResolveResult`：规则输入输出。
- `Demo2BoardEngine`：交换校验、匹配、消除、下落、补充和胜负计算。
- `IDemo2Random`：注入式随机源，使测试可复现。

规则方法返回新状态，不修改调用方传入的棋盘或全局状态。

### 3. 表现与流程放 GameLogic

`GameLogic/Demo2` 负责：

- 使用 `GameModule.SceneModule` Additive 加载、激活和卸载 `Demo2`。
- 创建并持有一局 `Demo2Session`，调用 GameBattle 规则。
- 将屏幕输入转换为格子坐标和 `Demo2Move`。
- 使用 `GameModule.ResourceModule` 或 `GameObjectPool` 创建棋子表现。
- 按规则结果播放交换、消除、下落和补充动画。
- 通过 DGame 接口事件通知 HUD 刷新步数、目标和结算状态。

表现层不得自己判定匹配或胜负，规则结果是唯一事实来源。

### 4. 使用 DGame UI 承载 HUD 和结算

新增 `Demo2HUDWindow` 和 `Demo2ResultWindow`：

- HUD 显示剩余步数、目标进度和返回按钮。
- Result 显示胜利或失败，并提供重开与返回。
- UI 内监听使用 `AddUIEvent`，按钮绑定使用生成 partial 方法。
- UI Prefab 放 `BundleAssets/UI`，图片放 `BundleAssets/UIRaw`。

场景中不复制 SweetSugar Canvas。棋盘表现可以使用场景根节点或独立 Prefab，DGame UI 始终由持久化 `UIRoot` 管理。

### 5. Demo2 资源完全独立

Demo2 的 Scene、Prefab、Sprite、Material、AudioClip 和 ScriptableObject（如有）必须位于 `BundleAssets` 的 Demo2 专用目录或项目公共目录。可以复制 SweetSugar 美术文件作为新资产，但复制后必须生成新的 `.meta` GUID，并修正所有依赖，不能继续引用原 SweetSugar GUID。

验收使用 Unity 依赖查询和文本 GUID 扫描，确认 `Demo2.unity`、Demo2 Prefab 和 UI Prefab 的依赖闭包不包含 `Assets/SweetSugar`。

### 6. 固定第一关定义先用代码

首版使用 `Demo2LevelDefinition.CreateLevelOne()` 返回不可变定义，避免为了单关 Demo 扩展 Luban 表结构。尺寸、步数、目标和棋子集合必须使用命名常量，不散落魔法数字。

这不是静默 fallback。定义构造失败、棋盘无可行步或资源缺失时必须抛出明确异常或记录 `DLogger.Error`，停止当前局。

### 7. 场景采用 Additive 生命周期

从 `MainWindow` 加载 `Demo2`：

1. 校验 `ContainsAsset("Demo2")`。
2. `LoadSceneAsync("Demo2", LoadSceneMode.Additive)`。
3. `ActivateScene("Demo2")`。
4. 创建 Session 并显示 HUD。

退出时关闭 Demo2 UI，销毁或回收棋子实例，卸载场景，释放资源持有关系并恢复 `MainWindow`。

## Risks / Trade-offs

- [重建三消规则的工作量高于适配现成代码] -> 严格限制为单关、有限棋子和一种特殊棋子，按可测试规则层逐步实现。
- [热更 MonoBehaviour 挂载或序列化受 HybridCLR 约束] -> 场景只保留基础锚点，运行时由 GameLogic 创建控制器和表现对象；需要挂载的 HotFix 类型必须在 Editor 和真机验证。
- [复制美术后仍间接依赖 SweetSugar] -> 使用 AssetDatabase 依赖闭包检查，不只搜索文件文本。
- [随机棋盘出现死局] -> 注入随机源，生成后检测可行交换；无解作为明确错误处理或执行规则层确定性重排。
- [动画与规则状态不同步] -> 输入在 resolve 期间锁定，表现按 `Demo2ResolveResult` 顺序执行，完成后才接受下一次输入。
- [GameBattle 与 GameLogic 职责混淆] -> GameBattle 禁止引用 UI、资源和场景；GameLogic 禁止实现匹配算法。

## Migration Plan

1. 保持空的 `BundleAssets/Scenes/Demo2.unity`，建立场景锚点和 Camera。
2. 在 GameBattle 实现并测试纯三消规则。
3. 在 GameLogic 实现 Session、表现、输入和场景生命周期。
4. 创建 Demo2 独立棋子、棋盘和 UI 资源。
5. 接入 MainWindow 并完成第一关闭环。
6. 扫描并消除所有 SweetSugar 代码和资源依赖。
7. 完成 Editor、HotFix DLL 和 Play Mode 验收。

回滚时删除 Demo2 的 HotFix 目录和 BundleAssets 资源，并恢复空 Demo2 场景；SweetSugar 与 Demo 基线不受影响。

## Open Questions

- Demo2 首版美术是复制 SweetSugar 素材后断开 GUID，还是直接使用项目公共图形，需要实施时根据授权和现有公共资源决定。
- 棋盘表现使用 UGUI 还是世界空间 SpriteRenderer，需要根据第一关当前视觉比例和输入坐标转换成本选择；规则层不受该选择影响。
