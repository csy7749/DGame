## Why

当前 Manifest 生成器把自动生成代码和业务代码放在同一个 `partial` 类型中，并在生成前通过读取业务 `.cs` 文件确认事件处理器存在。这使得一个尚未创建业务实现类的 Widget 即使绑定数据完整，也无法先生成可编译的自动代码。

## What Changes

- **BREAKING** 将 Manifest 自动生成类型与业务 UI 类型拆分为自动生成基类和业务派生类。
- 自动生成基类负责绑定字段、生命周期接入和显式 UnityEvent 注册。
- 自动绑定字段使用 `protected`，业务派生类可以直接操作已绑定组件。
- 业务派生类负责覆写事件处理器和业务生命周期逻辑，不再依赖生成前已有的 partial 实现文件。
- 生成器根据 UI 类型生成稳定的自动基类名，并让业务类型继承该基类。
- 自动代码生成时同步创建业务逻辑类骨架，并按 Window 与 Item 规则统一落位。
- 事件校验从“生成前扫描业务源文件”调整为“验证生成模型中的事件签名与继承契约”。
- 保留显式事件配置；未勾选生成 UnityEvent 的组件不生成监听，也不要求业务处理器。
- 显式事件支持 Button、Toggle、Slider 与 Dropdown，Dropdown 使用 `int` 参数回调。
- 为已有 partial 生成物提供明确的迁移兼容策略，避免自动覆盖手写业务逻辑。

## Capabilities

### New Capabilities

- `inheritance-ui-code-generation`: 定义 UI Manifest 自动基类、业务派生类和事件覆写契约。

### Modified Capabilities

- 无

## Impact

- 编辑器生成器：`GameUnity/Assets/Editor/UIScriptGenerator/UIBindingManifestGenerator.cs`、校验器及 Inspector 生成设置。
- 生成代码：`GameUnity/Assets/Scripts/HotFix/GameLogic/UI/Gen/*_Gen.g.cs` 的类型声明和事件处理入口。
- 业务代码：现有 UI partial 类需要迁移为继承自动基类的派生类。
- 目录结构：Window 逻辑位于 `UI/<WindowName>/`，其他 Item/Widget 逻辑统一位于 `UI/Item/`。
- Prefab：不改变绑定清单数据格式、Widget 边界和资源地址。
- 验证：需要覆盖没有业务类、存在业务覆写、事件签名不匹配和旧 partial 迁移场景。
