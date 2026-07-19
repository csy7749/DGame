## Context

当前 DGame 工程已经包含完整 SweetSugar 模板资源、脚本和场景，但 `GameUnity/Assets/Scenes/Demo.unity` 仍是只有 `Main Camera` 的空场景，无法承载第一关玩法。SweetSugar 的可运行链路集中在 `GameUnity/Assets/SweetSugar/Scenes/game.unity`，关卡数据通过 `Resources/Levels/Level_1.asset` 和 `Resources/Levels/Targets/TargetLevel1.asset` 成对加载。

DGame 规范要求业务资源最终走 `GameModule.ResourceModule`，热更业务落在 `GameUnity/Assets/Scripts/HotFix/GameLogic`。但 SweetSugar 当前是完整第三方小游戏模板，内部大量使用 `Resources.Load`、`PlayerPrefs`、`SceneManager.LoadScene`、协程和场景内引用。首轮目标是验证一关三消闭环，因此本设计选择先收口运行链，不做全量分层迁移。

## Goals / Non-Goals

**Goals:**

- 让 `GameUnity/Assets/Scenes/Demo.unity` 成为可直接测试的 SweetSugar 第一关 Demo 场景。
- 启动 Demo 后固定进入 `Level_1`，不受历史 `PlayerPrefs` 关卡号影响。
- 保留 SweetSugar 现有棋盘、棋子、目标、动画、特效和结算能力。
- 在 Demo 流程中绕开地图、多关卡推进、广告、内购、社交/网络、排行榜、GDPR、每日奖励、商店和转盘。
- 明确 Unity 编辑器验收标准，验证第一关可玩闭环。

**Non-Goals:**

- 不把 SweetSugar 玩法代码迁移到 DGame HotFix `GameLogic`。
- 不把 SweetSugar `Resources` 资源迁移到 `BundleAssets` 或 YooAsset。
- 不重写 SweetSugar UI 为 DGame `UIWindow`/`UIWidget`。
- 不做多关卡、地图、正式存档、商业化、社交或网络能力。
- 不清理 SweetSugar 全部无关脚本和资源。

## Decisions

### 1. 以 SweetSugar `game.unity` 作为 Demo 场景基线

`Demo.unity` 目前缺少玩法对象和场景引用，从空场景手动重建会非常容易漏掉 UI、相机、事件系统、目标面板、字段容器和脚本引用。实施时应先用 SweetSugar `game.unity` 验证原链路，再把它另存或复制为 `Assets/Scenes/Demo.unity` 后做收口。

备选方案是从空 `Demo.unity` 手工搭建运行对象。该方案对场景内引用理解要求高，且错误会集中表现为空引用或 UI 缺失，不适合作为首轮跑通路径。

### 2. 用薄 Demo 启动路径固定 `Level_1`

SweetSugar 当前通过 `OpenLevel` 等 `PlayerPrefs` 决定关卡入口。Demo 应增加或配置一个很薄的启动路径，显式设置第一关并触发现有加载流程。这样可以避免修改 `LevelManager` 的通用地图、测试和重开逻辑，也避免历史本地存档影响测试结果。

备选方案是直接大改 `LevelManager.LoadLevel()`。该方案会波及地图、测试关卡和结算流程，不符合首轮最小改动目标。

### 3. 通过场景与按钮链路隔离地图和外部系统

Demo 不应删除大批 SweetSugar 目录，而应从 Demo 场景入口、按钮事件和宏配置上绕开地图、广告、IAP、社交、网络、GDPR、商店和奖励入口。这样可以降低编译引用和序列化引用被误伤的风险。

备选方案是先清理无关目录。SweetSugar 没有独立 asmdef 分层，贸然删除会造成广泛编译错误或场景引用丢失。

### 4. 本阶段保留 SweetSugar `Resources` 加载链路

DGame 长期方向是通过 `GameModule.ResourceModule` 和 YooAsset 管理热更资源，但 SweetSugar 当前玩法和数据强依赖 `Resources`。首轮 Demo 允许保留 `Resources`，只验证玩法闭环；资源迁移作为后续独立变更评估。

备选方案是同步迁移到 `BundleAssets`。该方案会把资源地址、加载生命周期、预制体引用和关卡 ScriptableObject 读取一起扩大，风险超过单关 Demo 目标。

## Risks / Trade-offs

- [SweetSugar 原始 `game.unity` 在当前仓库无法跑通] -> 先修原链路，再复制为 Demo；不要在空 `Demo.unity` 上叠加不确定性。
- [Demo 仍可能被历史 `PlayerPrefs` 污染] -> 启动路径必须显式设置 `OpenLevel = 1`，并清理或覆盖影响第一关入口的测试关卡键。
- [结算按钮仍跳地图或下一关] -> 验收时必须覆盖胜利/失败弹窗，并检查 `SceneManager.LoadScene("main")`、`MapSwitcher` 和下一关按钮绑定。
- [广告/内购/网络入口通过 UI 事件被触发] -> Demo 场景中隐藏或禁用对应按钮和弹窗入口，不启用相关宏。
- [保留 `Resources` 与 DGame 资源规范不一致] -> 明确标记为首轮例外，后续迁移到 YooAsset 另开变更处理。
- [SweetSugar 授权边界不清] -> 内部 Demo 可继续验证；公开、外发或二次分发前必须确认授权。

## Migration Plan

1. 在 Unity 中原样打开 `Assets/SweetSugar/Scenes/game.unity`，验证 `Level_1` 可运行。
2. 将 `game.unity` 另存或复制为 `Assets/Scenes/Demo.unity`，保留原场景作为参照。
3. 添加或配置 Demo 启动路径，固定进入 `Level_1`。
4. 从 Demo 场景流程中绕开地图和结算后的地图/下一关跳转。
5. 从 Demo 场景中隐藏或禁用广告、IAP、社交/网络、GDPR、奖励、商店、转盘入口。
6. 把 `Assets/Scenes/Demo.unity` 加入 Build Settings。
7. 在 Unity 2021.3.30f1c1 中完成第一关手动验收。

回滚策略：保留 `Assets/SweetSugar/Scenes/game.unity` 原始基线；若 Demo 改造失败，恢复 `Assets/Scenes/Demo.unity` 到改造前状态，并重新从原始 SweetSugar 场景复制。

## Open Questions

- Demo 启动脚本最终放在 SweetSugar 目录内，还是放在 `GameUnity/Assets/Scenes` 附近的 Demo 专用目录中，需要实施时根据 Unity 编译引用和目录约定确认。
- 是否需要保留原 SweetSugar 三个场景在 Build Settings 中启用，取决于是否还要同时保留原模板对照测试入口。
