## Why

DGame 的 UI 自动绑定把 `List<Component>` 的可变位置同时当作 Prefab 数据和生成代码的运行时 ABI。组件重排、漏绑或两次生成不同步时，代码仍可能编译，却在运行时取到错误引用或产生后续索引错位；`m_item` 前缀还同时承担 Widget 边界，使 Widget 根节点无法自然绑定自身多个组件。

当前工作区已经有 `UIBindComponent` 自动生成路径及试验生成物，正适合在继续扩展使用范围前将绑定事实、生成流程和错误暴露方式收口，避免旧索引模型成为长期兼容负担。

## What Changes

- 新增以稳定 `BindingEntry` 为最小单位的序列化绑定清单：一个字段、预期组件类型、目标引用、稳定 ID 和可选的 UnityEvent 生成策略构成一条绑定事实；同一 GameObject 可以拥有多个条目。
- **BREAKING** 将生成代码的运行时引用从 `GetComponent<T>(int index)` 迁移为按稳定绑定 ID 的必需查找；绑定不存在或类型不匹配时明确中止 UI 创建，不再记录错误后返回空引用继续执行。
- 将 `UIBindComponent` Inspector 从可随意重排的组件数组改为受校验的绑定清单编辑器，并提供基于现有节点前缀的绑定建议，而非让前缀决定唯一绑定事实。
- 将 Widget 边界显式化为绑定容器配置；保留 `m_item` 仅作为迁移期默认建议，不再把它作为唯一的扫描停止条件。
- 收口为“校验并生成”主入口：同一份清单驱动 Prefab 序列化数据和 `*_Gen.g.cs`，所有校验通过后才写入产物，并保存可检测陈旧状态的生成签名。
- 保持 `UIWindow`、`UIWidget` 的既有生命周期、业务 partial 文件和 `AddUIEvent` 职责不变；生成器只处理引用、局部 UnityEvent 桥接和明确的待实现回调。
- 提供从既有 `m_components` 数组导入绑定清单、按批次迁移 Prefab、验证后删除旧索引路径的迁移计划。

## Capabilities

### New Capabilities
- `stable-ui-binding-manifest`: 为 UI Prefab 持久化、编辑、校验和运行时解析稳定的强类型绑定条目。
- `atomic-ui-binding-generation`: 从同一绑定清单校验并生成 Prefab 数据、绑定访问代码、事件桥接和陈旧状态签名。
- `explicit-ui-widget-boundaries`: 显式表达 Widget 根与父子扫描边界，同时允许 Widget 根节点绑定自身多个组件。

### Modified Capabilities

- 无现有 OpenSpec capability；本变更新增 UI 自动绑定的规范基线。

## Impact

- 运行时：`GameUnity/Assets/Scripts/HotFix/GameLogic/Module/UIModule/AutoBindComponent/UIBindComponent.cs` 及其编辑器 partial；生成 UI 的 `ScriptGenerator()` 访问模式。
- 编辑器：`GameUnity/Assets/Editor/UIScriptGenerator/UIScriptAutoGenerator.cs`、`UIComponentInspectorEditor.cs`、`UIScriptGeneratorSettings.cs` 和相关设置面板。
- 生成代码：`GameUnity/Assets/Scripts/HotFix/GameLogic/UI/Gen/*_Gen.g.cs`；手写 UI partial 不由生成器覆盖，但需按编译错误或迁移报告完成必要调整。
- 资源与程序集：不改动 UI Prefab 的资源位置、YooAsset 加载方式、`DGame.Runtime` 或 AOT 启动链路；新运行时绑定 API 仍位于 `GameLogic` 热更程序集，编辑器逻辑仍位于 `DGame.Editor`。
- 验证：需要 Unity/AIBridge 编译、Prefab 生成回归、Window/Widget 生命周期与事件行为的编辑器和 Play Mode 证据。
