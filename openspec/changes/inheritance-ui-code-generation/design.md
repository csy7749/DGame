## Context

当前 UI Manifest 生成器生成 `partial class Xxx : UIWindow/UIWidget`，事件监听直接引用业务 partial 中的方法。校验器通过读取 `ImplementationCodePath/Xxx.cs` 判断处理器是否存在，因此无法在业务类尚未创建时先生成自动绑定代码。

本次改造需要保持 `UIWindow`、`UIWidget` 生命周期、Manifest 数据格式、Widget 父子边界和 YooAsset 资源地址不变，仅替换自动代码与业务代码的类型组织方式。

## Goals / Non-Goals

**Goals:**

- 生成独立的自动基类，自动基类可以在没有业务派生类时完成代码生成。
- 业务 UI 类型通过继承自动基类覆写事件和生命周期扩展点。
- 生成器能够为 Window、普通 Widget 和泛型 Widget 保持现有基类参数。
- 事件处理器签名由生成基类和 Manifest 共同决定，缺失业务覆写不会阻断自动代码生成。
- 自动生成基类时同步创建不存在的业务逻辑类骨架。
- Window 与 Item 使用稳定、可预测的业务代码目录。
- 让已有 partial 生成物可以通过显式迁移步骤切换到继承结构。

**Non-Goals:**

- 不改变 Manifest 条目、BindingId、Widget 边界或运行时 `GetRequired` 解析协议。
- 不自动生成业务行为或吞掉未实现点击逻辑。
- 不在本次变更中批量迁移所有已有 UI Prefab。

## Decisions

### 1. 自动类命名与业务类命名

自动生成文件继续位于 `UI/Gen`，生成 `XxxAuto_Gen.g.cs` 和类型 `XxxAuto`；业务类型保留 `Xxx`，由业务代码声明 `class Xxx : XxxAuto`。这样生成器不会覆盖业务文件，也不会与现有业务类型同名。

替代方案：生成 `XxxBase`。不采用，因为 `Auto` 能直接表达文件由工具维护，且与现有 `*_Gen.g.cs` 命名一致。

### 2. 事件处理入口

自动基类提供 `protected virtual` 事件方法，并在自身 `ScriptGenerator` 中注册监听。默认实现只提供可覆写入口，不生成业务行为；业务派生类需要行为时覆写对应方法。校验器只验证事件种类、目标组件和生成方法签名，不读取业务源文件判断是否存在方法。

自动基类中的绑定组件字段使用 `protected`，使业务派生类能在生命周期方法中设置组件数据。BindingId 常量和 Manifest 组件引用继续使用 `private`，不向业务层暴露机械绑定细节。Dropdown 的 `onValueChanged` 与 Toggle、Slider 一样由自动基类注册，扩展点签名为 `protected virtual void Handler(int value)`。

替代方案：继续使用 `abstract` 方法。该方案会把生成阶段问题转成编译阶段错误，仍无法支持没有业务类的 UI，故不采用。

### 3. 生命周期归属

自动基类保留生成绑定和必要的生命周期调用；业务派生类覆写公开扩展点。生成器不得把业务 `OnCreate`、`OnRefresh` 等方法实现写进自动类，避免自动代码覆盖业务逻辑。

### 4. 迁移策略

迁移工具或明确的手工步骤负责将现有 `partial class Xxx : ...` 改为 `class Xxx : XxxAuto`，并把业务方法从 partial 实现改为 override。生成器发现同名旧 partial 业务文件时必须给出迁移诊断，不得静默覆盖或删除。

### 5. 业务逻辑类目录

自动基类仍生成到 `UI/Gen`。业务逻辑类按 UI 类型落位：

- `UIWindow`：`UI/<WindowName>/<WindowName>.cs`。
- 其他 Widget、Item：`UI/Item/<TypeName>.cs`。

生成器只创建不存在的业务类。发现同名业务文件位于其他目录时必须终止并报告迁移路径，不自动移动、不覆盖，也不生成第二个同名类型。

## Risks / Trade-offs

- [类型名变化] 生成代码和业务代码的基类关系改变 → 先只对新生成 UI 启用，旧 partial 结构保留迁移兼容检查。
- [默认 virtual 方法无业务行为] 未覆写的点击不会产生效果 → Inspector 显式展示事件入口，且不把默认实现伪装成业务成功。
- [泛型 Widget 类型解析] 自动基类的泛型参数可能与业务派生类不一致 → 复用现有 `GetUITypeName` 结果，并增加生成模型测试。
- [Prefab 运行时类型推导] `CreateWidget<T>` 需要业务类型而不是自动类型 → 生成 Widget 条目的字段类型继续使用业务类名，自动类只作为业务类基类。

## Migration Plan

1. 新生成路径先输出自动基类和迁移诊断，不触碰已有业务 partial 文件。
2. 对单个 Window、Widget 和泛型 Widget 完成生成、编译和 Play Mode 验证。
3. 将已有业务 partial 类逐个改为继承 `XxxAuto`，并将事件方法改为 `override`。
4. 全部迁移完成后，再移除旧 partial 处理器校验和旧生成物兼容分支。
