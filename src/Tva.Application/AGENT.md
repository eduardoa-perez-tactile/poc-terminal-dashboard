# AGENT.md (Tva.Application)

## Purpose
- Application brain: orchestration, navigation state, module registry, session lifecycle, command execution abstraction.

## What Code Is Allowed Here
- App state containers.
- Orchestration services.
- Module/screen abstraction interfaces.
- Shell execution abstraction and implementation.
- Live update loop and mock data coordination.

## What Must NOT Go Here
- Spectre/Avalonia rendering code.
- Concrete module screen content (belongs to `Tva.Modules`).
- Persistence/database infrastructure for this prototype.

## Dependencies Allowed
- `Tva.Core`.
- .NET BCL (`System.Diagnostics` etc.).

## Dependencies Forbidden
- `Tva.Tui`, `Tva.Desktop`, `Spectre.Console`, `Avalonia`.
- Heavy architecture frameworks (MediatR/CQRS frameworks).

## Architectural Rules
- Keep interfaces simple and explicit.
- Prefer direct method calls and simple services over command buses.
- Keep terminal execution behind `IShellExecutor`.
- Maintain mutable session state in one place (`AppSessionState`).

## How AI Agents Should Add Features
- Add new use-case services here.
- Extend `AppSessionState` for app-wide runtime state.
- Keep module contracts stable and UI-agnostic.
- Add notifications/status/navigation behavior here, not in UI.
