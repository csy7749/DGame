# DGame 项目级配置边界与候选项

## 目的

项目级配置用于保存会被多个资源或工具共同消费、并且需要随项目版本管理的稳定约定。它不应替代每个领域已有的专用配置，也不保存个人偏好、一次性导出目录或运行时业务数据。

## 当前项目级配置

UI 代码生成统一使用：

- 配置资产：`GameUnity/Assets/Editor/UIScriptGenerator/UIScriptGeneratorSettings.asset`
- 配置入口：`Project Settings > DGame > UI代码生成器设置`
- 自动代码路径：`genCodePath`
- 业务实现根路径：`impCodePath`；Window 派生为 `<WindowName>/<WindowName>.cs`，其他类型派生为 `Item/<TypeName>.cs`
- 手动创建 UIText 的默认字体：`defaultUITextFont`
- 其他同域设置：命名空间、Widget 节点标记、字段风格、UI 类型和节点前缀规则

`UIBindComponent` 只保存单个 Prefab 自身的类名、UI 类型、Manifest 和绑定条目，不再复制项目路径。修改项目配置后，所有普通生成和 Manifest 生成任务立即读取新路径。

自动代码生成会同时创建业务逻辑骨架。业务文件已经存在时不会覆盖；同名文件位于旧目录时会终止生成并报告当前路径和目标路径。

### 迁移影响

- 旧 Prefab YAML 中可能暂时残留 `genCodePath`、`impCodePath` 字段，但当前组件已不声明或消费它们，Unity 下次保存该 Prefab 时会清理这些未知序列化字段。
- 旧的单 Prefab 自定义路径不再生效。例如 `GuideUI.prefab` 曾把实现类路径设为 `Assets/Scripts/HotFix/GameLogic/UI/Common`，现在统一使用项目配置中的 `impCodePath`。
- 本次不批量重写 Prefab，避免把路径迁移与当前 UI 结构、Manifest 改动混在同一份差异里。

## 建议纳入共享项目配置

| 配置项 | 当前事实或重复位置 | 建议 |
|---|---|---|
| UI Prefab 根目录 | 多个 Editor 工具使用 `Assets/BundleAssets/UI` | 适合加入；Atlas 引用、文本提取和 UI 工具可共享 |
| UI 原始资源根目录 | PSD2UGUI、图集工具使用 `Assets/BundleAssets/UIRaw` | 适合加入；再由工具派生 Atlas、Raw 子目录 |
| 字体资源根目录 | UIText 编辑器使用 `Assets/BundleAssets/Fonts` | 适合加入；手动创建默认字体已由 UI 项目配置管理，PSD 字体映射仍留在 PSD2UGUI 配置 |
| 场景资源根目录 | 工具栏和场景切换器使用 `Assets/BundleAssets/Scenes` | 适合加入；用于统一编辑器场景发现范围 |
| 热更代码根目录 | UI、模块和生成代码都位于 `Assets/Scripts/HotFix` | 可加入根路径；具体程序集子目录仍由各生成器明确配置 |
| 热更资源根目录 | 项目约定为 `Assets/BundleAssets` | 可加入只读根约定，供 Editor 工具校验输出路径 |

共享根路径应在至少两个工具实际消费时再接入，避免只把常量搬家而没有减少重复。

## 应继续保留在领域配置

| 领域 | 现有配置 | 原因 |
|---|---|---|
| UI 生成规则 | `UIScriptGeneratorSettings.asset` | 前缀顺序、UI 类型和代码风格只属于 UI 生成器 |
| HybridCLR 与资源更新 | `UpdateSettings.asset` | 同时被 AOT 运行时和构建工具消费，不应依赖 Editor 总配置 |
| 启动流程 | `ProcedureSettings.asset` | 属于运行时状态机配置 |
| PSD 导入 | `PSD2UGUISettings.asset` | 包含图层、字体和搜索策略等工具专属参数 |
| 图集生成 | `AtlasConfig` | 包含 source、exclude 和单图集策略，领域含义明确 |
| 帧动画导入 | `FrameImportConfig` | 输入目录与生成目录由同一导入工具闭环消费 |
| 红点代码输出 | `RedDotTreeConfig` | 输出与具体红点树资产绑定，属于资产级配置 |

## 不应进入项目配置

- 单个 Prefab 的类名、UI 类型、Widget 类型和 Binding Manifest。
- 用户本机窗口状态、最近选择目录和其他个人偏好。
- Luban 表数据、运行时业务开关和服务器下发数据。
- 可以从根目录稳定派生、且没有独立业务含义的重复子路径。

## 配置约束

1. Unity 资产路径统一使用 `Assets/...` 和正斜杠。
2. 必填路径为空时生成与校验应明确失败，不使用静默默认值。
3. 生成代码和手写代码必须使用不同目录，避免生成器覆盖业务实现。
4. 配置资产纳入版本控制；个人偏好使用 `UserSettings`，不写入项目资产。
5. 新增字段前先确认至少一个明确消费者，并同步更新对应 Project Settings 页面。
