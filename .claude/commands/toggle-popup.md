---
description: 开启或关闭弹窗提醒 hook（不带参数=全部，或指定 stop/permission）
allowed-tools: Read, Write, Edit
argument-hint: "[stop|permission] (留空=全部)"
---

切换 `.claude/settings.json` 中弹窗提醒 hook 的启用状态。

参数 `$ARGUMENTS` 决定切换范围（大小写不敏感）：

- **留空** → 目标为两个 hook 全部：`Stop`、`PermissionRequest`。
- **`stop`**（或 `s`）→ 仅 `Stop`。
- **`permission`**（或 `p` / `permissionrequest`）→ 仅 `PermissionRequest`。
- 其他无法识别的参数 → 提示用户有效取值，不修改文件。

执行步骤：

1. 解析 `$ARGUMENTS`，确定本次要切换的目标 hook 集合（按上面的规则）。
2. 读取 `.claude/settings.json`。
3. 对目标集合判断当前状态并切换（多个目标时作为整体：只要其中任意一个存在就视为「已开启」，执行关闭；全部不存在才视为「已关闭」，执行开启）：
   - **已开启 → 关闭**：从 `hooks` 中删除目标集合里的对应键，告知用户已关闭了哪些 hook。
   - **已关闭 → 开启**：在 `hooks` 下为目标集合中的每个 hook 添加下面对应的固定配置，告知用户已开启了哪些 hook。
4. 写回时保持其余内容（不在目标集合内的 hook、缩进风格）不变。

各 hook 的固定配置：

```json
"Stop": [
  {
    "matcher": "",
    "hooks": [
      { "type": "command", "command": "python \"$CLAUDE_PROJECT_DIR/.claude/hooks/claude_popup.py\"" }
    ]
  }
],
"PermissionRequest": [
  {
    "matcher": "",
    "hooks": [
      { "type": "command", "command": "python \"$CLAUDE_PROJECT_DIR/.claude/hooks/claude_popup.py\"" }
    ]
  }
]
```

注意：只切换目标集合内的 hook，不要影响 `hooks` 下的其他类型 hook。
