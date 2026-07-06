# DGame 模块 API 速查

> **适用场景**：使用 GameModule.ResourceModule、UIModule、AudioModule、SceneModule、GameTimerModule、RedDotModule 等模块 API | **关联文档**：[ui-lifecycle.md](ui-lifecycle.md)、[resource-api.md](resource-api.md)、[event-system.md](event-system.md)

## 核心 API：GameModule 统一访问入口

热更业务通过 `GameLogic.GameModule` 访问模块。`GameModule` 会缓存底层模块，避免业务代码散落 `ModuleSystem.GetModule<T>()`。

```csharp
GameModule.RootModule          // RootModule
GameModule.FsmModule           // IFsmModule
GameModule.SensitiveWordModule // ISensitiveWordModule
GameModule.AnimModule          // IAnimModule
GameModule.ResourceModule      // IResourceModule
GameModule.AudioModule         // IAudioModule
GameModule.SceneModule         // ISceneModule
GameModule.GameTimerModule     // IGameTimerModule
GameModule.InputModule         // DGame.IInputModule
GameModule.Input               // GameLogic.IInputModule，ENABLE_INPUT_SYSTEM 下可用
GameModule.LocalizationModule  // ILocalizationModule
GameModule.GameObjectPool      // IGameObjectPoolModule
GameModule.UIModule            // UIModule.Instance
GameModule.RedDotModule        // RedDotModule.Instance

GameModule.Destroy()           // 清空缓存，仅退出/热更销毁时调用
```

> **注意**：`UIModule`、`RedDotModule` 是热更层单例；其他大多数模块来自 `ModuleSystem.GetModule<T>()`。

---

## 使用模式

### GameTimerModule 计时器

```csharp
private GameTimer m_timer;

protected override void OnCreate()
{
    m_timer = GameModule.GameTimerModule.CreateOnceGameTimer(1f, _ => Refresh());
}

protected override void OnDestroy()
{
    GameModule.GameTimerModule.DestroyGameTimer(m_timer);
    m_timer = null;
}
```

常用 API：

- `CreateOnceGameTimer`
- `CreateLoopGameTimer`
- `CreateUnscaledOnceGameTimer`
- `CreateUnscaledLoopGameTimer`
- `CreateLoopCountGameTimer`
- `DestroyGameTimer`

### SceneModule 场景管理

```csharp
await GameModule.SceneModule.LoadSceneAsync("BattleScene");
await GameModule.SceneModule.UnloadAsync("OldScene");
GameModule.SceneModule.Unload("OldScene");
```

### AudioModule 音频

```csharp
GameModule.AudioModule.Play(DGame.AudioType.UISound, soundCfg.Location, isInPool: true);
GameModule.AudioModule.Play(DGame.AudioType.Music, bgmPath, isLoop: true);
GameModule.AudioModule.Stop(DGame.AudioType.Music, fadeout: true);
GameModule.AudioModule.StopAll(fadeout: true);

GameModule.AudioModule.Volume = 0.8f;
GameModule.AudioModule.MusicVolume = 0.6f;
GameModule.AudioModule.SoundVolume = 1.0f;
GameModule.AudioModule.UISoundVolume = 1.0f;
GameModule.AudioModule.VoiceVolume = 1.0f;
```

开关类属性：`Enabled`、`MusicEnable`、`SoundEnable`、`UISoundEnable`、`VoiceEnable`。

`Stop` / `StopAll` 的真实签名都需要 `bool fadeout` 参数。

### LocalizationModule 本地化

```csharp
GameModule.LocalizationModule.SetLocalizationHelper(new DGameLocalizationHelper());
GameModule.LocalizationModule.SetLanguage(LocalAreaType.CN);
```

语言变化通过 `ILocalization_Event.OnLanguageChanged` 通知 UI。

### SensitiveWordModule 敏感词

```csharp
GameModule.SensitiveWordModule.SetKeywords(keywords);
bool hasSensitiveWord = GameModule.SensitiveWordModule.ContainsSensitiveWord(input);
string safeText = GameModule.SensitiveWordModule.ReplaceSensitiveWords(input);
```

常用 API：

- `SetKeywords(List<string> keywords)`
- `SetKeywords(List<string> keywords, List<int> blacklistTypes)`
- `ContainsSensitiveWord(string content)`
- `ReplaceSensitiveWords(string content, char replaceChar = '\0')`
- `FindAllSensitiveWords(string content)`
- `FindFirst(string content)` / `FindAll(string content)`：黑名单分级查询。

### AnimModule 动画图

```csharp
var playable = GameModule.AnimModule.CreateAnimPlayable(animator, animations);
playable.Play("Idle", fadeDuration: 0.2f);
GameModule.AnimModule.DestroyAnimPlayable(playable);
```

常用 API：

- `ContainsAnimPlayable(string name)`
- `GetAnimPlayable(string name)`
- `CreateAnimPlayable(Animator animator)`
- `CreateAnimPlayable(Animator animator, List<AnimationClip> animations)`
- `CreateAnimPlayable(Animator animator, List<AnimationWrapper> animations)`
- `DestroyAnimPlayable(IAnimPlayable animPlayable)` / `DestroyAnimPlayable(string name)`

创建的 `IAnimPlayable` 必须有明确销毁点，避免 PlayableGraph 长期保留。

### FsmModule 有限状态机

入口：

```csharp
var fsm = GameModule.FsmModule.CreateFsm(owner,
    new IdleState(),
    new BattleState());

fsm.Start<IdleState>();
```

常用 API：

- `ContainsFsm<T>()` / `ContainsFsm<T>(string name)`
- `GetFsm<T>()` / `GetFsm<T>(string name)`
- `CreateFsm<T>(owner, params IFsmState<T>[] states)`
- `CreateFsm<T>(string name, owner, params IFsmState<T>[] states)`
- `DestroyFsm<T>()` / `DestroyFsm<T>(string name)` / `DestroyFsm(fsm)`

规则：业务层通过 `GameModule.FsmModule` 获取，不散落 `ModuleSystem.GetModule<IFsmModule>()`。创建的状态机必须有明确销毁点，避免 owner 已销毁但状态仍在更新。

### GameObjectPool 对象池

入口：

```csharp
await GameModule.GameObjectPool.CreateGameObjectPoolAsync(
    location,
    initCapacity: 5,
    maxCapacity: 50,
    autoDestroyTime: 30f,
    ct: cancellationToken);

var go = await GameModule.GameObjectPool.SpawnAsync(location, parent, cancellationToken);
GameModule.GameObjectPool.Recycle(go);
```

常用 API：

- `CreateGameObjectPoolAsync(location, initCapacity, maxCapacity, autoDestroyTime, dontDestroy, allowMultiSpawn, ct)`
- `SpawnAsync(location, ct)`
- `SpawnAsync(location, parent, ct)`
- `SpawnAsync(location, parent, position, rotation, ct)`
- `Recycle(gameObject)`：回收到池。
- `Remove(gameObject)`：从池管理中移除并丢弃。
- `DestroyPool(location)` / `DestroyAllPool(includeAll)`
- `TryGetGameObjectPool(location, out pool)` / `GetDebugInfos(results)`

规则：池化对象不要手动 `Destroy` 后再 `Recycle`。对象不再复用时用 `Remove`，整池不用时用 `DestroyPool`。

### DataCenterModule 数据中心模块

热更业务的角色数据、跨 UI/模块共享状态可收口到 `DataCenterModule<T>`。继承 `DataCenterModule<T>` 的类会由 `DataCenterModuleGenerator` 源代码生成器自动注册到 `DataCenterSys.m_dataCenterModuleList`。

生成器会扫描 `GameLogic` 命名空间下继承 `DataCenterModule...` 的类，并生成：

- `partial void InitModule()`
- `RegisterModule(IDataCenterModule module)`
- `RegisterModule(XxxDataCenter.Instance)` 调用

`DataCenterSys.OnInit()` 调用 `InitModule()`；注册时会先执行模块 `OnInit()`，再加入 `m_dataCenterModuleList`。`DataCenterSys.OnUpdate()` 会逐个调用模块 `OnUpdate()`；`ClearClientData()` 会在有当前角色数据时关闭所有窗口并调用模块 `OnRoleLogout()`。

示例：

```csharp
namespace GameLogic
{
    public sealed class BagDataCenter : DataCenterModule<BagDataCenter>
    {
        private readonly Dictionary<int, int> m_itemCounts = new Dictionary<int, int>();

        public override void OnInit()
        {
            m_itemCounts.Clear();
        }

        public override void OnRoleLogin()
        {
            // 拉取或重建当前角色背包缓存。
        }

        public override void OnRoleLogout()
        {
            m_itemCounts.Clear();
        }

        public override void OnMainPlayerMapChange()
        {
            // 处理地图切换后需要刷新的角色状态。
        }

        public int GetCount(int itemId)
        {
            return m_itemCounts.TryGetValue(itemId, out var count) ? count : 0;
        }
    }
}
```

使用：

```csharp
int count = BagDataCenter.Instance.GetCount(itemId);
```

规则：

- 类放在 `GameLogic` 命名空间下，并继承 `DataCenterModule<自身类型>`。
- 类必须可 `new()`，不要依赖有参构造。
- 不要手动改 `DataCenterSys.m_dataCenterModuleList`、`InitModule()` 或生成文件 `DataCenterModule_Gen.g.cs`。
- `DataCenterModule<T>` 适合放角色级/客户端级状态和生命周期回调；不要把纯配置查询替代成 DataCenter，配置访问仍优先走 `ConfigMgr`。
- 需要登出清理的数据放在 `OnRoleLogout()`，避免切账号后旧角色状态残留。

### MemoryPool 内存池

DGame 内存池入口是 `MemoryPool.Spawn<T>()` / `MemoryPool.Release(...)`，不是 `Acquire`。

```csharp
public sealed class MyPayload : MemoryObject
{
    public int Value;

    public override void OnRelease()
    {
        Value = 0;
    }
}

var payload = MemoryPool.Spawn<MyPayload>();
payload.Value = 100;
MemoryPool.Release(payload);
```

`MemoryObject` 提供同名封装：

```csharp
var driver = MemoryObject.Spawn<GameEventDriver>();
MemoryObject.Release(driver);
```

常用 API：

- `MemoryPool.Spawn<T>() where T : class, IMemory, new()`
- `MemoryPool.Release(IMemory memory)`
- `MemoryPool.Release<T>(List<T> memories)`
- `MemoryPool.Add<T>(int count)` / `MemoryPool.Remove<T>(int count)`
- `MemoryPool.ClearMemoryCollector<T>()` / `MemoryPool.ClearAll()`
- `MemoryPool.EnableStrictCheck`：严格检查开关，调试时使用。

规则：可池化对象实现 `IMemory`，或继承 `MemoryObject` 并在 `OnRelease()` 中清理状态。不要把 Unity `GameObject` 生命周期和 `MemoryPool` 混用；GameObject 复用使用 `GameObjectPool`。

### DLogger 日志

入口位于 `DGame/Runtime/Core/DGameLog/DLogger.cs`：

```csharp
DLogger.Log("debug message");
DLogger.Info("module ready");
DLogger.Warning("asset missing: {0}", location);
DLogger.Error("load failed: {0}", error);
DLogger.Fatal(exception);
DLogger.Assert(condition, "unexpected state");
```

`DLogger` 方法带条件编译属性，日志是否输出以 `DLogger.cs` 对应方法的 `[Conditional]` 标注为准。例如 debug 级别方法受 `ENABLE_DGAME_LOG`、`ENABLE_DGAME_DEBUG_LOG`、`ENABLE_DGAME_DEBUG_AND_ABOVE_LOG` 控制。新增业务日志优先用 `DLogger`，不要直接散落 `UnityEngine.Debug.Log`。

---

## 新增模块规则

1. 先搜索 DGame 是否已有封装：`GameTimer`、`InputModule`、`AnimModule`、`MemoryPool`、`GameObjectPool`、`GameEvent`、`LocalizationModule`。
2. 框架模块放 `DGame/Runtime/Module` 并注册到 `ModuleSystem`。
3. 业务单例模块放 `GameLogic/Module`，沿用 `Singleton<T>`。
4. 需要业务统一访问时，在 `GameModule` 增加缓存属性。

---

## 常见错误
| 错误写法 | 正确写法 | 原因 |
|---------|---------|------|
| `ModuleSystem.GetModule<IResourceModule>()` | `GameModule.ResourceModule` | 业务层使用统一缓存入口 |
| 计时器不销毁 | `DestroyGameTimer(m_timer)` | 回调可能引用已销毁 UI |
| `GameModule.UI` | `GameModule.UIModule` | DGame 实际属性名是 `UIModule` |
| `GameModule.Timer` | `GameModule.GameTimerModule` | DGame 实际属性名是 `GameTimerModule` |
| `MemoryPool.Acquire<T>()` | `MemoryPool.Spawn<T>()` | DGame 实际方法名是 `Spawn` |
| `AudioModule.StopAll()` | `AudioModule.StopAll(bool fadeout)` | DGame 需要显式 fadeout |
| Runtime 模块引用 GameLogic 单例 | 通过接口/事件解耦 | 保持程序集边界 |
| 池化对象直接 `Destroy` | `GameModule.GameObjectPool.Recycle/Remove` | 保持池状态一致 |
| 业务直接 `Debug.Log` | `DLogger` | 保持日志宏和项目日志通道一致 |

---

## 交叉引用

- UI 模块见 [ui-lifecycle.md](ui-lifecycle.md)
- 资源模块见 [resource-api.md](resource-api.md)
- 事件系统见 [event-system.md](event-system.md)
- 红点系统见 [reddot-system.md](reddot-system.md)
