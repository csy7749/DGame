## ADDED Requirements

### Requirement: Demo scene launches Level 1 directly
The Demo flow SHALL allow `GameUnity/Assets/Scenes/Demo.unity` to enter SweetSugar `Level_1` directly without requiring map scene interaction.

#### Scenario: Enter Demo scene from Unity editor
- **WHEN** `Assets/Scenes/Demo.unity` is opened and Play mode starts
- **THEN** the scene loads SweetSugar level data for `Level_1`
- **AND** the player can interact with the match-3 board without clicking a map node

#### Scenario: Existing local progress points to another level
- **WHEN** local `PlayerPrefs` contains an `OpenLevel` value other than `1`
- **THEN** the Demo flow still starts `Level_1`
- **AND** stale map progress does not change the Demo entry level

### Requirement: Demo scene preserves the playable match-3 loop
The Demo scene SHALL preserve SweetSugar's core first-level gameplay loop, including board generation, item swapping, matching, falling/refill, special candy creation, target tracking, and win/lose settlement.

#### Scenario: Player makes a valid match
- **WHEN** the player swaps items to create a valid match in `Level_1`
- **THEN** matched items are removed
- **AND** the board refills through the existing SweetSugar gameplay logic

#### Scenario: Player creates a special candy
- **WHEN** the player creates a match pattern that SweetSugar defines as a special candy pattern
- **THEN** the corresponding special candy is generated through existing SweetSugar rules

#### Scenario: Level reaches a terminal state
- **WHEN** the player completes the level target or exhausts the level limit
- **THEN** the Demo flow shows the existing win or fail settlement UI

### Requirement: Demo flow excludes map and multi-level progression
The Demo flow SHALL NOT require or enter SweetSugar map, static map, or multi-level progression scenes during first-level testing.

#### Scenario: Demo starts
- **WHEN** Play mode starts from `Demo.unity`
- **THEN** the flow does not require `main.unity` or `gameStatic.unity`

#### Scenario: Settlement action is selected
- **WHEN** the player uses a settlement action such as restart, next, home, or map
- **THEN** the flow does not advance to another level
- **AND** the flow does not navigate to SweetSugar map scenes

### Requirement: Demo flow excludes external monetization and network systems
The Demo flow SHALL NOT trigger ads, in-app purchases, social login, network sync, leaderboard, GDPR, daily reward, shop, or bonus spin systems while validating the first level.

#### Scenario: Player opens and completes the Demo
- **WHEN** the player starts and finishes a first-level Demo session
- **THEN** no advertisement, purchase, social login, network sync, leaderboard, GDPR, daily reward, shop, or bonus spin UI is shown as part of the required flow

#### Scenario: Scene contains legacy SweetSugar UI elements
- **WHEN** legacy SweetSugar UI elements are present in the copied scene hierarchy
- **THEN** elements unrelated to the first-level gameplay loop are hidden, disabled, or disconnected from the Demo path

### Requirement: Demo keeps SweetSugar Resources loading for this phase
The implementation SHALL keep SweetSugar's existing `Resources`-based loading path for first-level Demo validation and SHALL NOT require YooAsset or DGame `GameModule.ResourceModule` migration in this change.

#### Scenario: Level 1 data loads
- **WHEN** the Demo loads level data
- **THEN** it uses the existing SweetSugar `Resources/Levels/Level_1.asset` and `Resources/Levels/Targets/TargetLevel1.asset` assets

#### Scenario: DGame resource migration is evaluated
- **WHEN** the first-level Demo is not yet validated as playable
- **THEN** implementation does not migrate SweetSugar gameplay assets to `BundleAssets`
- **AND** implementation does not replace SweetSugar gameplay loads with `GameModule.ResourceModule`

### Requirement: Demo has explicit Unity validation criteria
The change SHALL define and satisfy an editor validation checklist for the first-level Demo before implementation is considered complete.

#### Scenario: Manual validation is performed
- **WHEN** Unity 2021.3.30f1c1 runs `Assets/Scenes/Demo.unity`
- **THEN** validation confirms direct entry to `Level_1`, board interaction, matching, refill, target counter updates, special candy behavior, and win or lose settlement

#### Scenario: Blocking runtime errors appear
- **WHEN** the Unity Console shows errors that prevent first-level interaction or settlement
- **THEN** the Demo is not considered complete until those errors are resolved at the source
