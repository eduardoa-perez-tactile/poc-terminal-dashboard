# AGENT.md (Tva.Core)

## Purpose
- Hold durable, UI-independent domain models and primitives shared by all frontends.

## What Code Is Allowed Here
- Domain records/enums/value objects.
- Screen/module identifiers.
- UI-agnostic view/layout models.
- Theme token model contracts.
- Terminal command/result models.

## What Must NOT Go Here
- Business orchestration services.
- Module registration/composition.
- Process execution or OS-specific logic.
- Spectre/Avalonia types.

## Dependencies Allowed
- .NET BCL only.

## Dependencies Forbidden
- `Spectre.Console`, `Avalonia`, `Microsoft.Extensions.*` UI concerns.
- Any project reference to `Tva.Application`, `Tva.Modules`, `Tva.Theme`, `Tva.Tui`, or `Tva.Desktop`.

## Architectural Rules
- Keep models immutable where practical (records preferred).
- No framework lock-in.
- Types here should survive a frontend swap unchanged.

## How AI Agents Should Add Features
- Add new shared concepts as explicit records/enums.
- Prefer extending existing models over creating parallel variants.
- Keep names explicit and framework-neutral.
