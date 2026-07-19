# DGame 软件工程级框架设计文档

## 1. 文档目的

本文档描述 DGame 客户端框架的工程设计，包括设计目标、分层架构、关键机制、模块边界、扩展方式和质量约束。它面向后续参考、迁移、二次实现和架构评审。

本文档不替代源码级接口索引。具体接口、路径和模块清单见 `docs/architecture-reference.md`。

## 2. 背景与约束

DGame 是基于 TEngine 二次封装的 Unity 客户端框架，运行在 Unity 2021.3.30f1c1，组合使用 HybridCLR、YooAsset、UniTask、Luban 等基础设施。

核心工程约束：

- 业务代码需要支持热更新。
- 资源需要支持 YooAsset 管理、版本更新、补丁下载和缓存清理。
- 配置数据由 Luban 生成，运行时按二进制配置加载。
- UI、红点、文本、多语言、存档、数据中心等能力需要可复用。
- AOT 启动层不能直接依赖 HotFix 业务实现。
- 框架需要在 Unity 生命周期、热更生命周期、资源生命周期之间保持明确边界。

## 3. 设计目标

### 3.1 功能目标

框架需要提供以下基础能力：

- 可扩展的运行时模块系统。
- AOT 启动流程编排。
- HybridCLR 热更程序集加载与入口桥接。
- YooAsset 资源初始化、加载、下载、清理和低内存处理。
- Luban 配置加载与业务封装入口。
- UI 窗口栈、组件、控制器、生命周期和事件托管。
- 事件接口定义、生成、分发和 UI 生命周期自动解绑。
- 多语言、文本表、文本绑定和语言切换刷新。
- 红点树、红点值聚合和 UI 绑定。
- 数据中心、本地存档和网络客户端接入点。

### 3.2 非功能目标

框架重点优化以下质量属性：

- **可维护性**：分层清晰，模块边界稳定，业务逻辑集中在 HotFix 层。
- **可扩展性**：新增模块、UI、配置、事件时有明确落位和入口。
- **可热更性**：主业务逻辑位于热更程序集，AOT 层只承担启动和桥接。
- **可测试性**：非 Unity 表现逻辑尽量沉到普通类、接口、配置管理器和数据模块中。
- **资源安全**：资源统一由 `IResourceModule` 管理，避免绕过 YooAsset 生命周期。
- **运行时稳定性**：模块销毁、UI 事件解绑、低内存释放和单例清理有统一入口。

### 3.3 非目标

当前框架不试图解决以下问题：

- 不提供完整玩法业务架构。
- 不把战斗表现、UI、音频和资源加载塞进 `GameBattle`。
- 不将配置生成产物作为人工维护对象。
- 不在 Runtime/AOT 层直接引用热更业务类型。
- 不为每个业务功能创建新的全局入口或平行模块系统。

## 4. 架构总览

DGame 客户端采用四层单向依赖架构：

```text
┌────────────────────────────────────────────┐
│ HotFix 业务层                               │
│ GameLogic / GameProto / GameBattle          │
│ UI、业务模块、配置消费、战斗纯逻辑           │
└───────────────────────┬────────────────────┘
                        │ 仅向下依赖
┌───────────────────────▼────────────────────┐
│ Main AOT 应用层                              │
│ DGame.AOT / GameEntry / Procedure           │
│ 启动编排、资源更新、热更程序集加载           │
└───────────────────────┬────────────────────┘
                        │
┌───────────────────────▼────────────────────┐
│ DGame Runtime 核心层                         │
│ DGame.Runtime / RootModule / ModuleSystem   │
│ 模块系统、资源、事件、池、FSM、音频、场景     │
└───────────────────────┬────────────────────┘
                        │
┌───────────────────────▼────────────────────┐
│ 基础设施层                                  │
│ Unity / YooAsset / HybridCLR / UniTask      │
│ Luban / DOTween / 第三方库                  │
└────────────────────────────────────────────┘
```

依赖规则：

- 下层不感知上层。
- Runtime 提供稳定能力，不引用 HotFix。
- AOT 负责进入 HotFix 前的流程，不编译期引用 HotFix 业务实现。
- HotFix 业务通过 `GameModule` 聚合访问 Runtime 和业务模块。
- `GameProto` 承载配置生成代码，`GameLogic` 承载业务封装，`GameBattle` 承载纯战斗逻辑。

## 5. 程序集设计

| 程序集 | 层级 | 设计职责 | 依赖原则 |
| --- | --- | --- | --- |
| `DGame.Runtime` | Runtime 核心层 | 运行时模块系统和通用基础能力 | 可依赖 Unity 和第三方库，不依赖 HotFix。 |
| `DGame.AOT` | Main AOT 应用层 | 启动流程、资源更新、热更加载 | 可依赖 Runtime，不直接依赖 HotFix 业务类型。 |
| `GameLogic` | HotFix 业务层 | UI、业务模块、数据中心、文本、红点 | 可依赖 Runtime、GameProto、GameBattle。 |
| `GameProto` | HotFix 配置/协议层 | Luban 配置代码、配置加载入口 | 尽量保持薄，只承载配置和协议相关能力。 |
| `GameBattle` | HotFix 战斗层 | 战斗纯逻辑 | 可依赖 GameProto，不依赖 GameLogic 和表现层。 |
| `DGame.Editor` | Editor 工具层 | 开发期工具 | 只在编辑器内生效，不能污染运行时依赖链。 |

程序集设计的核心目的是控制依赖方向。热更能力不是简单把代码放进 HotFix 目录，而是要求主工程和业务之间只通过稳定入口、反射、配置、事件或公开接口协作。

## 6. 启动与热更设计

### 6.1 启动职责划分

启动链路分为三个阶段：

1. **Runtime 根驱动阶段**
   - `RootModule` 初始化日志、Json、字符串工具、帧率、时间缩放和内存策略。
   - `RootModule.Update()` 驱动 `ModuleSystem.Update()`。

2. **AOT Procedure 阶段**
   - `GameEntry.Awake()` 初始化核心模块并启动 Procedure。
   - Procedure 负责资源包初始化、版本检查、下载、预加载、热更 DLL 加载。

3. **HotFix 业务启动阶段**
   - `LoadAssemblyProcedure` 反射调用 `GameStart.Entrance(object[] objects)`。
   - `GameStart` 初始化热更侧事件、多语言和业务入口 UI。

### 6.2 Procedure 设计

Procedure 是启动过程的状态机，每个节点负责一个启动阶段：

```text
Launch
  -> Splash
  -> InitPackage
  -> InitResource
  -> CreateDownloader
  -> DownloadFile
  -> DownloadOver
  -> ClearCache
  -> Preload
  -> LoadAssembly
  -> StartGame
```

设计理由：

- 启动阶段天然是线性流程，用状态机表达更清晰。
- 每个状态只处理一个启动关注点，便于定位失败。
- 资源更新、下载、热更加载都发生在进入 HotFix 之前，避免业务入口承担底层启动职责。

扩展规则：

- 新增启动阶段应新增 Procedure，而不是把逻辑塞进 `GameStart`。
- 资源包初始化、下载、缓存清理、DLL 加载属于 AOT Procedure。
- 进入 HotFix 后的业务初始化属于 `GameStart` 或 HotFix 业务模块。

### 6.3 热更桥接设计

热更桥接点是 `LoadAssemblyProcedure -> GameStart.Entrance`。

桥接方式：

- AOT 层通过 YooAsset 加载 DLL bytes。
- 使用 `Assembly.Load` 加载热更程序集。
- 查找主业务程序集中的 `GameStart` 类型。
- 反射调用静态入口 `Entrance(object[] objects)`。
- 将已加载热更程序集列表作为参数传入。

设计收益：

- AOT 不需要编译期引用 HotFix 类型。
- HotFix 可以整体替换或更新。
- 入口约定简单，排查路径明确。

约束：

- `GameStart` 类型名和 `Entrance` 方法签名是稳定协议。
- 不应在 AOT 层新增对 `GameLogic` 具体类型的直接依赖。
- 反射只适合入口桥接，不应在高频业务路径滥用。

## 7. 模块系统设计

### 7.1 Runtime 模块系统

`ModuleSystem` 是 Runtime 层模块容器。

核心机制：

- 通过接口类型获取模块。
- 实现类命名由接口名去掉 `I` 推导。
- 模块实例按优先级注册。
- 实现 `IUpdateModule` 的模块被加入更新列表。
- `RootModule.Update()` 统一驱动更新模块。
- 销毁时按逆序执行 `OnDestroy()`。

设计收益：

- 模块创建规则统一。
- Runtime 模块可按接口解耦。
- 更新顺序可由优先级控制。
- 销毁链路统一，避免散落清理。

设计约束：

- 模块接口和实现命名必须匹配。
- 获取模块时必须传接口类型。
- Runtime 模块应保持通用能力，不包含具体业务语义。

### 7.2 HotFix 模块门面

`GameModule` 是 HotFix 层模块聚合门面。

设计职责：

- 对业务层隐藏 `ModuleSystem.GetModule<T>()` 细节。
- 聚合 Runtime 模块和 HotFix 单例模块。
- 给业务代码提供统一访问习惯。
- 在热更销毁时清空缓存引用。

设计收益：

- 业务代码不需要到处知道模块来自 Runtime 还是 HotFix。
- 便于统一约束模块访问方式。
- 后续替换底层模块时可收敛修改点。

约束：

- 业务层优先通过 `GameModule` 访问模块。
- 不新增平行的全局模块入口。
- 不在业务对象中长期保存模块字段，避免热更销毁和重启后引用陈旧。

## 8. 资源系统设计

### 8.1 设计职责

资源系统以 `IResourceModule` 为统一入口，负责：

- YooAsset 初始化和包初始化。
- 默认包和资源运行参数管理。
- Asset、Prefab、场景和配置资源加载。
- 资源版本请求和 Manifest 更新。
- 补丁下载器创建。
- Bundle 缓存清理。
- 低内存时释放未使用资源。

### 8.2 生命周期规则

资源生命周期按资源类型区分：

| 类型 | 推荐入口 | 释放规则 |
| --- | --- | --- |
| UI Sprite | `SetSprite` / `SetSubSprite` | 通常由扩展和 UI 生命周期处理。 |
| GameObject Prefab | `LoadGameObjectAsync` / 对象池 | 实例销毁或回收时处理。 |
| 普通 Asset | `LoadAssetAsync<T>` / `LoadAsset<T>` | 使用方在生命周期结束时调用 `UnloadAsset`。 |
| 高频 GameObject | `IGameObjectPoolModule` | 使用 `SpawnAsync` / `Recycle`。 |
| 配置 TextAsset | `ConfigSystem` 内部加载 | 由配置系统和资源模块协作。 |

### 8.3 资源边界

运行时资源必须进入 `BundleAssets` 并由 YooAsset 管理。业务代码不应直接使用 `Resources.Load()` 或绕过 `IResourceModule`。

设计理由：

- 统一资源寻址。
- 统一引用计数和卸载策略。
- 支持补丁下载和版本更新。
- 支持低内存释放。

## 9. 配置系统设计

### 9.1 配置生成链路

配置源头在 `GameConfig/`：

```text
Excel / schema / template
  -> Luban 生成
  -> GameProto/LubanConfig C# 代码
  -> BundleAssets/Configs/Binary 二进制数据
  -> ConfigSystem 运行时加载
  -> GameLogic/ConfigMgr 业务封装
```

### 9.2 运行时加载

`ConfigSystem` 是 `GameProto` 内的配置入口。

加载机制：

- 首次访问 `ConfigSystem.Instance.Tables` 时懒加载。
- `Tables` 构造时注入 `LoadByteBuf`。
- `LoadByteBuf` 通过 `IResourceModule` 加载二进制配置。
- Luban 表类提供 `GetOrDefault`、`TryGetValue`、`DataMap`、`DataList` 等访问方式。

### 9.3 业务封装

`GameLogic/ConfigMgr` 是业务配置访问层。

设计原则：

- UI 和业务模块不直接散落访问 `TbXXX`。
- 高频使用或有业务语义的表应封装为 `ConfigMgr`。
- 配置字段变更时优先收敛到 `ConfigMgr`。

这样做的实际收益是降低配置结构调整对 UI 和业务模块的影响。

## 10. UI 系统设计

### 10.1 核心结构

UI 系统由三层组成：

```text
UIModule
  管理 UIRoot、Canvas、Camera、窗口栈、控制器、Esc 关闭

UIWindow
  表示完整窗口或弹窗，管理生命周期、层级、模态、全屏、安全区

UIWidget / BaseChildPage
  表示局部组件、复用项、子页面
```

### 10.2 生命周期设计

`UIWindow` 生命周期固定为：

```text
ScriptGenerator()
-> BindMemberProperty()
-> RegisterEvent()
-> OnCreate()
-> OnRefresh()
-> OnDestroy()
```

设计意图：

- 绑定、初始化、事件注册、创建逻辑和刷新逻辑职责分离。
- UI 事件订阅集中在 `RegisterEvent()`。
- 销毁时通过基类统一回收事件和组件资源。

### 10.3 UI 事件托管

`UIBase` 内部使用 `GameEventDriver` 托管 UI 事件订阅。

设计目的：

- UI 注册事件时不需要手动维护所有监听列表。
- UI 销毁时自动反注册。
- 降低重复注册、忘记解绑和闭包错误带来的风险。

### 10.4 扩展规则

- 完整页面继承 `UIWindow`。
- 局部组件继承 `UIWidget`。
- Tab 子页面使用 `SwitchPageMgr` / `BaseChildPage`。
- 大数据列表使用循环列表封装。
- UI 图片加载使用资源扩展方法，不手动加载 Sprite。
- UI 显示和关闭通过 `GameModule.UIModule`。

## 11. 事件系统设计

### 11.1 事件分层

事件系统分为四层：

```text
业务定义层
  IEvent 接口 + EventInterfaceAttribute + EEventGroup

生成层
  IXxx_Event.g.cs / IXxx_Gen.g.cs / GameEventHelper.g.cs

使用层
  GameEvent.Get<T>().Method()
  AddUIEvent(IXxx_Event.Method, callback)

底层分发层
  GameEvent / EventDispatcher / GameEventDriver
```

### 11.2 设计原则

- 接口是事件定义源。
- 生成产物提供事件 ID 和代理实现。
- 发送侧优先使用接口语义。
- 监听侧使用生成的事件 ID。
- UI 监听通过 `AddUIEvent` 托管生命周期。

### 11.3 边界规则

- 不手写事件 int 常量。
- 不手写 `*_Event.cs`。
- 不把所有事件塞进一个巨大接口。
- 不在 UI 中直接 `GameEvent.AddEventListener` 后忘记释放。
- 事件接口按能力域拆分，例如登录 UI、通用 UI、多语言、战斗。

## 12. 红点系统设计

红点系统采用树结构。

核心模型：

- `RedDotModule` 维护所有节点。
- `RedDotNode` 保存路径、值、父子关系、聚合策略和监听。
- `RedDotPathDefine` 由配置生成，提供稳定路径常量。
- `RedDotItem` 是 UI 展示组件。

设计收益：

- 父节点可由子节点聚合得到状态。
- UI 只监听自己关心的节点。
- 红点路径集中生成，减少硬编码。
- 表现与业务计算分离。

扩展规则：

- 新红点节点先进入红点树配置。
- 业务模块只更新节点值。
- UI 通过 `CreateRedDot` 创建显示项并监听变化。

## 13. 多语言与文本设计

多语言和文本系统分为两条链：

```text
语言链：
LocalizationModule
  -> DGameLocalizationHelper
  -> ILocalization.OnLanguageChanged
  -> UI 监听刷新

文本链：
TbTextConfig
  -> TextConfigMgr
  -> G.R(...)
  -> UITextIDBinder / UI 代码
```

设计原则：

- 当前语言由 `ILocalizationModule` 管理。
- 文本内容由配置表驱动。
- UI 文本优先通过 `UITextIDBinder` 或 `G.R(...)` 获取。
- 语言切换通过事件通知 UI 刷新。

## 14. 数据中心与本地存档设计

### 14.1 DataCenter

`DataCenter` 提供 HotFix 业务数据组织方式。

核心抽象：

- `DataCenterSys`：当前玩家、角色 ID、数据清理和更新入口。
- `DataCenterModule<T>`：业务数据模块基类，提供登录、登出、更新和地图切换生命周期。

设计意图：

- 将玩家状态和业务状态从 UI 中剥离。
- 给业务数据提供统一生命周期。
- 为后续网络消息、角色切换和数据清理提供稳定位置。

### 14.2 ClientSaveData

客户端存档系统提供本地持久化入口。

核心抽象：

- `BaseClientSaveData`
- `ClientSaveDataMgr`
- `ClientSaveDataAttribute`
- `SystemSaveData`

设计原则：

- 存档类显式声明 key 和存储模式。
- 读取入口集中在 `ClientSaveDataMgr`。
- 批量保存由管理器统一执行。

## 15. 战斗层设计

`GameBattle` 的设计定位是纯逻辑热更程序集。

允许内容：

- 战斗规则。
- 战斗状态。
- 战斗命令。
- 战斗计算。
- 与配置相关的纯数据读取模型。

不允许内容：

- UI 窗口。
- Unity 场景对象操作。
- 资源加载。
- 动画、特效、音频播放。
- 对 `GameLogic` 的依赖。

设计理由：

- 纯逻辑更容易测试。
- 跨平台一致性更容易控制。
- 战斗可以独立于表现层复用或模拟。

## 16. Editor 工具设计

Editor 工具层服务于开发期，不属于运行时依赖链。

典型职责：

- UI 绑定代码生成。
- 红点配置编辑和路径代码生成。
- Luban 导表辅助。
- HybridCLR 生成与构建辅助。
- 发布、工具栏、项目设置辅助。

约束：

- Editor 可以读取或引用 HotFix 类型做开发期工具。
- Editor 依赖不能泄漏到 Runtime/AOT 发布路径。
- 生成结果应通过工具产生，不手工修改生成产物。

## 17. 扩展设计规范

### 17.1 新增 Runtime 模块

适用场景：

- 能力通用。
- 不包含具体业务语义。
- 多个业务模块需要复用。
- 生命周期需要由 `RootModule` / `ModuleSystem` 管理。

规则：

- 放在 `GameUnity/Assets/DGame/Runtime/Module/`。
- 定义 `IXxxModule` 接口。
- 实现 `XxxModule : Module, IXxxModule`。
- 若需要每帧更新，实现 `IUpdateModule`。
- 不依赖 HotFix 类型。

### 17.2 新增 HotFix 业务模块

适用场景：

- 具体业务功能。
- UI、数据、配置封装、红点、文本、活动、系统玩法。

规则：

- 放在 `GameUnity/Assets/Scripts/HotFix/GameLogic/Module/` 或对应业务目录。
- 优先复用 `Singleton<T>` 或现有业务模块模式。
- 访问 Runtime 模块走 `GameModule`。
- 跨模块通知优先走事件接口。

### 17.3 新增 UI

规则：

- 完整窗口继承 `UIWindow`。
- 复用组件继承 `UIWidget`。
- 事件注册放 `RegisterEvent()`。
- 通过 `GameModule.UIModule.ShowWindow<T>()` 打开。
- 资源加载走 UI 资源加载链路，不绕过 `IResourceModule`。

### 17.4 新增配置表

规则：

- 源头改 `GameConfig/Datas` 和 schema。
- 执行 Luban 生成脚本。
- 不手改 `GameProto/LubanConfig`。
- 业务层新增或扩展 `ConfigMgr`。
- UI 和业务模块不直接散落访问 `TbXXX`。

### 17.5 新增事件

规则：

- 在 `IEvent/` 下定义接口。
- 添加 `[EventInterface(EEventGroup.Xxx)]`。
- 重新编译生成事件 ID 和代理。
- 发送走 `GameEvent.Get<T>()`。
- UI 监听走 `AddUIEvent`。

## 18. 关键质量属性分析

### 18.1 可维护性

维护性来自分层和入口收敛：

- Runtime 通用能力集中在 `DGame.Runtime`。
- 启动流程集中在 `DGame.AOT/Procedure`。
- 业务集中在 `GameLogic`。
- 配置生成集中在 `GameProto`。
- 业务访问模块集中在 `GameModule`。

主要风险：

- 业务绕过 `GameModule` 直接到处取模块。
- UI 直接访问配置表。
- AOT 层引用 HotFix 类型。
- 生成产物被人工修改。

### 18.2 可扩展性

扩展性来自固定落位和固定协议：

- 新模块遵循 `IXxxModule -> XxxModule`。
- 新 UI 遵循 `UIWindow/UIWidget`。
- 新配置遵循 `GameConfig -> Luban -> GameProto -> ConfigMgr`。
- 新事件遵循接口定义和生成链。

主要风险：

- 为单个业务功能新增全局系统。
- 绕过现有 UI、资源、配置、事件体系。
- 把表现逻辑混入 `GameBattle`。

### 18.3 热更安全

热更安全来自 AOT 和 HotFix 的反射边界：

- AOT 不编译期依赖 HotFix。
- HotFix 主入口协议固定。
- HybridCLR 元数据补充在 AOT Procedure 阶段处理。

主要风险：

- 新增泛型组合后未重新走 HybridCLR 生成链。
- iOS AOT 场景只在 Editor 验证。
- 在高频业务路径滥用反射。

### 18.4 资源安全

资源安全来自统一资源模块：

- YooAsset 生命周期统一。
- GameObject 实例化入口统一。
- 普通 Asset 释放责任明确。
- 低内存处理有集中回调。

主要风险：

- 使用 `Resources.Load`。
- 手动 `LoadAsset<GameObject>` 后 `Instantiate`。
- 普通 Asset 加载后不释放。
- 高频对象不用对象池。

### 18.5 UI 生命周期安全

UI 生命周期安全来自基类和事件托管：

- `UIWindow` 生命周期固定。
- `AddUIEvent` 自动托管 UI 事件。
- 窗口显示、隐藏、关闭由 `UIModule` 管理。

主要风险：

- UI 中手动注册事件但不释放。
- 绕过 `UIModule` 自己实例化窗口。
- 在列表复用项里残留旧状态。

## 19. 已知工程风险

| 风险 | 影响 | 控制方式 |
| --- | --- | --- |
| AOT 与 HotFix 泛型补充遗漏 | iOS 或 IL2CPP 平台运行失败 | 新增热更泛型组合后重新运行 HybridCLR 生成链。 |
| 配置生成产物被手改 | 下次导表覆盖，行为不可追踪 | 只改 `GameConfig` 源头。 |
| 业务直接访问 `TbXXX` | 配置结构变更影响面扩大 | 高频配置访问封装进 `ConfigMgr`。 |
| 资源绕过 `IResourceModule` | 引用计数、热更、卸载失控 | 运行时统一走资源模块。 |
| AOT 直接依赖 HotFix | 热更边界破坏 | 通过反射入口、接口、配置或事件协作。 |
| UI 事件未解绑 | 重复响应、内存泄漏、空引用 | UI 监听统一使用 `AddUIEvent`。 |
| `GameBattle` 引入表现层 | 战斗不可测试、不可复用 | 战斗层保持纯逻辑。 |

## 20. 架构决策摘要

| 决策 | 结论 | 原因 |
| --- | --- | --- |
| 分层方式 | Runtime / AOT / HotFix / 基础设施 | 控制热更边界和职责边界。 |
| 启动流程 | Procedure 状态机 | 启动阶段清晰，可定位，可扩展。 |
| 热更入口 | 反射调用 `GameStart.Entrance` | 避免 AOT 编译期依赖 HotFix。 |
| 模块访问 | HotFix 走 `GameModule` | 收敛入口，降低耦合。 |
| 资源访问 | 统一走 `IResourceModule` | 支持 YooAsset、热更、卸载和下载。 |
| 配置访问 | `GameProto` 生成，`GameLogic/ConfigMgr` 封装 | 分离生成结构和业务语义。 |
| UI 架构 | `UIModule + UIWindow + UIWidget` | 统一窗口栈、生命周期和事件回收。 |
| 事件定义 | 接口 + Source Generator | 避免手写 ID，提高语义清晰度。 |
| 红点模型 | 树结构 + 生成路径 | 支持聚合和 UI 监听。 |
| 战斗层 | `GameBattle` 纯逻辑 | 保持可测试、可复用和依赖干净。 |

## 21. 推荐落位矩阵

| 需求 | 推荐落位 | 主要依赖 |
| --- | --- | --- |
| 通用底层能力 | `DGame/Runtime/Module` | Runtime 模块系统 |
| 启动前流程 | `DGame.AOT/Procedure` | Procedure / ResourceModule |
| 热更业务逻辑 | `Scripts/HotFix/GameLogic` | GameModule / Event / ConfigMgr |
| 配置生成代码 | `Scripts/HotFix/GameProto` | Luban / ConfigSystem |
| 战斗计算 | `Scripts/HotFix/GameBattle` | GameProto |
| UI 页面 | `GameLogic/UI` | UIModule / UIWindow |
| UI 复用组件 | `GameLogic/Module/UIModule` 或业务 UI 目录 | UIWidget |
| 红点配置与展示 | `GameLogic/Module/RedDotModule` | RedDotModule |
| 文本和多语言 | `GameLogic/Module/TextModule` / `LocalizationModule` | TextConfigMgr / LocalizationModule |
| 本地存档 | `GameLogic/DataCenter/ClientSaveData` | ClientSaveDataMgr |
| 编辑器工具 | `DGame/Editor` 或业务 `Editor` | UnityEditor |

## 22. 结论

DGame 的核心设计是用 Runtime 层承载稳定通用能力，用 AOT 层承载启动和热更桥接，用 HotFix 层承载可变业务逻辑。框架价值主要体现在入口收敛和生命周期收敛：模块通过 `ModuleSystem` 和 `GameModule` 收敛，资源通过 `IResourceModule` 收敛，UI 通过 `UIModule` 收敛，配置通过 `ConfigSystem` 和 `ConfigMgr` 收敛，事件通过接口和生成链收敛。

后续参考或迁移这个框架时，优先复用这些边界和入口。不要只复制单个工具类或模块实现，否则容易丢失它依赖的生命周期、资源、热更和生成链上下文。
