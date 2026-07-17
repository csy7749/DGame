## ADDED Requirements

### Requirement: One validated manifest drives all generated binding artifacts
The generator SHALL derive Prefab binding metadata, generated field access code, Widget creation code, UnityEvent bridge code, and generation signature from the same validated manifest. Node-prefix scanning SHALL only create editable suggestions and SHALL NOT directly generate final binding code.

#### Scenario: Generate a Window from its manifest
- **WHEN** a Window manifest passes validation
- **THEN** the generator writes deterministic `*_Gen.g.cs` bindings and persists the matching manifest signature to the Prefab

#### Scenario: A prefix suggestion is not confirmed
- **WHEN** a node name matches a configured prefix but no corresponding manifest entry is confirmed
- **THEN** the generator does not emit a field or runtime binding for that node

### Requirement: Generation validates before writing assets
The generator SHALL complete validation and produce all output content in memory before changing any Prefab or generated C# file. If validation fails, it SHALL not modify either artifact and SHALL present all validation errors in one run.

#### Scenario: One invalid entry among valid entries
- **WHEN** a manifest has valid bindings and one null target
- **THEN** the generator writes no generated file or Prefab update and reports the null target together with any other discovered errors

### Requirement: Write failures are explicit and leave detectable state
The generator SHALL report asset or file write failures as errors and SHALL NOT report generation success. If a failure occurs after any artifact is written, signature verification SHALL mark the affected UI stale until a subsequent full generation succeeds.

#### Scenario: Generated file write fails
- **WHEN** writing a generated C# file fails after validation
- **THEN** the generator reports the file path and failure, and the UI is not reported as synchronized

### Requirement: UnityEvent bridges require an implemented handler
The generator SHALL emit a UnityEvent listener only for entries that explicitly enable event generation and whose business partial class contains a compatible handler. A missing handler SHALL be a generation error; the generator SHALL NOT rely on an unimplemented partial method or an empty callback.

#### Scenario: Event handler is missing
- **WHEN** a Button entry requests a click bridge but the business partial lacks the configured handler
- **THEN** generation fails without adding the listener and identifies the expected handler signature

#### Scenario: Event handler exists
- **WHEN** a Toggle entry requests a value-changed bridge and the business partial has a compatible handler
- **THEN** generated code adds the listener to that handler during UI initialization

