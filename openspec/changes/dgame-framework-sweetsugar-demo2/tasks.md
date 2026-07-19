## 1. 基线与依赖边界

- [ ] 1.1 从 `GameStart.unity` 验证 DGame、YooAsset、HotFix DLL 和 `MainWindow` 启动链可运行。
- [ ] 1.2 确认 `BundleAssets/Scenes/Demo2.unity` 的 YooAsset 地址为 `Demo2`，无需新增场景 Collector。
- [ ] 1.3 记录 SweetSugar 第一关的最小行为基线：棋盘尺寸、棋子类型、步数、目标、普通匹配和一种特殊棋子。
- [ ] 1.4 建立 Demo2 禁止依赖清单：`Assets/SweetSugar`、`Resources.Load`、地图、广告、IAP、社交、网络和多关卡推进。

## 2. GameBattle 规则层

- [ ] 2.1 在 `GameBattle/Demo2` 创建不可变的坐标、棋子、移动、匹配、关卡定义、棋盘状态和结算结果类型。
- [ ] 2.2 定义 `IDemo2Random` 并实现可注入、可复现的随机序列。
- [ ] 2.3 实现相邻与边界校验、交换、横纵匹配检测和无效交换回滚。
- [ ] 2.4 实现统一消除、按列下落、空位补充和连锁结算。
- [ ] 2.5 实现初始棋盘生成，保证无初始匹配且至少存在一个合法移动。
- [ ] 2.6 实现死局检测与确定性重排；无法恢复时明确返回错误。
- [ ] 2.7 实现固定第一关定义、步数消耗、目标计数和胜负判定。
- [ ] 2.8 实现四连生成基础特殊棋子及其触发消除效果。
- [ ] 2.9 为规则层添加自动化测试，覆盖有效交换、无效交换、非相邻交换、连锁、目标、胜负、特殊棋子和死局。

## 3. Demo2 独立资源

- [ ] 3.1 在 `BundleAssets/Prefabs/Demo2` 创建棋盘根节点和棋子表现 Prefab，不引用 SweetSugar GUID。
- [ ] 3.2 在 `BundleAssets/UIRaw` 或项目公共目录准备 Demo2 Sprite、材质和必要音频；复制素材时生成新 GUID。
- [ ] 3.3 在空的 `BundleAssets/Scenes/Demo2.unity` 中配置 Camera、棋盘锚点和必要环境对象，不挂载 SweetSugar 脚本。
- [ ] 3.4 使用 Unity AssetDatabase 递归检查 Demo2 Scene、Prefab 和 UI 依赖，清除所有 `Assets/SweetSugar/` 路径。

## 4. GameLogic Session 与表现

- [ ] 4.1 在 `GameLogic/Demo2` 实现 `Demo2Session`，持有 GameBattle 状态并作为一局生命周期拥有者。
- [ ] 4.2 实现 Demo2 场景流程，使用 `SceneModule` Additive 加载、激活和卸载地址 `Demo2`。
- [ ] 4.3 实现棋盘表现控制器，通过 `ResourceModule` 或 `GameObjectPool` 创建、移动、更新和回收棋子实例。
- [ ] 4.4 实现输入控制器，将指针或触摸转换为格子交换请求，并在 resolve 期间锁定输入。
- [ ] 4.5 按 GameBattle 的结算结果顺序播放交换、消除、下落、补充和特殊棋子效果。
- [ ] 4.6 实现 Session 重开和销毁，清理事件、资源持有、对象池实例和场景对象。

## 5. DGame UI 与事件

- [ ] 5.1 在 `GameLogic/IEvent` 定义 Demo2 HUD、目标进度和结算所需的接口事件并生成事件代码。
- [ ] 5.2 创建 `Demo2HUDWindow` Prefab 和 HotFix 代码，显示剩余步数、目标进度和返回按钮。
- [ ] 5.3 创建 `Demo2ResultWindow` Prefab 和 HotFix 代码，显示胜负并提供重开和返回按钮。
- [ ] 5.4 UI 内监听使用 `AddUIEvent`，异步资源操作传入窗口销毁取消令牌。
- [ ] 5.5 如需音效，通过 `GameModule.AudioModule` 播放 BundleAssets 音频，不直接持有或加载 SweetSugar AudioClip。

## 6. 主入口接入

- [ ] 6.1 在 `MainWindow.OnClickStartGameBtn()` 调用 Demo2 流程入口。
- [ ] 6.2 进入成功后关闭 MainWindow；进入失败时保留 MainWindow 并使用 `DLogger.Error` 暴露根因。
- [ ] 6.3 返回时关闭 Demo2 UI、卸载场景、释放资源并重新显示 MainWindow。
- [ ] 6.4 防止重复点击导致并发加载或创建多个 Session。

## 7. 静态与编译验证

- [ ] 7.1 编译 GameBattle 和 GameLogic，修复所有新增 error，并确认 HotFix DLL 构建链可生成。
- [ ] 7.2 扫描 Demo2 HotFix 源码，确认不存在 `SweetSugar`、`Resources.Load` 或 `ModuleSystem.GetModule`。
- [ ] 7.3 扫描 Demo2 场景和 Prefab 的脚本 GUID，确认全部指向 DGame、GameBattle、GameLogic 或 Unity/允许的第三方基础库。
- [ ] 7.4 使用 AssetDatabase 生成 Demo2 递归依赖报告，确认没有 `Assets/SweetSugar` 路径。
- [ ] 7.5 确认新增函数、文件、参数、嵌套和复杂度满足仓库硬限制。

## 8. Play Mode 验收

- [ ] 8.1 从 `GameStart.unity` 进入 Demo2，确认场景、棋盘、HUD 和输入正常。
- [ ] 8.2 验证有效交换、无效回滚、消除、下落、补充和至少一次连锁。
- [ ] 8.3 验证四连特殊棋子生成与触发效果。
- [ ] 8.4 验证目标进度、步数、胜利和失败结算。
- [ ] 8.5 验证重开当前关和返回主界面。
- [ ] 8.6 连续进入退出两次，确认无重复 Camera、Session、输入、UI、事件监听或棋子实例。
- [ ] 8.7 汇总源码扫描、脚本 GUID 和 AssetDatabase 递归依赖报告，证明 Demo2 编译与运行闭包不包含 `Assets/SweetSugar`。
- [ ] 8.8 回归现有 `Demo.unity`，确认本变更没有修改 SweetSugar 基线。
