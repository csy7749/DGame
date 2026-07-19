# DGame 架构与模块参考

本文档基于当前仓库静态扫描整理，用于后续参考 DGame 项目结构、模块功能、接口入口和扩展边界。代码以 `GameUnity/Assets` 下的实际实现为准；涉及 Unity 场景、YooAsset 包配置、HybridCLR 生成和 Luban 导表的内容仍需要在 Unity 2021.3.30f1c1 与对应工具链中验证。

## 1. 总体架构

DGame 是基于 TEngine 二次封装的 Unity 游戏框架，当前客户端按四层理解最稳：

```text
HotFix 业务层
  GameLogic / GameProto / GameBattle
  负责 UI、业务模块、配置消费、战斗纯逻辑
        ↓
Main AOT 应用层
  DGame.AOT / GameEntry / Procedure
  负责启动流程、资源更新、热更程序集加载、进入 HotFix
        ↓
DGame Runtime 核心层
  DGame.Runtime / RootModule / ModuleSystem / Runtime Modules
  负责模块系统、生命周期驱动、资源、音频、场景、池、事件等基础能力
        ↓
基础设施层
  Unity / YooAsset / HybridCLR / UniTask / Luban / DOTween 等
```

关键边界：

- `DGame.Runtime` 不直接引用 `GameLogic`、`GameProto`、`GameBattle`。
- `DGame.AOT` 通过反射调用 HotFix 入口，不应编译期写死 HotFix 类型。
- `GameLogic` 可依赖 `GameProto` 和 `GameBattle`。
- `GameBattle` 应保持纯逻辑，不承载 UI、资源、场景、动画、音频等表现层逻辑。
- 配置生成产物不要手改，源头在 `GameConfig/`。

## 2. 程序集与目录

| 程序集 | 路径 | 是否热更 | 主要职责 |
| --- | --- | --- | --- |
| `DGame.Runtime` | `GameUnity/Assets/DGame/Runtime/` | 否 | 框架核心运行时，模块系统、事件、资源、音频、场景、FSM、对象池等。 |
| `DGame.Editor` | `GameUnity/Assets/DGame/Editor/` | 否 | 编辑器工具链，发布、HybridCLR、Luban、工具栏等开发期能力。 |
| `DGame.AOT` | `GameUnity/Assets/DGame.AOT/` | 否 | 主工程启动层，`GameEntry`、Procedure、资源更新、热更程序集加载。 |
| `GameLogic` | `GameUnity/Assets/Scripts/HotFix/GameLogic/` | 是 | 主业务热更程序集，UI、业务模块、配置封装、数据中心、红点、文本等。 |
| `GameProto` | `GameUnity/Assets/Scripts/HotFix/GameProto/` | 是 | Luban 配置代码、`ConfigSystem`、生成表和类型。 |
| `GameBattle` | `GameUnity/Assets/Scripts/HotFix/GameBattle/` | 是 | 战斗域热更程序集，当前主要是纯逻辑预留。 |

仓库根目录职责：

- `GameUnity/`：Unity 主工程。
- `GameConfig/`：Luban 配置源表、schema、模板和导表脚本。
- `GameRelease/`：发布输出。
- `Tools/`：仓库级辅助工具。

## 3. 启动链路

当前主启动场景位于：

- `GameUnity/Assets/Scenes/GameStart`

启动主线：

```text
RootModule.Awake/Update
  -> GameEntry.Awake
  -> ProcedureSettings.StartProcedure()
  -> Procedure 状态机
  -> LoadAssemblyProcedure
  -> 反射调用 GameStart.Entrance(object[] objects)
  -> GameStart.StartGame()
  -> GameModule.UIModule.ShowWindow<MainWindow>()
```

核心文件：

- `GameUnity/Assets/DGame/Runtime/Module/RootModule.cs`
- `GameUnity/Assets/DGame.AOT/GameEntry.cs`
- `GameUnity/Assets/DGame.AOT/Procedure/`
- `GameUnity/Assets/DGame.AOT/Procedure/LoadAssemblyProcedure.cs`
- `GameUnity/Assets/Scripts/HotFix/GameLogic/GameStart.cs`

Procedure 当前主要阶段：

1. `LaunchProcedure`
2. `SplashProcedure`
3. `InitPackageProcedure`
4. `InitResourceProcedure`
5. `CreateDownloaderProcedure`
6. `DownloadFileProcedure`
7. `DownloadOverProcedure`
8. `ClearCacheProcedure`
9. `PreloadProcedure`
10. `LoadAssemblyProcedure`
11. `StartGameProcedure`

`LoadAssemblyProcedure` 的职责较重：

- 根据 `Settings.UpdateSettings` 判断是否启用热更。
- 在非编辑器热更模式下加载 AOT 元数据。
- 通过 `IResourceModule` 加载热更 DLL 的 `TextAsset`。
- `Assembly.Load(textAsset.bytes)` 装载热更程序集。
- 找到主业务程序集里的 `GameStart`，反射调用 `Entrance`。

`GameStart.Entrance` 当前做这些事：

- 保存热更程序集列表。
- 初始化 `GameLogic.GameEventLauncher`。
- 注册销毁监听。
- 初始化多语言。
- 打开 `MainWindow`。

## 4. Runtime 模块系统

模块系统入口：

- `GameUnity/Assets/DGame/Runtime/Core/ModuleSystem/ModuleSystem.cs`

核心规则：

- 业务通常通过 `GameModule` 访问模块。
- `ModuleSystem.GetModule<T>()` 要求 `T` 是接口。
- 实现类型通过接口名去掉 `I` 推导，例如 `IResourceModule -> ResourceModule`。
- 模块实例按 `Priority` 排序；实现 `IUpdateModule` 的模块由 `RootModule.Update()` 统一驱动。
- `ModuleSystem.Destroy()` 逆序销毁模块，并清理内存池和缓存内存。

`RootModule` 是 Runtime 层 Unity 生命周期入口：

- 初始化 MemoryPool 严格检查、字符串/日志/Json helper。
- 设置帧率、时间缩放、后台运行、屏幕休眠。
- 订阅低内存回调。
- 每帧调用 `ModuleSystem.Update(GameTime.DeltaTime, GameTime.UnscaledDeltaTime)`。

## 5. HotFix 模块门面 `GameModule`

HotFix 业务层统一入口：

- `GameUnity/Assets/Scripts/HotFix/GameLogic/GameModule.cs`

常用模块访问：

| 入口 | 类型 | 主要职责 |
| --- | --- | --- |
| `GameModule.RootModule` | `RootModule` | Runtime 根节点。 |
| `GameModule.FsmModule` | `IFsmModule` | 有限状态机。 |
| `GameModule.SensitiveWordModule` | `ISensitiveWordModule` | 敏感词过滤。 |
| `GameModule.AnimModule` | `IAnimModule` | Playable 动画图。 |
| `GameModule.ResourceModule` | `IResourceModule` | YooAsset 资源、版本、下载、缓存。 |
| `GameModule.AudioModule` | `IAudioModule` | 音频播放、音量、音频资源池。 |
| `GameModule.SceneModule` | `ISceneModule` | 场景加载、卸载、激活。 |
| `GameModule.GameTimerModule` | `IGameTimerModule` | 游戏计时器。 |
| `GameModule.InputModule` | `DGame.IInputModule` | Runtime 旧输入/快捷键。 |
| `GameModule.Input` | `GameLogic.IInputModule` | HotFix 新输入模块，受 `ENABLE_INPUT_SYSTEM` 控制。 |
| `GameModule.LocalizationModule` | `ILocalizationModule` | 多语言。 |
| `GameModule.GameObjectPool` | `IGameObjectPoolModule` | GameObject 对象池。 |
| `GameModule.UIModule` | `UIModule` | UI 窗口、栈、层级、控制器。 |
| `GameModule.RedDotModule` | `RedDotModule` | 红点树与监听。 |

使用建议：

- HotFix 业务优先从 `GameModule` 获取模块。
- 不要在业务对象中长期缓存底层模块引用。
- 新增业务能力前先确认 `GameModule` 是否已有入口。

## 6. Runtime 核心模块接口

### 6.1 资源 `IResourceModule`

路径：

- `GameUnity/Assets/DGame/Runtime/Module/ResourceModule/IResourceModule.cs`

职责范围：

- YooAsset 初始化与包初始化。
- 默认包名、PlayMode、加密、WebGL 加载方式等资源运行参数。
- 资源加载、实例化和卸载。
- 资源存在性和下载状态查询。
- 热更版本请求、Manifest 更新、下载器创建。
- 缓存清理和低内存回调。

常用接口：

- `Initialize()`
- `InitPackage(string packageName, bool needInitMainFest = false)`
- `LoadAssetAsync<T>(string location, CancellationToken cancellationToken = default, string packageName = "")`
- `LoadAsset<T>(string location, string packageName = "")`
- `LoadGameObjectAsync(string location, Transform parent = null, CancellationToken cancellationToken = default, string packageName = "")`
- `UnloadAsset(object asset)`
- `UnloadUnusedAssets()`
- `ForceUnloadUnusedAssets(bool performGCCollect)`
- `ContainsAsset(string location, string packageName = "")`
- `CheckLocationValid(string location, string packageName = "")`
- `GetAssetInfo(string location, string packageName = "")`
- `GetAssetInfos(string resTag, string packageName = "")`
- `CreateResourceDownloader(string customPackageName = "")`
- `RequestPackageVersionAsync(...)`
- `UpdatePackageManifestAsync(...)`
- `ClearCacheFilesAsync(...)`

使用边界：

- 运行时资源统一走 `GameModule.ResourceModule`。
- UI 图片优先走 `SetSprite` / `SetSubSprite` 扩展。
- GameObject Prefab 优先走 `LoadGameObjectAsync`，不要先 `LoadAssetAsync<GameObject>` 再手动 `Instantiate`。
- 普通 Asset 加载后按生命周期主动 `UnloadAsset`。

### 6.2 音频 `IAudioModule`

路径：

- `GameUnity/Assets/DGame/Runtime/Module/AudioModule/IAudioModule.cs`

职责范围：

- 总音量、音乐、音效、UI 音效、人声分组控制。
- 音频开关。
- AudioMixer 与实例根节点。
- AudioClip 资源句柄池。
- 播放、停止、预加载和清理音频资源。

常用接口：

- `Initialize(AudioGroupConfig[] audioGroupConfigs, Transform instanceRoot = null, AudioMixer audioMixer = null)`
- `Play(AudioType audioType, string path, bool isLoop = false, float volume = 1.0f, bool isAsync = false, bool isInPool = false)`
- `Stop(AudioType audioType, bool fadeout)`
- `StopAll(bool fadeout)`
- `PreLoadPutInAudioPool(List<string> list)`
- `RemoveClipFromAudioPool(List<string> list)`
- `ClearSoundPool()`

### 6.3 场景 `ISceneModule`

路径：

- `GameUnity/Assets/DGame/Runtime/Module/SceneModule/ISceneModule.cs`

职责范围：

- 主场景名记录。
- 场景异步/同步加载。
- 场景激活、挂起恢复、卸载。
- 场景存在性查询。

常用接口：

- `LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, ...)`
- `LoadScene(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, ...)`
- `ActivateScene(string location)`
- `UnSuspend(string location)`
- `UnloadAsync(string location, Action<float> progressCallBack = null)`
- `Unload(string location, Action callBack = null, Action<float> progressCallBack = null)`
- `ContainsScene(string location)`

### 6.4 FSM `IFsmModule`

路径：

- `GameUnity/Assets/DGame/Runtime/Module/FsmModule/IFsmModule.cs`

职责范围：

- 创建、查询、销毁有限状态机。
- 支持命名状态机。
- 支持带 Animator 和 AnimationWrapper 的状态机。

常用接口：

- `CreateFsm<T>(T owner, params IFsmState<T>[] states)`
- `CreateFsm<T>(string name, T owner, params IFsmState<T>[] states)`
- `GetFsm<T>()`
- `GetFsm<T>(string name)`
- `ContainsFsm<T>()`
- `DestroyFsm<T>()`

### 6.5 动画 `IAnimModule`

路径：

- `GameUnity/Assets/DGame/Runtime/Module/AnimModule/IAnimModule.cs`

职责范围：

- 基于 Playable 的动画图创建和管理。
- 按名称查询和销毁 `IAnimPlayable`。

常用接口：

- `CreateAnimPlayable(Animator animator)`
- `CreateAnimPlayable(Animator animator, List<AnimationClip> animations)`
- `CreateAnimPlayable(Animator animator, List<AnimationWrapper> animations)`
- `GetAnimPlayable(string name)`
- `DestroyAnimPlayable(IAnimPlayable animPlayable)`

### 6.6 计时器 `IGameTimerModule`

路径：

- `GameUnity/Assets/DGame/Runtime/Module/GameTimer/IGameTimerModule.cs`

职责范围：

- 创建受时间缩放或不受时间缩放影响的计时器。
- 支持一次、循环、指定循环次数。
- 计时器暂停、恢复、重置、销毁。

常用接口：

- `CreateOnceGameTimer(float interval, TimerHandler handler, object[] args = null)`
- `CreateLoopGameTimer(float interval, TimerHandler handler, object[] args = null)`
- `CreateLoopCountGameTimer(float interval, int loopCount, TimerHandler handler, object[] args = null)`
- `CreateUnscaledOnceGameTimer(...)`
- `CreateUnscaledLoopGameTimer(...)`
- `Pause(GameTimer timer)`
- `Resume(GameTimer timer)`
- `DestroyGameTimer(GameTimer timer)`

### 6.7 GameObject 对象池 `IGameObjectPoolModule`

路径：

- `GameUnity/Assets/DGame/Runtime/Module/GameObjectPoolModule/IGameObjectPoolModule.cs`

职责范围：

- 按资源 location 创建 GameObject 池。
- 异步生成实例。
- 回收、移除、销毁池。
- 提供调试快照。

常用接口：

- `CreateGameObjectPoolAsync(string location, int initCapacity = 0, int maxCapacity = int.MaxValue, ...)`
- `SpawnAsync(string location, CancellationToken ct = default)`
- `SpawnAsync(string location, Transform parent, CancellationToken ct = default)`
- `Recycle(GameObject gameObject)`
- `Remove(GameObject gameObject)`
- `GetGameObjectPool(string location)`
- `DestroyPool(string location)`
- `DestroyAllPool(bool includeAll)`

### 6.8 通用对象池 `IObjectPoolModule`

路径：

- `GameUnity/Assets/DGame/Runtime/Module/ObjectPoolModule/IObjectPoolModule.cs`

职责范围：

- 面向 `BasePoolObject` 的通用对象池管理。
- 支持 single-spawn 和 multi-spawn 两类对象池。
- 支持容量、过期时间、自动释放间隔和优先级。
- 统一释放未使用对象。

常用接口族：

- `Contains<T>()`
- `GetObjectPool<T>()`
- `CreateSingleSpawnObjectPool<T>(...)`
- `CreateMultiSpawnObjectPool<T>(...)`
- `DestroyObjectPool<T>(...)`
- `ReleaseAllUnused()`

### 6.9 输入 `DGame.IInputModule`

路径：

- `GameUnity/Assets/DGame/Runtime/Module/InputModule/IInputModule.cs`

职责范围：

- 注册键盘、鼠标和快捷键输入命令。
- 移除输入命令。
- 支持按键重绑定和保存。

常用接口：

- `RegisterInputCommand(...)`
- `RemoveInputCommand(...)`
- `RebindInputCommand(KeyCode oldKeyCode, KeyCode newKeyCode)`
- `TryGetRebindKeyCode(...)`
- `SaveRebindKeyMap()`

## 7. HotFix 业务模块

### 7.1 UI 系统

核心目录：

- `GameUnity/Assets/Scripts/HotFix/GameLogic/Module/UIModule/`
- `GameUnity/Assets/Scripts/HotFix/GameLogic/UI/`

核心类型：

- `UIModule`：UI 总入口，管理 `UIRoot`、Canvas、Camera、窗口栈、显示/隐藏/关闭、控制器注册。
- `UIWindow`：完整窗口基类，负责完整页面/弹窗生命周期、层级、模态、全屏、安全区、关闭。
- `UIWidget`：局部复用组件基类。
- `UIBase`：窗口和组件共同基类，提供事件托管、组件查找、Widget 创建、列表辅助、红点创建。
- `IUIController`：UI 控制器注册接口，仅定义 `RegUIMessage()`。

`UIModule` 常用接口：

- `ShowWindow<T>(params object[] userDatas)`
- `ShowWindowAsync<T>(params object[] userDatas)`
- `ShowWindowAsyncAwait<T>(params object[] userDatas)`
- `GetWindow<T>()`
- `GetWindowAsyncAwait<T>()`
- `CloseWindow<T>()`
- `HideWindow(UIWindow window)`
- `CloseAllWindows()`
- `ContainsWindow<T>()`
- `GetTopWindow()`
- `GetAndCloseTopWindow(...)`
- `IsAnyLoading()`

`UIBase` 常用接口：

- `AddUIEvent(...)` 系列重载：UI 生命周期托管事件订阅。
- `CreateWidget<T>(...)`
- `CreateWidgetByPath<T>(...)`
- `CreateWidgetByPathAsync<T>(...)`
- `AdjustItemNum(...)`
- `CreateRedDot(...)`
- `CreateRedDotAsync(...)`

`UIWindow` 生命周期约定：

```text
ScriptGenerator()
-> BindMemberProperty()
-> RegisterEvent()
-> OnCreate()
-> OnRefresh()
-> OnDestroy()
```

UI 开发落位：

- 完整页面/弹窗放 `GameLogic/UI/`，继承 `UIWindow`。
- 复用组件继承 `UIWidget`。
- Tab 子页面走 `SwitchPageMgr` / `BaseChildPage`。
- 大数据列表优先用 `UILoopListWidget` / `UILoopGridWidget` / SuperScrollView 相关封装。
- 事件监听放 `RegisterEvent()`，优先 `AddUIEvent(...)`。

### 7.2 红点系统

核心目录：

- `GameUnity/Assets/Scripts/HotFix/GameLogic/Module/RedDotModule/`

核心类型：

- `RedDotModule`：红点树管理入口。
- `RedDotNode`：红点树节点，保存 id、path、value、父子关系和监听。
- `RedDotItem`：UI 侧红点 Widget。
- `RedDotTreeConfig`：编辑器配置。
- `RedDotPathDefine_Gen.g.cs`：生成的红点路径定义。

`RedDotModule` 常用接口：

- `Register(...)`
- `GetNode(int id)` / `GetNode(string path)`
- `HasNode(...)`
- `Unregister(...)`
- `SetValue(...)`
- `AddValue(...)`
- `GetValue(...)`
- `IsShow(...)`
- `ClearNodeValue(...)`
- `ClearAll()`
- `AddListener(...)`
- `RemoveListener(...)`
- `GetAllLeafNodes()`
- `GetAllNodes()`
- `PrintTree()`

使用边界：

- UI 侧通过 `UIBase.CreateRedDot(...)` 或 `CreateRedDotAsync(...)` 创建显示项。
- 节点路径优先用生成的 `RedDotPathDefine`，不要散落硬编码字符串。
- 红点表现和业务计算分开：业务模块更新值，UI 监听节点变化。

### 7.3 多语言与文本

多语言目录：

- `GameUnity/Assets/Scripts/HotFix/GameLogic/Module/LocalizationModule/`

文本目录：

- `GameUnity/Assets/Scripts/HotFix/GameLogic/Module/TextModule/`

核心类型：

- `ILocalizationModule` / `LocalizationModule`：语言选择、当前语言、系统语言、helper 注入。
- `DGameLocalizationHelper`：当前语言实现与语言切换事件触发。
- `TextConfigMgr`：从 `TbTextConfig` 读取文本配置。
- `G`：文本读取便捷入口。
- `UITextIDBinder`：UI 文本绑定组件，监听语言切换并刷新。

常用接口：

- `LocalizationModule.SetLocalizationHelper(...)`
- `LocalizationModule.SetLanguage(LocalAreaType language)`
- `LocalizationModule.CurrentLanguage`
- `TextConfigMgr.Instance.GetText(int id, params object[] args)`
- `G.R(int id)` / `G.R(TextDefine id)`
- `UITextIDBinder.SetText(...)`

语言切换事件链：

```text
DGameLocalizationHelper.SetLanguage(...)
  -> GameEvent.Get<ILocalization>().OnLanguageChanged(...)
  -> UITextIDBinder / UIWindow AddUIEvent 监听
  -> 刷新 UI 文本
```

### 7.4 配置管理器

目录：

- `GameUnity/Assets/Scripts/HotFix/GameLogic/ConfigMgr/`
- `GameUnity/Assets/Scripts/HotFix/GameLogic/Module/TextModule/TextConfigMgr.cs`

当前已有封装：

- `ModelConfigMgr`：封装 `TbModelConfig`。
- `SoundConfigMgr`：封装 `TbSoundConfig`。
- `TextConfigMgr`：封装 `TbTextConfig`。

建议：

- 业务层优先通过 `ConfigMgr` 访问配置。
- 不要把 `ConfigSystem.Instance.Tables` 或 `TbXXX` 访问散落到 UI/模块里。
- 新增配置频繁使用时，优先新增一个业务语义清晰的 `ConfigMgr`。

### 7.5 DataCenter 与客户端数据

目录：

- `GameUnity/Assets/Scripts/HotFix/GameLogic/DataCenter/`

核心类型：

- `DataCenterSys`：数据中心总控，管理当前玩家数据、角色 ID、清理客户端数据。
- `DataCenterModule<T>`：业务数据模块基类，提供 `OnInit`、`OnRoleLogin`、`OnRoleLogout`、`OnUpdate`、`OnMainPlayerMapChange` 生命周期。
- `GameClient`：网络客户端入口，包含连接、断开、重连、发送、RPC、消息处理注册。
- `ClientSaveDataMgr`：客户端本地存档管理。
- `BaseClientSaveData`：存档基类。
- `ClientSaveDataAttribute`：存档 key、按角色存储、存储模式标记。
- `SystemSaveData`：系统设置存档样例。

`GameClient` 常用接口：

- `Connect(string address, int port, bool reconnect = false)`
- `Disconnect()`
- `Reconnect()`
- `Send<T>(T message, uint rpcID = 0, long routeID = 0)`
- `Call<T>(T request, long routeId = 0)`
- `RegisterMsgHandler(uint protocolCode, Action ctx)`
- `UnRegisterMsgHandler(uint protocolCode, Action ctx)`

当前判断：

- `DataCenter` 具备基础框架，但业务数据类型还很少。
- 网络协议和消息分发更像预留接入点，后续参考时需要结合具体项目协议实现补充。

### 7.6 SingletonSystem

目录：

- `GameUnity/Assets/Scripts/HotFix/GameLogic/Module/SingletonSystem/`

定位：

- HotFix 业务侧单例体系。
- `UIModule`、`RedDotModule`、`ConfigMgr`、`DataCenterSys` 等使用 `Singleton<T>`。
- `GameStart.OnDestroy()` 会调用 `SingletonSystem.Destroy()` 清理。

## 8. 事件系统

Runtime 底层：

- `GameUnity/Assets/DGame/Runtime/Core/GameEvent/GameEvent.cs`
- `GameUnity/Assets/DGame/Runtime/Core/GameEvent/GameEventDriver.cs`
- `GameUnity/Assets/DGame/Runtime/Core/GameEvent/EventInterfaceAttribute.cs`

HotFix 定义：

- `GameUnity/Assets/Scripts/HotFix/GameLogic/IEvent/`
- `GameUnity/Assets/Scripts/HotFix/GameBattle/IEvent/`

分层：

```text
业务接口定义
  [EventInterface(EEventGroup.Xxx)] + IEvent 接口
        ↓
Source Generator 生成
  IXxx_Event.g.cs / IXxx_Gen.g.cs / GameEventHelper.g.cs
        ↓
使用层
  GameEvent.Get<T>().Method()
  AddUIEvent(IXxx_Event.Method, callback)
        ↓
底层分发
  GameEvent / EventDispatcher / GameEventDriver
```

当前已定义接口：

| 接口 | 分组 | 方法 |
| --- | --- | --- |
| `ICommonUI` | `GroupUI` | `ShowWaitingUI(uint waitFuncID, uint textID, Action callback)` |
| `ILoginUI` | `GroupUI` | `ShowLoginUI()` |
| `ILocalization` | `GroupUI` | `OnLanguageChanged(int language)` |
| `IBattle` | 未标注 | 当前为空，占位。 |

使用原则：

- 新事件先定义接口，不手写 int ID。
- 发送优先 `GameEvent.Get<IXxx>().Method(...)`。
- UI 监听优先 `AddUIEvent(IXxx_Event.Method, callback)`。
- `*_Event.g.cs`、`*_Gen.g.cs`、`GameEventHelper.g.cs` 是生成产物，不手改。

## 9. 配置系统与 Luban

源头目录：

- `GameConfig/Datas/`
- `GameConfig/Defines/`
- `GameConfig/CustomTemplate/`
- `GameConfig/GenerateTool_Binary/`
- `GameConfig/GenerateTool_Json/`

客户端输出：

- 代码：`GameUnity/Assets/Scripts/HotFix/GameProto/LubanConfig/`
- 数据：`GameUnity/Assets/BundleAssets/Configs/Binary/`
- 入口：`GameUnity/Assets/Scripts/HotFix/GameProto/ConfigSystem.cs`

运行时加载：

```text
ConfigSystem.Instance.Tables
  -> 首次访问触发 Load()
  -> new Tables(LoadByteBuf)
  -> LoadByteBuf(file)
  -> ModuleSystem.GetModule<IResourceModule>()
  -> LoadAsset<TextAsset>(file)
  -> new ByteBuf(textAsset.bytes)
```

生成表常用访问形式：

- `TbItemConfig.GetOrDefault(id)`
- `TbItemConfig.TryGetValue(id, out cfg)`
- `TbItemConfig.DataMap`
- `TbItemConfig.DataList`
- `ConfigSystem.Instance.Tables.TbItemConfig`

业务建议：

- 直接表访问只适合局部或基础代码。
- 业务模块/UI 层优先依赖 `ConfigMgr`。
- 修改配置结构时改 `GameConfig` 源表/schema/模板，再跑导表脚本。

默认客户端二进制 LazyLoad 脚本：

- `GameConfig/GenerateTool_Binary/gen_bin_client_lazyload.bat`

## 10. 资源组织

运行时资源主目录：

- `GameUnity/Assets/BundleAssets/`

关键子目录：

- `Actor/`：角色资源。
- `Audios/`：音频。
- `Configs/`：配置数据。
- `DLL/`：热更 DLL bytes。
- `Effects/`：特效。
- `Fonts/`：字体。
- `FrameSprite/`：序列帧。
- `Materials/`：材质。
- `Prefabs/`：通用预制体。
- `Scenes/`：场景。
- `UI/`：UI Prefab。
- `UIRaw/Atlas/`：UI 图集源图。
- `UIRaw/Raw/`：UI 原图。

美术/图集产物：

- `GameUnity/Assets/AssetArt/Atlas/`

资源约束：

- 运行时资源必须进入 `BundleAssets` 并由 YooAsset 管理。
- 运行时不要使用 `Resources.Load()` 直接加载资源。
- location 约定为资源文件名，不带路径和扩展名。
- 预加载资源使用 `PRELOAD` 标签。

## 11. Editor 工具链

主编辑器工具目录：

- `GameUnity/Assets/DGame/Editor/`
- `GameUnity/Assets/Editor/`
- `GameUnity/Assets/Scripts/HotFix/GameLogic/Editor/`

常见职责：

- DGame 菜单与工具栏扩展。
- HybridCLR 相关生成和构建辅助。
- Luban 导表辅助。
- UI 绑定代码生成。
- 红点树编辑器和代码生成。
- 文本绑定编辑器。

注意：

- Editor 可引用 HotFix 类型做开发期工具，但不能让这些依赖泄漏到 Runtime/AOT 发布路径。
- 生成文件优先由工具生成，不手动维护。

## 12. 后续参考时的落位规则

| 要做的事 | 优先落位 |
| --- | --- |
| 底层运行时模块、通用引擎能力 | `GameUnity/Assets/DGame/Runtime/Module/` 或 `Runtime/Core/` |
| 启动流程、资源更新、热更 DLL 加载 | `GameUnity/Assets/DGame.AOT/Procedure/` |
| 热更业务逻辑、UI、业务模块 | `GameUnity/Assets/Scripts/HotFix/GameLogic/` |
| 配置表生成代码、配置系统入口 | `GameUnity/Assets/Scripts/HotFix/GameProto/` |
| 战斗纯逻辑 | `GameUnity/Assets/Scripts/HotFix/GameBattle/` |
| 配置源表、schema、模板、导表脚本 | `GameConfig/` |
| 运行时资源 | `GameUnity/Assets/BundleAssets/` |
| 编辑器工具 | `GameUnity/Assets/DGame/Editor/` 或对应业务 `Editor/` |

判断原则：

1. 先判断是否运行时、AOT 启动、HotFix 业务、配置生成、资源或 Editor。
2. 先复用现有模块和门面，不新增平行入口。
3. HotFix 访问底层模块优先走 `GameModule`。
4. 跨模块协作优先走接口事件或已有模块 API。
5. 配置访问优先封装到 `ConfigMgr`。
6. UI 走 `UIModule` / `UIWindow` / `UIWidget`，不要绕开窗口栈。
7. 资源走 `IResourceModule`，对象高频复用走 `IGameObjectPoolModule`。

## 13. 当前项目状态判断

当前仓库更像一个完整框架样板，而不是已经填满业务的游戏项目：

- 启动、热更、资源、配置、UI、红点、文本、数据中心等基础设施完整。
- `GameLogic/UI` 里已有 `Main`、`Common`、`GMPanel` 等基础界面。
- `GameProto/LubanConfig` 已有道具、模型、音效、文本、GM、特效等基础配置。
- `GameBattle` 当前内容很少，事件接口也是空占位。
- 事件接口数量较少，业务模块之间的复杂协作还未展开。

因此后续参考这个项目时，最适合参考的是框架落位、启动热更链路、资源/配置/UI/红点/文本这些基础设施；如果要参考完整玩法业务，还需要结合实际业务项目补充。
