---
name: toggle-popup
description: Toggle DGame project Codex popup reminder hooks in .codex/config.toml. Use when the user asks to enable, disable, or switch Codex stop/subagent-stop popups, permission request popups, or asks for a Codex equivalent of .claude/commands/toggle-popup.md.
---

# Toggle Popup

用于切换当前 DGame 项目的 Codex 弹窗提醒 hooks。执行时不要手写 TOML，直接运行脚本：

```powershell
python ".codex/skills/toggle-popup/scripts/toggle_popup.py" [stop|permission]
```

## 参数

- 留空或 `all`：切换 `Stop`、`SubagentStop`、`PermissionRequest`。
- `stop` 或 `s`：切换 `Stop` 和 `SubagentStop`，因为项目完成提醒同时覆盖主任务和子任务。
- `permission`、`p` 或 `permissionrequest`：只切换 `PermissionRequest`。

## 行为

脚本读取 `.codex/config.toml`，只处理目标集合内的 hook：

- 目标集合中任意 hook 已存在时，视为已开启，执行关闭。
- 目标集合全部不存在时，视为已关闭，执行开启。
- 不修改 `hooks` 下其他 hook，也不改 MCP、模型、sandbox 等配置。

执行后把脚本输出原样总结给用户。
