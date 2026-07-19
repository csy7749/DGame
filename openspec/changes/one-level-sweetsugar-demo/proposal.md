## Why

DGame 已经接入 SweetSugar 资源并有一份单关三消 Demo 清单，但 `Assets/Scenes/Demo.unity` 目前仍是只有相机的空场景，无法进入 `Level_1`。本变更定义一个最小范围方案：先把 SweetSugar 现有 `game.unity` 运行链收口为稳定的一关 Demo，不同时承担 DGame 资源、UI 或 HotFix 迁移。

## What Changes

- 创建一个直接进入 SweetSugar `Level_1` 的 Demo 场景流程。
- 首轮实现以 SweetSugar 现有 `game.unity` 的场景结构、脚本、`Resources` 资源、动画、特效和关卡数据为基线。
- 新增或配置一个很薄的 Demo 启动路径，固定进入第 1 关，并避免历史 `PlayerPrefs` 关卡号影响结果。
- 在 Demo 流程中绕开地图入口、返回地图、下一关推进、广告、内购、社交/网络、排行榜、GDPR、每日奖励、商店和转盘路径。
- 实施时把 `Assets/Scenes/Demo.unity` 加入 Unity Build Settings，作为明确的 Demo 测试入口。
- 本阶段保留 SweetSugar 现有 `Resources` 加载链路；暂不迁移到 `BundleAssets` 或 `GameModule.ResourceModule`。

## Capabilities

### New Capabilities
- `sweetsugar-one-level-demo`: 在 DGame 项目内启动并验证一个可玩的 SweetSugar `Level_1` Demo 场景，且不依赖地图、商业化、社交或多关卡流程。

### Modified Capabilities
- 无。

## Impact

- 影响场景：`GameUnity/Assets/Scenes/Demo.unity`、作为参照基线的 `GameUnity/Assets/SweetSugar/Scenes/game.unity`，以及 Unity Build Settings。
- 影响 SweetSugar 代码区域：`Scripts/Core/LevelManager.cs`、`Scripts/GUI/AnimationEventManager.cs`、场景内 UI/Button 事件绑定，以及可能新增的固定关卡 Demo 启动脚本。
- 影响资源：继续使用现有 `GameUnity/Assets/SweetSugar/Resources/Levels/Level_1.asset`、`Resources/Levels/Targets/TargetLevel1.asset` 和 SweetSugar 玩法资源。
- 本阶段不影响：DGame Runtime 模块、HotFix `GameLogic`、Luban 配置、YooAsset `BundleAssets`、DGame UI 框架、广告 SDK、IAP、社交/网络服务和正式多关卡推进。
