## ADDED Requirements

### Requirement: Demo2 从 DGame 主启动链进入
系统 SHALL 从 `GameStart.unity` 完成 DGame 初始化，并通过 `MainWindow` 使用 `GameModule.SceneModule` 进入地址为 `Demo2` 的场景。

#### Scenario: 从主界面进入
- **WHEN** 用户点击 `MainWindow` 的开始按钮
- **THEN** 系统异步加载并激活 `BundleAssets/Scenes/Demo2.unity`

### Requirement: Demo2 运行时不依赖 SweetSugar
Demo2 的 HotFix 源码、场景、Prefab、UI 和资源依赖闭包 SHALL 不包含 `GameUnity/Assets/SweetSugar` 下的脚本或资产。

#### Scenario: 源码依赖检查
- **WHEN** 扫描 `GameBattle/Demo2` 和 `GameLogic/Demo2` 的源码
- **THEN** 不存在 `using SweetSugar`、SweetSugar 类型名引用或对 SweetSugar 程序集的引用

#### Scenario: Unity 资产依赖检查
- **WHEN** 使用 Unity AssetDatabase 查询 Demo2 场景及所有 Demo2 Prefab 的递归依赖
- **THEN** 返回路径中不存在以 `Assets/SweetSugar/` 开头的资产

### Requirement: 三消规则位于 GameBattle
系统 SHALL 在 `HotFix/GameBattle/Demo2` 中实现与 Unity 表现解耦的棋盘规则，并通过注入随机源获得可复现结果。

#### Scenario: 合法交换产生匹配
- **WHEN** 两个正交相邻棋子交换后形成横向或纵向三个及以上连续同类棋子
- **THEN** 规则层返回匹配集合、消除结果、下落补充结果和新的不可变棋盘状态

#### Scenario: 无效交换回滚
- **WHEN** 相邻交换未形成任何匹配
- **THEN** 规则层判定移动无效且最终棋盘与交换前一致

#### Scenario: 非相邻交换被拒绝
- **WHEN** 输入的两个格子不正交相邻或超出棋盘边界
- **THEN** 规则层返回明确的无效移动结果

### Requirement: 第一关棋盘有效
系统 SHALL 使用固定第一关定义创建无初始匹配且至少存在一个合法移动的棋盘。

#### Scenario: 初始化第一关
- **WHEN** Demo2 创建新 Session
- **THEN** 初始棋盘不存在即时三连且存在至少一个可产生匹配的交换

#### Scenario: 无解棋盘
- **WHEN** 消除补充后的棋盘不存在合法移动
- **THEN** 规则层执行确定性重排或明确报告无解状态，不得静默继续不可玩的棋盘

### Requirement: 第一关目标与胜负闭环
系统 SHALL 为 Demo2 第一关定义固定步数、目标棋子类型和目标数量，并由规则状态判定胜利或失败。

#### Scenario: 达成目标
- **WHEN** 目标计数在剩余步数归零前达到要求
- **THEN** Session 进入胜利状态并停止接受棋盘输入

#### Scenario: 步数耗尽
- **WHEN** 剩余步数归零且目标未达成
- **THEN** Session 进入失败状态并停止接受棋盘输入

### Requirement: 基础特殊棋子
系统 SHALL 支持至少一种由四连匹配生成的特殊棋子，并由规则层定义其触发效果。

#### Scenario: 四连生成特殊棋子
- **WHEN** 一次有效移动形成连续四个同类棋子
- **THEN** 消除结算后在规则指定位置生成基础特殊棋子

#### Scenario: 特殊棋子触发
- **WHEN** 特殊棋子参与有效匹配
- **THEN** 规则层返回其附加消除范围并更新目标计数

### Requirement: GameLogic 只负责流程与表现
GameLogic SHALL 根据 GameBattle 返回的结果驱动输入锁定、交换动画、消除、下落、补充、HUD 和结算，不得重复实现匹配或胜负算法。

#### Scenario: 结算期间输入锁定
- **WHEN** 一次移动正在播放交换、消除或下落动画
- **THEN** 新的棋盘输入被拒绝直到表现完成并同步到最新规则状态

### Requirement: DGame 模块管理资源与 UI
新增 Demo2 业务 SHALL 使用 `GameModule.SceneModule`、`ResourceModule`、`UIModule`、`AudioModule` 或 `GameObjectPool` 管理对应能力，不得使用 `Resources.Load` 或散落的 `ModuleSystem.GetModule<T>()`。

#### Scenario: 加载棋子表现
- **WHEN** Demo2 需要创建棋子或 UI Prefab
- **THEN** 资源从 `BundleAssets` 通过 DGame 资源或 UI API 加载并具有明确释放生命周期

### Requirement: HUD、结算、重开与返回
系统 SHALL 使用 DGame UIWindow 显示步数、目标、胜负结果，并提供重开当前关和返回主界面的操作。

#### Scenario: 重开第一关
- **WHEN** 用户在结算窗口点击重开
- **THEN** 当前 Session 和棋盘表现被销毁并以相同第一关定义创建新局

#### Scenario: 返回主界面
- **WHEN** 用户点击返回
- **THEN** 系统关闭 Demo2 UI、卸载 Demo2 场景、释放持有资源并重新显示 `MainWindow`

### Requirement: Demo2 可重复进入且无残留
系统 SHALL 支持多次进入和退出 Demo2，不产生重复 Camera、输入控制器、Session、UI 或棋子实例。

#### Scenario: 连续进入两次
- **WHEN** 用户完成一次进入和退出后再次进入 Demo2
- **THEN** 新局从干净状态开始且不存在上一局的运行对象或事件监听

### Requirement: 失败明确暴露
场景、资源、规则定义或棋盘生成失败时，系统 SHALL 抛出明确异常或记录 `DLogger.Error` 并停止当前进入流程，不得提供模拟成功或静默降级。

#### Scenario: 棋子 Prefab 缺失
- **WHEN** Demo2 必需的棋子资源地址无效
- **THEN** 系统记录具体资源地址和失败原因，并终止当前局初始化

### Requirement: SweetSugar 基线保持不变
实现 Demo2 SHALL 不修改 SweetSugar 原目录和现有 `Demo.unity` 的运行行为。

#### Scenario: Demo 回归
- **WHEN** 用户直接运行现有 `Demo.unity`
- **THEN** 原有 SweetSugar 第一关仍按改动前行为运行
