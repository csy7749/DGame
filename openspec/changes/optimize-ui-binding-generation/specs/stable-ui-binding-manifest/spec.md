## ADDED Requirements

### Requirement: Stable binding entries represent every generated UI reference
The system SHALL persist each generated UI reference as one `BindingEntry` containing an immutable binding ID, unique field name, expected component type name, target Component reference, binding kind, and optional event metadata. A single GameObject SHALL be allowed to contribute multiple entries when it has multiple approved components.

#### Scenario: Bind multiple components on one node
- **WHEN** a UI node has a Button, Image, and CanvasGroup selected as bindings
- **THEN** the manifest contains three distinct entries with distinct IDs and field names that reference those components on the same node

#### Scenario: Reorder manifest display
- **WHEN** an editor reorders the displayed entries without changing IDs or values
- **THEN** generated code resolves the same fields to the same targets

### Requirement: Selected nodes expose independent ShiHua-style binding actions
In Prefab Stage, the GameObject Inspector SHALL enumerate the selected node's existing components, filter them through the configured binding whitelist, and display an independent `+ Type` and `- Type` action for each bindable object. The editor SHALL NOT provide a collect-all action, batch multi-select action, or additional GameObject menu for creating these entries.

#### Scenario: Add one of several components
- **WHEN** a selected node contains Button, Image, and CanvasGroup and the editor clicks `+ Button`
- **THEN** only the Button entry is added to the owning manifest and the Image and CanvasGroup remain unbound

#### Scenario: Remove one bound component
- **WHEN** Button and Image on the same node are already bound and the editor clicks `- Image`
- **THEN** only the Image entry is removed and the Button binding keeps its ID and field name

#### Scenario: Internal components are filtered
- **WHEN** the selected node contains RectTransform, CanvasRenderer, Button, and Image
- **THEN** the Inspector offers GameObject, Button, and Image bindings but does not expose Transform, RectTransform, CanvasRenderer, or UIBindComponent as component bindings

### Requirement: Manifest validation rejects invalid binding facts before generation
The editor SHALL reject generation when any entry has an empty or duplicate ID, invalid or duplicate field name, null target, target outside the owning Prefab, incompatible target type, duplicate generated event handler, or an invalid Widget ownership relationship. The diagnostic SHALL identify the Prefab, entry ID, field name, expected type, actual state, and repair action.

#### Scenario: Detect a stale target
- **WHEN** a bound component is removed from a Prefab
- **THEN** generation fails before modifying the Prefab or generated code and reports the entry and expected component type

#### Scenario: Detect duplicate field names
- **WHEN** two entries use the same field name
- **THEN** generation fails and reports both conflicting entry IDs

### Requirement: Runtime reads bindings by required stable ID
The runtime SHALL expose `GetRequired<T>(bindingId)` or an equivalent stable-ID API and generated UI code SHALL use it instead of a mutable component index. Missing, duplicate, null, or incompatible bindings SHALL throw a contextual exception that aborts UI creation.

#### Scenario: Runtime detects type mismatch
- **WHEN** a generated Image field resolves to an entry whose target is a Button
- **THEN** UI creation fails with an exception naming the binding ID, field, expected Image type, actual Button type, and owning UI object

#### Scenario: Runtime has a valid binding
- **WHEN** a generated UI field requests an existing entry with a compatible target
- **THEN** the API returns the serialized target without Transform-name lookup or component-array indexing

### Requirement: Generated state detects stale artifacts
The editor SHALL calculate a deterministic signature from the normalized manifest and generator version, persist it with the Prefab metadata and generated file, and mark the UI as stale when either signature differs.

#### Scenario: Prefab changes after generation
- **WHEN** an editor changes a manifest field or target after a successful generation
- **THEN** the Inspector marks the generated artifacts as stale and identifies that a full validation and generation is required
