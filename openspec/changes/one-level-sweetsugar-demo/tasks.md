## 1. Baseline Verification

- [ ] 1.1 Open `GameUnity/Assets/SweetSugar/Scenes/game.unity` in Unity 2021.3.30f1c1 and confirm the original SweetSugar scene can enter `Level_1`.
- [ ] 1.2 Verify `Resources/Levels/Level_1.asset` and `Resources/Levels/Targets/TargetLevel1.asset` load through the existing SweetSugar flow without blocking Console errors.
- [ ] 1.3 Record any original-scene runtime blockers before changing `GameUnity/Assets/Scenes/Demo.unity`.

## 2. Demo Scene Setup

- [x] 2.1 Copy or save `GameUnity/Assets/SweetSugar/Scenes/game.unity` as `GameUnity/Assets/Scenes/Demo.unity`, preserving scene references and the original SweetSugar scene as a baseline.
- [x] 2.2 Add `GameUnity/Assets/Scenes/Demo.unity` to `GameUnity/ProjectSettings/EditorBuildSettings.asset` as the explicit Demo test scene.
- [ ] 2.3 Confirm Play mode from `Demo.unity` starts the copied SweetSugar scene structure without missing serialized references.

## 3. Fixed Level 1 Entry

- [x] 3.1 Add or configure a minimal Demo bootstrap path that explicitly sets the SweetSugar entry level to `Level_1`.
- [x] 3.2 Ensure the Demo start path overrides stale local `PlayerPrefs.OpenLevel` values that point to other levels.
- [ ] 3.3 Confirm starting Play mode from `Demo.unity` enters the match-3 board directly without clicking a map node.

## 4. Demo Flow Isolation

- [x] 4.1 Disable or disconnect Demo scene paths that navigate to SweetSugar map scenes such as `main.unity` or `gameStatic.unity`.
- [x] 4.2 Ensure settlement actions such as restart, next, home, or map do not advance to another level or enter map scenes during Demo validation.
- [x] 4.3 Hide, disable, or disconnect Demo-path UI entries for ads, IAP, social or network sync, leaderboard, GDPR, daily reward, shop, and bonus spin systems.

## 5. Gameplay Validation

- [ ] 5.1 Validate a legal swap in `Level_1` removes matched items and refills the board through existing SweetSugar gameplay logic.
- [ ] 5.2 Validate target counters update when matching the required first-level targets.
- [ ] 5.3 Validate at least one SweetSugar-defined special candy creation path still works in the Demo scene.
- [ ] 5.4 Validate win or fail settlement UI appears when `Level_1` reaches a terminal state.

## 6. Final Acceptance

- [x] 6.1 Confirm the Demo keeps SweetSugar `Resources` loading and does not require YooAsset, `BundleAssets`, or `GameModule.ResourceModule` migration for this change.
- [ ] 6.2 Confirm Unity Console has no blocking runtime errors that prevent first-level interaction or settlement.
- [ ] 6.3 Confirm `GameUnity/Assets/Scenes/Demo.unity` is ready as the single first-level SweetSugar Demo entry for this change.
