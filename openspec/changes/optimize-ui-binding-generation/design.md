## Context

当前 UI 自动绑定由 `DGame.Editor` 中的 `UIScriptAutoGenerator` 扫描节点前缀，再分别执行 `GenerateUIComponentScript()` 与 `GenerateCSharpScript()`。前者将引用顺序写入 `GameLogic.UIBindComponent.m_components`，后者以同一遍历顺序递增 `m_bindIndex` 并生成 `GetComponent<T>(index)`。`UIBindComponent` Inspector 使用允许排序、增删和替换元素的 `ReorderableList`，因此组件数组顺序构成不可见的运行时 ABI。

生成器只从根节点子级开始扫描，`m_item` 既生成父级 `CreateWidget<T>` 引用，又阻止扫描进入其子树；Widget 根自身挂载的 Button、Image 或 CanvasGroup 无法被自动绑定。`UIWindow` 和 `UIWidget` 创建后固定按 `ScriptGenerator -> BindMemberProperty -> RegisterEvent -> OnCreate -> OnRefresh` 执行，且业务逻辑位于可保留的 partial 文件。改造必须保留这一运行时生命周期和程序集边界：编辑器代码留在 `DGame.Editor`，运行时容器与业务 UI 留在 `GameLogic` 热更程序集。

## Goals / Non-Goals

**Goals:**

- 以可序列化、稳定且强类型校验的 `BindingEntry` 作为 Prefab 绑定与代码生成的唯一事实来源。
- 支持一个 GameObject 的多个组件分别声明为不同字段，并让展示顺序不影响运行时含义。
- 让 Widget 边界、组件绑定、前缀建议与事件策略各自独立表达。
- 在写入 Prefab 或生成代码前完整发现错误；运行时遇到损坏绑定时带上下文失败，不返回空对象继续运行。
- 迁移已有 `m_components` Prefab，并在全部迁移和回归通过后删除索引读取路径。

**Non-Goals:**

- 不引入 MVVM、数据观察、双向绑定、运行时按路径或名称搜索 UI。
- 不变更 `UIWindow`/`UIWidget` 生命周期、`GameModule.UIModule`、资源加载或跨模块事件架构。
- 不自动修改已有业务 partial 文件；也不以空 partial 方法静默吞掉新增 UnityEvent。
- 不把所有 Unity 组件自动暴露为业务字段，仍只从可配置的业务组件白名单中提出建议。

## Decisions

### 1. Binding Manifest 是唯一事实来源

`UIBindComponent` 以序列化条目替换 `List<Component>`：

```csharp
[Serializable]
public sealed class UIBindingEntry
{
    [SerializeField] private string m_bindingId;
    [SerializeField] private string m_fieldName;
    [SerializeField] private string m_expectedTypeName;
    [SerializeField] private Component m_target;
    [SerializeField] private UIBindingKind m_kind;
    [SerializeField] private bool m_generateUnityEvent;
    [SerializeField] private string m_eventHandlerName;
    [SerializeField] private string m_widgetTypeName;
}
```

`m_bindingId` 在条目创建时生成 GUID 且永不因重命名、移动或重排改变；普通组件的 `m_expectedTypeName` 记录目标组件完整类型名，Widget 条目的 `m_expectedTypeName` 则记录 `UIWidget` 子类完整类型名。`m_target` 始终是 `Component`；Widget 条目以其根 `RectTransform` 为目标，并从预期 Widget 类型生成 `CreateWidget<T>`，避免把业务 Widget 类型误表示成 Prefab 组件类型。

替代方案是继续保存稳定排序的数组或在生成代码内嵌数组下标。它们减少短期改动，却仍让顺序承担 ABI，因此不采用。直接保存 `System.Type` 会受到 Unity 序列化和程序集重命名限制，也不采用。

### 2. 运行时按稳定 ID 必需解析

`UIBindComponent` 在首次读取时从条目构建只读 ID 到条目的索引，`GetRequired<T>(bindingId)` 必须检查重复 ID、缺失条目、空目标和类型不匹配。任一失败必须抛出包含 Prefab 层级、ID、字段名、期望类型和实际类型的 `InvalidOperationException`，从而中止该 UI 创建；不得记录日志后返回 null。

每个 `*_Gen.g.cs` 生成本类私有的绑定 ID 常量，并以 `GetRequired<T>(...)` 取得字段。ID 查找仅在 Window 或 Widget 创建时发生，不在 Update 路径执行。相比位置数组，少量 UI 创建期字典开销可接受；若性能分析显示热点，再以不可变 ID 索引表优化，但语义不得退回到可编辑位置。

### 3. 前缀只负责建议，Widget 边界显式化

`UIScriptGeneratorSettings.scriptGenerateRulers` 继续提供“建议组件类型、默认字段名、默认事件类型”的输入。Inspector 将建议转为可编辑 `BindingEntry`，生成器只能读取已确认的清单，不得绕过清单按前缀直接生成。

前缀建议保留 `m_item` 的 `isUIWidget` 语义。Prefab Stage 选中任意 GameObject 时，GameObject Inspector 枚举该节点现有组件并按规则白名单过滤，为每个可绑定对象独立显示 `+ 类型` / `- 类型`。点击 `+` 只向最近的所属 Manifest 增加该组件的一条 BindingEntry，点击 `-` 只删除对应条目；不提供“收集全部”、批量多选或额外 GameObject 菜单入口。字段名在已绑定时可直接编辑，默认建议使用组件前缀和节点语义组合，例如 `m_itemTest` 上的 Button 建议为 `m_btnItemTest`。

普通节点的组件归属最近父 `UIBindComponent`。Widget 根同时显示两种归属：父 UI 只持有 Widget 条目，Widget 根自身的 Button、Image、CanvasGroup 等组件归属自己的 Manifest。若 `m_item` 节点尚未配置 Widget 根，点击父 UI 的 `+ Widget` 会创建或启用该节点的 Manifest、设置 `IsWidgetRoot`，并从子节点 `className` 或节点语义填入 Widget 类型；组件条目的预期类型和 Widget 条目的业务类型均由目标自动同步，不允许在清单中手工改成其他类型。

`UIBindComponent` 新增 `IsWidgetRoot` 元数据；父 UI 的 `Widget` 条目保存目标根 `RectTransform` 和 `widgetTypeName`，子 Widget 根自己的清单负责根节点和内部控件。扫描父 UI 时遇到子 `IsWidgetRoot` 即止步；扫描该 Widget 时包含其根节点。`m_item` 仅用于创建迁移建议：建议建立 Widget 条目并设置 `IsWidgetRoot`，不再是唯一边界事实。

替代方案是独立 `UIWidgetRoot` MonoBehaviour。它可行，但需要额外组件和 Inspector 跳转；边界与绑定清单共址能减少 Prefab 配置分散，故采用前者。

### 4. 校验优先于确定性写入，签名负责暴露陈旧产物

编辑器主入口改为“校验并生成”：收集 manifest、计算生成模型、验证所有规则、预生成全部代码文本，全部通过后才批量保存 Prefab 和替换 `*_Gen.g.cs`。校验项至少包括 GUID 唯一性、合法字段名、字段名/事件名冲突、目标非空、目标属于当前 Prefab、目标类型兼容、Widget 所有权与边界、事件处理方法存在、生成路径与类型可解析。

生成器对规范化后的 manifest 和生成器版本计算签名，并将签名写入 Prefab 元数据与生成文件头。Inspector 显示“已同步/已陈旧”；任一不一致必须明确报出而不是重新按节点扫描猜测。文件系统无法为 Prefab 与 C# 提供跨资产事务，写入期间的 I/O 失败必须完整报错，并以签名不匹配暴露中断状态，禁止把半完成状态视作成功。

### 5. UnityEvent 必须有显式业务处理器

条目只有启用 `m_generateUnityEvent` 时才生成监听。生成前使用 Unity/Roslyn 可见的脚本语义检查确认业务 partial 中存在签名匹配的处理器；无处理器即校验失败，不写监听。工具可提供独立的“创建待实现处理器”命令，其生成体必须显式抛出 `NotImplementedException`，而非生成空方法或未实现 partial 声明。这样既不覆盖业务文件，也不会让用户输入被编译器静默移除。

### 6. 迁移是编辑器期一次性转换，不是运行时双读

迁移工具依据当前生成代码与 `m_components` 的现存顺序导入初始条目，为每条目分配 ID，并将结果交给常规校验与生成。旧数组只能在迁移工具中读取，新的运行时 `GetRequired<T>` 不提供数组回退。每个 Prefab 必须在切换后生成、编译并验证后才计入批次完成；全部目标 Prefab 完成后删除 `m_components`、`GetComponent<T>(int)` 与旧菜单入口。

## Risks / Trade-offs

- [旧 Prefab 或生成文件无法可靠对应数组索引] → 迁移器必须输出逐条报告并阻止该 Prefab 切换；由开发者补全条目，不猜测或自动回退。
- [GUID、字段重命名和生成签名带来更多 Inspector 数据] → 把 ID 设为只读，字段改名时显示受影响的生成类与事件；签名只存固定长度摘要。
- [一次性批量写入被 Unity 编译或磁盘错误打断] → 预写入前完成全部校验，写入失败显式失败；后续 Inspector 以签名不匹配标记为陈旧，要求重新执行完整生成。
- [严格事件处理器检查提高初次制作成本] → 将缺失处理器显示为可定位的阻断项，并提供显式抛错的骨架创建命令；不使用空回调降低成本。
- [迁移期同时维护旧 UI] → 按 Prefab 小批量切换；未迁移 Prefab 仍走旧生成产物，已迁移 Prefab 只走新 API，避免单个 Prefab 双读。

## Migration Plan

1. 实现 `BindingEntry`、必需读取 API、GameObject Inspector 逐组件 `+/-` 和生成器模型，但保留旧数组仅供编辑器导入工具读取；删除批量收集菜单，旧索引入口仅保留给未迁移 Prefab。
2. 为代表性 Window、普通 Widget、动态列表 Item 和“同节点多组件”Prefab 建立迁移测试集；迁移单个 Prefab，生成新代码，使用 AIBridge 编译并在 Play Mode 验证生命周期和事件。
3. 按功能目录批量迁移，记录每个 Prefab 的 manifest/代码签名、编译结果和运行时验证证据；任何校验失败的 Prefab 留在旧批次，不自动切换。
4. 所有目标 Prefab、自动化编辑器测试和运行时回归通过后，删除旧数组、索引访问、旧绑定 Inspector 与旧生成菜单，并更新 DGame UI 规范。

回退仅限尚未删除旧模型时对未完成批次恢复版本控制中的 Prefab 与生成文件；已迁移 Prefab 不在运行时尝试旧数组访问。删除旧模型的提交必须在完整回归通过后独立落库，方便通过版本控制整体回退。

## Open Questions

- `UIBindingEntry` 的运行时索引应懒构建还是在 `Awake` 构建；实施时以 Unity 序列化/域重载行为和 profiler 结果决定，均不得改变必需解析语义。
- 编辑器事件处理器检查采用 Roslyn 分析还是 Unity `TypeCache` 加文本位置映射；需先核验项目当前 Unity/Roslyn 可用 API。
- 旧 Prefab 的迁移范围以全部 `BundleAssets/UI` 为目标还是先以正在维护的 UI 目录为首批；该范围影响任务拆分但不影响架构契约。
