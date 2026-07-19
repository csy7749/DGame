# SweetSugar 一关三消 Demo 清单

## 1. 目标

本清单只服务当前阶段目标：

- 在 DGame 项目里先跑通一个可玩的单关三消 Demo。
- 直接进入关卡，不做地图、不做多关卡流转、不做广告、不做内购、不做社交。
- 尽量复用 SweetSugar 已有三消规则、资源、动画和特效。
- 当前阶段允许继续依赖 SweetSugar 现有 `Resources` 加载链路，先不要同时做 DGame 资源系统改造。

## 2. 已确认的关键事实

### 2.1 关卡加载链路

- 关卡入口在 [LevelManager.cs](D:/UGit/Git/Framework/DGame/GameUnity/Assets/SweetSugar/Scripts/Core/LevelManager.cs:1618)
- 加载方法在 [LoadingManager.cs](D:/UGit/Git/Framework/DGame/GameUnity/Assets/SweetSugar/Scripts/Level/LoadingManager.cs:10)
- 关卡 Scriptable 容器在 [LevelContainer.cs](D:/UGit/Git/Framework/DGame/GameUnity/Assets/SweetSugar/Scripts/Level/LevelContainer.cs:9)
- 关卡数据结构在 [LevelData.cs](D:/UGit/Git/Framework/DGame/GameUnity/Assets/SweetSugar/Scripts/Level/LevelData.cs:16)
- Scriptable 读取入口在 [ScriptableLevelManager.cs](D:/UGit/Git/Framework/DGame/GameUnity/Assets/SweetSugar/Scripts/System/ScriptableLevelManager.cs:46)

当前关卡是通过 `Resources/Levels/Level_x.asset` 和 `Resources/Levels/Targets/TargetLevelx.asset` 成对加载，不依赖地图系统也能独立开局。

### 2.2 单关资源现状

- 已存在关卡数据：`GameUnity/Assets/SweetSugar/Resources/Levels/Level_1.asset`
- 已存在目标数据：`GameUnity/Assets/SweetSugar/Resources/Levels/Targets/TargetLevel1.asset`
- 已存在运行场景：`GameUnity/Assets/SweetSugar/Scenes/game.unity`
- 已存在地图/外壳场景：`GameUnity/Assets/SweetSugar/Scenes/main.unity`、`gameStatic.unity`

### 2.3 当前模板不是纯资源包

`SweetSugar` 目录内大约有：

- 247 个 C# 脚本
- 43 个 Prefab
- 3 个主场景
- 大量 `Resources`、动画、音频、材质、贴图

它本质上是完整小游戏模板，不是只有三消核心。

## 3. 单关 Demo 的最小范围

### 3.1 本阶段必须保留

这些内容直接参与一关三消的可玩闭环：

| 类别 | 目录/对象 | 说明 |
| --- | --- | --- |
| 核心玩法 | `GameUnity/Assets/SweetSugar/Scripts/Core` | 主循环、输入、状态机、关卡运行 |
| 关卡数据 | `GameUnity/Assets/SweetSugar/Scripts/Level` | 关卡数据、棋盘、加载入口 |
| 棋盘格子 | `GameUnity/Assets/SweetSugar/Scripts/Blocks` | 方格、障碍、层级块 |
| 棋子系统 | `GameUnity/Assets/SweetSugar/Scripts/Items` | 普通糖果、特殊糖果、销毁逻辑 |
| 目标系统 | `GameUnity/Assets/SweetSugar/Scripts/TargetScripts` | 收集目标、目标计数、关卡目标判定 |
| 组合规则 | `GameUnity/Assets/SweetSugar/Scripts/System/Combiner` | 特殊糖果生成和组合逻辑 |
| 公共工具 | `GameUnity/Assets/SweetSugar/Scripts/System` | 当前核心逻辑依赖的公共脚本 |
| 玩法特效 | `GameUnity/Assets/SweetSugar/Scripts/Effects` | 爆炸、闪电、飞行效果 |
| 关卡资源 | `GameUnity/Assets/SweetSugar/Resources/Levels` | 单关数据和目标数据 |
| 棋盘/特效资源 | `GameUnity/Assets/SweetSugar/Prefabs`、`Animation`、`Audio`、`Materials`、`Textures_png` | 保证关卡能完整表现 |

### 3.2 本阶段可以保留但不主动改

这些内容可能被现有场景或 UI 轻度引用，但不是当前改造重点：

| 类别 | 目录 | 处理策略 |
| --- | --- | --- |
| 基础 UI 脚本 | `GameUnity/Assets/SweetSugar/Scripts/GUI` | 先只保留运行所需，不做 DGame UI 改造 |
| 本地化 | `GameUnity/Assets/SweetSugar/Scripts/Localization` | 先不清理，避免误伤引用 |
| 方向适配 | `GameUnity/Assets/SweetSugar/Scripts/System/Orientation` | 若场景依赖则保留 |
| Scriptable 奖励 | `GameUnity/Assets/SweetSugar/Scriptable` | 当前不扩展，但可暂存 |

### 3.3 本阶段明确不做

这些内容不属于“一关可玩”目标，应先隔离，不纳入首轮接入：

| 类别 | 目录/脚本 | 原因 |
| --- | --- | --- |
| 地图系统 | `MapScripts`、`main.unity`、`gameStatic.unity` | 单关 Demo 不需要地图入口和关卡推进 |
| 广告 | `AdsEvents`、`UnityAdsController.cs`、`AppodealIntegration.cs` | 与玩法无关，且引入外部 SDK 依赖 |
| 内购 | `UnityInAppsIntegration.cs`、`GUI/Purchasing` | 首版不需要付费链路 |
| 社交/网络 | `Integrations`、`PlayFab`、`Leadboard` | 首版不需要账号、好友、排行 |
| GDPR | `GDPR.cs`、`GDPRPopupManager.cs` | 首版内部 Demo 不需要隐私弹窗链路 |
| 每日奖励/转盘/商店 | `DailyReward`、`BonusSpin`、`BoostShop`、`LifeShop` | 与单关核心闭环无关 |

## 4. 必须面对的技术差异

SweetSugar 当前实现与 DGame 规范存在明显差异，后续实现要按“先跑通、后收口”的顺序处理。

### 4.1 资源加载差异

SweetSugar 大量使用：

- `Resources.Load`
- `Resources.LoadAll`
- 直接实例化 `Resources` 下资源

DGame 规范要求业务资源最终走 `GameModule.ResourceModule`。但当前如果一上来就把 SweetSugar 全量迁到 `BundleAssets`，改造面会过大。  
本阶段结论：

- 首版单关 Demo 允许继续使用 SweetSugar 现有 `Resources`。
- 等单关玩法稳定后，再评估迁移到 `BundleAssets`。

### 4.2 状态存储差异

SweetSugar 大量使用：

- `PlayerPrefs`
- `OpenLevel`
- `Score{level}`
- `ReachedLevel`

本阶段结论：

- 单关 Demo 可容忍少量 `PlayerPrefs` 继续存在。
- 但应避免再扩写地图进度、商店货币、广告奖励相关状态。

### 4.3 流程控制差异

SweetSugar 大量使用：

- `SceneManager.LoadScene`
- `StartCoroutine`
- `DontDestroyOnLoad`

DGame 更偏向：

- 主场景固定启动
- 业务在 HotFix 层组织
- 资源/界面通过模块驱动

本阶段结论：

- 单关 Demo 不先做全量协程改 `UniTask`。
- 但不要继续扩建新的 SweetSugar 多场景流转。

## 5. 一关 Demo 的推荐落地方式

### 5.1 推荐起步路径

先不要拆 SweetSugar 玩法代码到 DGame 热更层。首轮更稳的做法是：

1. 先用 SweetSugar 自带 `game.unity` 验证 `Level_1` 能在当前仓库正常跑起来。
2. 把地图、广告、内购、社交入口从这条运行链里摘掉。
3. 把单关启动入口改成固定打开第 1 关。
4. 跑通后，再决定哪些逻辑要逐步搬到 `GameLogic`。

### 5.2 当前阶段的成功标准

满足以下条件即可认为“一关 Demo 清单阶段完成”：

- 可以直接进入 `Level_1`
- 可以交换、消除、生成特殊糖果
- 可以正确结算胜负
- 不依赖地图场景
- 不依赖广告、内购、PlayFab、排行

## 6. 后续实施时的优先级

真正开始改代码时，建议按下面顺序做：

1. 固定单关启动入口
2. 切断地图入口和返回地图逻辑
3. 切断广告、内购、网络、GDPR 入口
4. 验证 `Level_1` 完整可玩
5. 再评估是否迁移到 DGame 的 HotFix UI / ResourceModule

## 7. 当前阶段不建议做的事情

- 不要同时做玩法接入和资源热更迁移
- 不要同时做多关卡系统
- 不要同时重写 SweetSugar 的全部 UI
- 不要先清理全部无关脚本再验证单关能跑
- 不要在还没跑通 `Level_1` 前就做 DGame 风格的大重构

## 8. 风险记录

### 8.1 授权风险

SweetSugar 源文件头部带有禁止再分发声明。  
如果后续仓库需要公开、外发或二次分发，必须单独确认资源和源码授权边界。

### 8.2 改造风险

SweetSugar 目前没有 `asmdef`，脚本默认混在一起编译。  
这意味着先做“运行链收口”比先做“程序集分层”更稳，否则很容易一次打散大量引用。

### 8.3 文档与实现边界

本清单是接入范围清单，不是最终技术方案。  
后续真正开始实现时，应以实际场景引用和脚本调用链为准。
