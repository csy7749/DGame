## ADDED Requirements

### Requirement: UI automatic code uses a separate generated base type
The generator SHALL emit an automatic base type whose name is distinct from the business UI type, and the generated type SHALL inherit the configured UIWindow or UIWidget base contract.

#### Scenario: Generate a Widget without a business class
- **WHEN** a valid Widget Manifest has no corresponding business `.cs` file
- **THEN** generation SHALL write the automatic base code successfully without reporting a missing business implementation class

#### Scenario: Business type extends generated base
- **WHEN** a business Widget class is declared for a generated Manifest
- **THEN** the business class SHALL be able to inherit the generated automatic base and retain the existing UIWidget lifecycle

### Requirement: Automatic generation creates the business logic skeleton
The generator SHALL create a missing business logic class together with the automatic base, and SHALL preserve an existing business file without overwriting it.

#### Scenario: Generate a Window without a business class
- **WHEN** a UIWindow Manifest generates automatic code and no matching business file exists
- **THEN** the generator SHALL create `UI/<WindowName>/<WindowName>.cs` inheriting `<WindowName>Auto`

#### Scenario: Generate an Item without a business class
- **WHEN** a non-UIWindow Manifest generates automatic code and no matching business file exists
- **THEN** the generator SHALL create `UI/Item/<TypeName>.cs` without creating a type-specific subfolder

#### Scenario: Business file is in a legacy directory
- **WHEN** a matching business class file exists outside its required directory
- **THEN** generation SHALL stop with a migration diagnostic and SHALL NOT move, overwrite, or duplicate the class

### Requirement: Generated bindings own only mechanical UI behavior
The generated base SHALL contain binding lookup, generated field assignment, and explicitly configured UnityEvent registration, but SHALL NOT generate domain behavior.

#### Scenario: Generate component bindings
- **WHEN** Manifest validation succeeds
- **THEN** the generated base SHALL use stable BindingId lookup, expose bound component fields as protected, and preserve the existing ScriptGenerator binding order

#### Scenario: UnityEvent is not configured
- **WHEN** a binding entry has `GenerateUnityEvent` disabled
- **THEN** the generated base SHALL not register a listener or require an event handler

### Requirement: Business event behavior uses overridable extension points
For an explicitly configured Button, Toggle, or Slider event, the generated base SHALL expose a matching protected virtual method for the business class to override.

#### Scenario: Business class overrides a click handler
- **WHEN** a generated Button event handler is overridden by the business class
- **THEN** the generated listener SHALL invoke the business override with the existing event signature

#### Scenario: Handler is not overridden
- **WHEN** an explicitly configured event has no business override
- **THEN** generation SHALL succeed and the generated base SHALL preserve an explicit no-domain-behavior default

#### Scenario: Dropdown value change is configured
- **WHEN** a Dropdown binding enables UnityEvent generation in the Manifest Inspector
- **THEN** the generated base SHALL register `onValueChanged` and expose a protected virtual handler with one `int value` parameter

### Requirement: Legacy partial types are not silently overwritten
The generator SHALL detect an existing business partial type that conflicts with the inheritance output and SHALL report a migration diagnostic before replacing its generated type relationship.

#### Scenario: Existing partial business file is found
- **WHEN** generation finds `Xxx.cs` using the old partial pattern
- **THEN** generation SHALL report that the type requires inheritance migration and SHALL NOT delete or overwrite the business file
