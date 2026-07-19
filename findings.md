# 发现记录

## SweetSugar 运行链
- `LevelManager.Start()` 会先把 `gameStatus` 设为 `Map`。
- 当 `PlayerPrefs.OpenLevelTest > 0` 且场景里没有 `RestartLevel` 时，`GameState.Map` 分支会直接切到 `PrepareGame`。
- `PrepareGame()` 内部会调用 `EnableMap(false)` 和 `LoadLevel()`，后者默认读取 `PlayerPrefs.OpenLevel`，未设置时回落到 `1`。

## Demo 场景现状
- `GameUnity/Assets/Scenes/Demo.unity` 当前只有默认 `Main Camera`。
- 它还没有任何 SweetSugar 运行对象，不能靠当前场景直接跑玩法。

## 接入判断
- 最稳的首版方式是：`Demo.unity` 作为启动入口，设置 `OpenLevel=1`、`OpenLevelTest=1`，再切到 `Assets/SweetSugar/Scenes/game.unity`。
- `game.unity` 已在 `EditorBuildSettings.asset` 中启用，不需要额外补入 Build Settings。

## 当前限制
- 如果用户的诉求是“SweetSugar 全部对象真的都实例在 Demo 场景层级里”，那不是最小改法，需要做场景拷贝或 Prefab 化整理，成本会明显更高。
