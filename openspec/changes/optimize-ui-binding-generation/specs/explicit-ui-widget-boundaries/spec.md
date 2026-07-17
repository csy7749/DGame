## ADDED Requirements

### Requirement: Widget roots are explicit binding boundaries
The system SHALL persist `IsWidgetRoot` on the Widget root binding container and SHALL use it to define parent-child scanning boundaries. A parent UI SHALL bind a Widget root through a dedicated Widget entry and SHALL NOT generate bindings for that Widget's internal nodes.

#### Scenario: Parent scans a nested Widget
- **WHEN** a Window scan reaches a descendant marked `IsWidgetRoot`
- **THEN** it emits only the configured Widget root binding for the parent and does not traverse the Widget internals

### Requirement: Widget roots can bind their own components
The generator SHALL include the root node when generating the manifest for a Widget. Components on that root SHALL be eligible for separate approved binding entries, independent of the Widget root relationship.

#### Scenario: Clickable Widget root
- **WHEN** an Item root is marked `IsWidgetRoot` and has Button, Image, and CanvasGroup components
- **THEN** the Item manifest can bind all three components while its parent retains only the Widget root entry

#### Scenario: Select a nested Widget root in the Inspector
- **WHEN** a nested Widget root is selected in Prefab Stage
- **THEN** the Inspector shows the parent UI's independent Widget `+/-` action and shows the root components as independent `+/-` actions owned by the Widget manifest

### Requirement: Legacy item prefix is migration advice only
The `m_item` prefix SHALL be treated as an editor migration suggestion for creating a Widget entry and marking its root. It SHALL NOT be the sole source of Widget boundary or scanning semantics after the manifest model is enabled.

#### Scenario: Widget root without legacy prefix
- **WHEN** a UI node is explicitly marked `IsWidgetRoot` but does not start with `m_item`
- **THEN** generation honors the explicit Widget boundary

#### Scenario: Legacy item prefix before migration confirmation
- **WHEN** an existing node starts with `m_item` and has not been converted to a manifest
- **THEN** the migration tool proposes a Widget boundary and entry for developer review without silently changing its Prefab
