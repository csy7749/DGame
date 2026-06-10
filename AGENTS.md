# AGENTS.md

请使用中文写提案和回答
这个文件为 Codex 提供指导，用于处理此代码库中的代码。

DGame 基于 TEngine 二次封装，使用 HybridCLR + YooAsset + UniTask + Luban 构建。Unity 版本 2021.3.30f1c1，主启动场景位于 `GameUnity/Assets/Scenes/GameStart`。

---

## ⚡ 强制工作流（所有任务必须遵守）

> **禁止跳过** — 无论任务大小，必须按此顺序执行：

### 第零步：判断任务等级

在执行任何操作前，先判断任务等级：

| 等级 | 判断标准 | 知识查询策略 |
|------|---------|-------------|
| **L1 简单** | typo 修正、注释修改、日志输出、单行变量改名（**前提：不涉及框架 API 名称、UI 节点前缀、事件定义或资源路径**） | ❌ 跳过查询，直接编码 |
| **L2 调用** | 调用已知 API、单一模块的局部修改 | ✅ 触发 `dgame-dev` skill（只查该主题） |
| **L3 功能** | 新功能开发、跨文件修改、新增 UI/资源/事件逻辑 | ✅ 触发 `dgame-dev` skill（全量相关主题） |
| **L4 架构** | 模块设计、系统重构、多模块协作、架构决策 | ✅ 触发 `dgame-dev` skill（并行多主题） |

> **判断原则**：宁可高估等级，不可低估——不确定时上调一级。

---

### 第一步：按等级获取规范（使用 dgame-dev skill）

**L1 任务直接跳到第二步。L2-L4 必须先触发 `dgame-dev` skill。**

**知识源**：`.codex/skills/dgame-dev/references/`（AI 专用精炼文档，唯一权威来源；完整原文见同目录 `originals/`）

#### 调用方式

```
使用 $dgame-dev 在 DGame 仓库中实现或修改功能。
描述需要查询的技术问题或功能点
```

#### 会话内缓存（避免重复查询）

同一会话中已查询过的主题无需重复触发 skill：
- 直接引用本次会话已获取的规范摘要
- 仅当任务涉及**本次会话未覆盖的新主题**时才重新触发

#### 触发时机

| 场景 | 必须查询主题 |
|------|------------|
| 文件落位 / 改哪个程序集 | project-map.md — 目录职责、落位规则 |
| 架构 / 启动 / 分层 | client-architecture-codex.md — 分层、启动链路、HotFix 与 Runtime 边界 |
| UI 开发 | client-ui-development-codex.md — UIWindow/UIWidget、UIModule、循环列表、子页面 |
| 资源加载 | client-resource-management-codex.md — GameModule.ResourceModule、加载与释放 |
| 热更代码 | client-hotfix-development-codex.md — 程序集划分、GameStart、Procedure、HybridCLR、AOT |
| 热更资源包 | client-hotpatch-development-codex.md — 资源版本更新、下载器、缓存、YooAsset |
| 模块使用 | client-modules-codex.md — GameModule.XXX、模块获取与依赖 |
| 事件系统 | client-event-system-codex.md — GameEventDriver、EEventGroup、UI 事件监听 |
| 红点系统 | client-reddot-development-codex.md — RedDotModule、RedDotItem、RedDotPathDefine |
| Luban 配置 | luban-game-config-codex.md — 配置表生成流程、访问方式 |
| 代码规范 | client-conventions-codex.md — 命名约定、节点前缀、异步、日志、Git 协作 |

---

### 第二步：输出代码/方案

基于 dgame-dev skill 返回的规范编写实现。

**当 references 规范与代码实际 API 冲突时**：
1. 搜索实际方法签名验证（例：搜索 `GameModule.ResourceModule` 确认 API）
2. 优先信任代码中的实际实现
3. 在输出中标注冲突点，供后续修正

---

## 核心原则（编码红线）

1. **分层落位**：框架运行时 → `GameUnity/Assets/DGame/Runtime`；编辑器工具 → `GameUnity/Assets/DGame/Editor`；热更玩法 → `GameUnity/Assets/Scripts/HotFix/GameLogic`；HotFix 基础 → `GameUnity/Assets/Scripts/HotFix/GameBase`
2. **优先复用 TEngine 二次封装**：新增服务/系统前，先确认 DGame 是否已对原能力做封装或替换（如 `GameTimer`、`ILocalizationModule`、`MemoryCollector`、`InputModule`、`AnimModule`、对象池与事件封装），不要绕过封装层
3. **模块访问**：通过 `GameModule.XXX` 访问，而非 `ModuleSystem.GetModule<T>()`
4. **异步优先**：IO 操作用 `UniTask`，禁止同步加载 / Coroutine
5. **资源必须释放**：资源加载与卸载成对出现，遵循 `GameModule.ResourceModule` 的生命周期约定
6. **事件解耦**：模块间用 `GameEventDriver` / 接口事件，UI 内部用 UI 事件监听，遵循 `EEventGroup` 定义规范

---

## 📚 References 参考文档

> **AI 唯一权威来源：`.codex/skills/dgame-dev/references/`**（精简版供查询，`originals/` 存完整原文）

| 文档 | 内容 | 层级 |
|-----|------|------|
| project-map.md | 仓库目录职责 / 文件落位规则 | 核心 |
| client-architecture-codex.md | 项目结构 / 分层 / 启动链路 / 程序集边界 | 核心 |
| client-modules-codex.md | 模块访问规则（GameModule.XXX）| 核心 |
| client-ui-development-codex.md | UI 开发（UIWindow/UIWidget/UIModule/子页面）| 核心 |
| client-event-system-codex.md | 事件系统（GameEventDriver / EEventGroup）| 核心 |
| client-resource-management-codex.md | 资源加载 / 卸载 / 寻址 | 核心 |
| client-hotfix-development-codex.md | 热更代码（HybridCLR / 程序集划分 / Procedure）| 核心 |
| client-hotpatch-development-codex.md | 热更资源包（版本更新 / 下载器 / YooAsset）| 核心 |
| luban-game-config-codex.md | 配置表（Luban / Excel / 生成链路）| 核心 |
| client-conventions-codex.md | 代码规范 / 命名约定 / 节点前缀 / Git 协作 | 核心 |
| client-reddot-development-codex.md | 红点系统（RedDotModule / RedDotPathDefine）| 进阶 |

---

# 通用编码准则

Behavioral guidelines to reduce common LLM coding mistakes. Merge with project-specific instructions as needed.

**Tradeoff:** These guidelines bias toward caution over speed. For trivial tasks, use judgment.

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

## 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

---

**These guidelines are working if:** fewer unnecessary changes in diffs, fewer rewrites due to overcomplication, and clarifying questions come before implementation rather than after mistakes.
