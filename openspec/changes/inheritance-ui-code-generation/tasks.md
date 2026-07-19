## 1. Generation Model

- [x] 1.1 Add a deterministic automatic base type name and output path for Window, Widget, and generic Widget manifests.
- [x] 1.2 Update generated type declarations so automatic classes inherit the configured UIWindow/UIWidget contract without colliding with business type names.
- [x] 1.3 Preserve stable BindingId constants, typed fields, Widget creation, and existing lifecycle binding order in the automatic base.

## 2. Event Contract

- [x] 2.1 Generate protected virtual Button, Toggle, and Slider event entry points with the existing parameter signatures.
- [x] 2.2 Register generated listeners against the virtual entry points and remove source-file scanning from event validation.
- [x] 2.3 Ensure disabled UnityEvent entries generate no listener and require no handler.
- [x] 2.4 Add Dropdown `onValueChanged(int)` configuration, validation, generation, and Inspector display.

## 3. Business Type Integration

- [x] 3.1 Add generated business-type metadata or Inspector guidance describing the required `Xxx : XxxAuto` declaration.
- [x] 3.2 Detect old partial business classes and report an explicit migration diagnostic without overwriting them.
- [x] 3.3 Verify `CreateWidget<T>`, Window creation, generic Widget types, and generated field access use the business type contract correctly.
- [x] 3.4 Generate a missing business class together with automatic code.
- [x] 3.5 Route Window logic to `UI/<WindowName>` and all other Item/Widget logic to `UI/Item`.
- [x] 3.6 Reject legacy or duplicate business-file locations without overwriting or moving user code.
- [x] 3.7 Generate bound component fields as protected for business-derived class access.

## 4. Migration And Verification

- [x] 4.1 Migrate one Window and one Widget fixture to the inheritance output and keep existing business behavior intact.
- [ ] 4.2 Add editor/generator tests for missing business class, overridden event, unoverridden event, disabled event, and legacy partial conflict.
- [ ] 4.3 Run Unity compilation, Manifest generation, `git diff --check`, and repeated generation checks for the migrated fixtures.
