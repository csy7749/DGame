# SweetSugar Demo 接入计划

## 目标
- 在 `GameUnity/Assets/Scenes/Demo.unity` 中进入 Play 后，直接启动 SweetSugar 的单关玩法。
- 当前目标只要求 `Level_1` 可进入并具备基础可玩性，不处理地图、多关卡、广告、内购、社交。

## 阶段
- [x] 确认 `Demo.unity` 当前只有默认相机。
- [x] 确认 SweetSugar 单关入口依赖 `PlayerPrefs.OpenLevel` 与 `PlayerPrefs.OpenLevelTest`。
- [ ] 为 Demo 场景添加最小启动器并接入 `game.unity`。
- [ ] 校验场景/脚本改动是否自洽，并整理仍需用户在 Unity 里完成的动作。

## 关键决定
- 首轮不拆 SweetSugar 玩法代码，不迁移到 DGame 资源系统。
- 先复用 `Assets/SweetSugar/Scenes/game.unity` 的现有运行链。
- `Demo.unity` 只作为一个干净入口场景，负责设置单关启动参数并切到 SweetSugar 玩法场景。

## 风险
- `game.unity` 依赖大量 `Resources` 和场景内引用，不能靠代码临时拼装完整玩法对象。
- 直接从 `Demo.unity` 跳转到 `game.unity` 可以最大限度复用现有引用链，但这意味着首版并不是“所有内容都在 Demo 场景层级里”。
