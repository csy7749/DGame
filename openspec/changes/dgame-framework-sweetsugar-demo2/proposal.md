## Why

`Demo.unity` 已验证 SweetSugar 第一关的玩法方向，但 Demo2 的目标是验证 DGame 自有热更架构，而不是继续承载第三方运行时代码。`GameUnity/Assets/BundleAssets/Scenes/Demo2.unity` 目前是空场景，适合从零建立一个只依赖 DGame、GameLogic 和 GameBattle 的单关三消 Demo。

## What Changes

- 在 `HotFix/GameBattle` 中实现 Demo2 的纯三消规则：棋盘状态、相邻交换、匹配检测、消除、下落补充、目标计数、步数和胜负判定。
- 在 `HotFix/GameLogic` 中实现 Demo2 的场景流程、输入控制、棋盘表现、DGame UI 和进入/退出生命周期。
- 从 `GameStart.unity` 的 `MainWindow` 通过 `GameModule.SceneModule` 加载 `BundleAssets/Scenes/Demo2.unity`。
- 在 `BundleAssets` 下创建 Demo2 自有的场景、Prefab、Sprite、材质和 UI 资源，并通过 DGame `ResourceModule`、`UIModule` 和 `AudioModule` 使用。
- 可以参考或人工迁移 SweetSugar 第一关的规则和视觉设计，但 Demo2 运行时、场景、Prefab、资源依赖和 HotFix 源码不得引用 `GameUnity/Assets/SweetSugar`。
- Demo2 提供第一关完整闭环：交换、消除、下落补充、目标计数、基础特殊棋子、胜负结算、重开和返回主界面。
- 保留现有 `Demo.unity` 与 SweetSugar 目录作为对照基线，不修改其运行行为。

## Capabilities

### New Capabilities

- `dgame-framework-sweetsugar-demo2`: 在 DGame HotFix 和 YooAsset 体系内独立实现一个不依赖 SweetSugar 运行时代码与资源的第一关三消 Demo2。

### Modified Capabilities

- 无。

## Impact

- 新增玩法规则：`GameUnity/Assets/Scripts/HotFix/GameBattle/Demo2/`。
- 新增业务和 UI：`GameUnity/Assets/Scripts/HotFix/GameLogic/Demo2/`、`GameLogic/UI/Demo2/`。
- 新增资源：`GameUnity/Assets/BundleAssets/Scenes/Demo2.unity`、`BundleAssets/Prefabs/Demo2/`、`BundleAssets/UI/Demo2/`、`BundleAssets/UIRaw/.../Demo2/`。
- 修改入口：`GameLogic/UI/Main/MainWindow.cs` 和必要的 Demo2 事件接口。
- 不修改 DGame Runtime、AOT Procedure、Luban 表结构或 SweetSugar 原目录。
