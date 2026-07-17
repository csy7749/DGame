## 1. Runtime Binding Manifest

- [x] 1.1 在 `GameLogic/Module/UIModule/AutoBindComponent` 新增可序列化的 `UIBindingEntry`、`UIBindingKind` 和稳定 GUID 创建逻辑，限制每条目只表示一个字段事实。
- [x] 1.2 将 `UIBindComponent` 的运行时存储迁移为 manifest，并实现只读 ID 索引与 `GetRequired<T>(bindingId)`；为重复、缺失、空引用和类型不符抛出包含 UI 层级与条目上下文的异常。
- [x] 1.3 为 GameObject 和 Widget 条目定义以 `RectTransform` 为目标的生成访问约定，并为 `IsWidgetRoot`、Widget 类型和所有权关系提供运行时/序列化字段。
- [ ] 1.4 编写运行时单元或编辑器测试，覆盖同节点多组件、条目重排、缺失 ID、重复 ID、空目标和类型不匹配的失败行为。

## 2. Manifest Inspector And Validation

- [x] 2.1 将 `UIComponentInspectorEditor` 的可重排 `m_components` 列表替换为 `BindingEntry` Inspector，展示只读 ID、字段名、预期类型、目标、绑定种类、事件配置和同步状态。
- [ ] 2.2 实现完整 manifest 校验器，收集字段、ID、类型、Prefab 归属、事件、Widget 边界和生成路径错误，并在一次运行中输出全部可定位诊断。
- [x] 2.3 保留 `UIScriptGeneratorSettings` 中的前缀规则作为建议来源，在 Prefab Stage 的 GameObject Inspector 中按 ShiHua 模式为白名单组件分别提供 `+ 类型` / `- 类型` 与默认字段名，不提供批量收集或额外菜单入口。
- [ ] 2.4 实现基于规范化 manifest 与生成器版本的签名计算、持久化与 Inspector 陈旧提示，并为中断写入后的不一致状态添加验证测试。

## 3. Deterministic Code Generation

- [x] 3.1 将 `UIScriptAutoGenerator` 重构为从经校验的 manifest 构建统一生成模型，移除对 `m_bindIndex` 和节点前缀直接生成字段的依赖。
- [x] 3.2 生成每个 UI 类私有的稳定绑定 ID 常量及 `GetRequired<T>` 调用，保持 `ScriptGenerator`、`BindMemberProperty`、`RegisterEvent`、`OnCreate`、`OnRefresh` 生命周期顺序不变。
- [ ] 3.3 实现“校验并生成”单入口：先在内存生成全部代码与签名，校验成功后批量写入 Prefab/`*_Gen.g.cs`；写入失败时明确报错并保留陈旧状态。
- [x] 3.4 将旧的独立“重新绑定组件”和代码生成入口替换或明确标注为迁移前入口，确保新 UI 无法创建索引式生成产物。
- [ ] 3.5 为无变更重复生成、单个无效条目阻断写入、生成文件写入失败和签名不匹配添加编辑器回归测试。

## 4. Widget Boundaries And UnityEvent Contracts

- [x] 4.1 实现 `IsWidgetRoot` 的扫描规则：父 UI 只生成 Widget 根条目且停止下探，Widget 自身生成时包含其根节点并在下一个显式 Widget 根处停止。
- [x] 4.2 实现从 `m_item` 提示创建 Widget 条目和根标记的迁移建议，允许非 `m_item` 命名的显式 Widget 根，并验证父子所有权冲突会阻断生成。
- [x] 4.3 为 Button、Toggle、Slider 的显式事件条目实现业务 partial 处理器签名检查；处理器缺失时阻断生成，且不生成空 partial 回调或无效监听。
- [ ] 4.4 提供独立的待实现处理器骨架创建操作，骨架必须显式抛出 `NotImplementedException`，并为已有处理器、缺失处理器和签名不兼容分别添加编辑器测试。

## 5. Migration And Legacy Removal

- [ ] 5.1 实现仅编辑器可用的旧 `m_components` 导入器：依据现有生成代码和数组顺序生成待确认条目、分配 GUID，并输出无法可靠匹配的逐条迁移报告。
- [ ] 5.2 选取一个 Window、一个可点击 Widget 根、一个动态列表 Item 和一个同节点多组件 Prefab 作为首批样本，完成导入、校验、生成和人工 Inspector 复核。
- [ ] 5.3 按 UI 功能目录批量迁移目标 Prefab，记录每个 Prefab 的 manifest/代码签名、编译结果与未解决错误；迁移失败项不得自动切换新运行时路径。
- [ ] 5.4 全量迁移与回归完成后删除 `m_components`、`GetComponent<T>(int)`、索引式生成代码和旧 Inspector/菜单入口，并更新 UI 生成器说明与 DGame UI 规范。

## 6. Unity Verification

- [x] 6.1 使用 AIBridge 触发 Unity 刷新和编译，修复所有由新运行时 API、生成文件或程序集边界引发的编译错误，并采集 Error Console 日志。
- [ ] 6.2 在 Play Mode 验证首批 Window 与 Widget 的创建顺序、字段引用、局部 UnityEvent、父子 Widget 销毁和动态列表复用，确认没有依赖旧索引读取。
- [ ] 6.3 运行新增编辑器/运行时测试与现有相关测试，执行 `git diff --check`，并复核生成文件在连续两次无变更执行后不产生无意义 diff。
