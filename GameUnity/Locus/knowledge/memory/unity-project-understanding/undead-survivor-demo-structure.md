---
id: kd_773ff976-c938-42a9-8843-d90eeec57d50
type: memory
path: unity-project-understanding/undead-survivor-demo-structure.md
title: undead-survivor-demo-structure
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
inheritAiConfig: true
createdAt: 1783867047867
updatedAt: 1783867047868
---

# undead-survivor-demo-structure

## Summary
Undead Survivor Complete 是高度场景化的 2D 幸存者 Demo，核心逻辑集中在 GameManager、场景 Canvas、PoolManager 和 AudioManager；迁移时应保留资产并重建运行架构。

<!-- locus:body:start -->
- 主场景是 `Assets/Undead Survivor/Complete/CompleteScene.unity`，包含 `GameManager`、`PoolManager`、`AudioManager`、`AchiveManager`、`Player`、场景内 Canvas/UI、Cinemachine 相机和 Tilemap 地图。
- 原逻辑依赖多个全局单例和直接场景引用：`GameManager.instance` 汇总角色状态、经验、击杀、时间、暂停、开始/重试/结果逻辑。
- 游戏循环：选角 → 激活玩家 → 自动给予初始武器 → 移动与自动攻击 → 击杀得经验 → 暂停三选一升级 → 300 秒胜利或死亡失败。
- `PoolManager` 使用基于 Prefab 数组索引的简单对象池；索引协议当前是 Enemy / Bullet 0 / Bullet 1。
- `AudioManager` 自管 BGM/SFX 池和高通滤波；适合迁到 DGame 的 AudioModule。
- `Player` 使用新 Input System 的 `PlayerInput`，但部分武器逻辑仍混用旧输入接口；输入体系不统一。
- UI 全在场景 Canvas 下，含开始界面、HUD、升级、结果、解锁提示，不适合直接接入 DGame `UIWindow` 体系，应拆成独立窗口预制体。
- `ItemData` 目前承载主要武器/装备���据；角色、经验、波次等仍大量硬编码。适合未来迁到 Luban。
- 已确认迁移风险：`GameRetry()` 固定 `LoadScene(0)`、场景内重复 Camera/AudioListener/EventSystem、重度依赖数组顺序和层级索引、旧代码位于 `Assembly-CSharp` 不能直接接入现有 HybridCLR 业务链。
<!-- locus:body:end -->
