---
id: kd_0630c196-06af-4e45-8300-fe58a0769810
type: memory
path: unity-project-understanding/dgame-runtime-architecture.md
title: dgame-runtime-architecture
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
inheritAiConfig: true
createdAt: 1783867047847
updatedAt: 1783867047848
---

# dgame-runtime-architecture

## Summary
DGame 项目采用 AOT 启动 + 热更业务分层；启动链、Procedure、YooAsset、UIRoot、模块系统和 Demo2 是后续迁移任务的主要入口。

<!-- locus:body:start -->
- 启动入口是 `Assets/Scenes/GameStart/GameStart.unity`，通过 `Assets/DGame.AOT/GameEntry.cs` 驱动 AOT 启动层。
- 运行分层为：`DGame.Runtime`（框架基础模块）→ `DGame.AOT`（启动/更新/加载热更）→ `GameProto`/`GameBattle`/`GameLogic`（热更业务）。
- Procedure 主链从 `LaunchProcedure` 开始，完成 YooAsset 包初始化、资源更新、预加载、热更程序集加载，最后反射调用 `GameStart.Entrance()`。
- `ModuleSystem` 采用接口/实现命名约定的懒创建方式，常用模块包括 Resource、Scene、Audio、FSM、GameObjectPool、Input。
- 常驻 UI 根是 `Assets/DGame/Runtime/Setting/Prefabs/UIRoot.prefab`，业务 UI 通过 `GameLogic` 的 `UIModule/UIWindow` 体系加载，默认以窗口类名作为资源地址。
- 资源系统基于 YooAsset，当前 Collector 大量使用 `AddressByFileName`；新增资源要注意同名地址冲突。
- 场景系统支持 YooAsset Additive 加载；局内内容更适合做成 Additive 战斗场景，而不是塞进启动场景。
- Demo2 是当前最接近标准业务实现的参考：展示了 Battle/Logic 分层、Additive 场景、UIWindow、事件、对象池和退出清理流程。
<!-- locus:body:end -->
