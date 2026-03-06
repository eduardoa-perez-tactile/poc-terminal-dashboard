# AGENT.md (Tva.Modules)

## Purpose
- Implement feature modules and screen providers using core/application abstractions.

## What Code Is Allowed Here
- `IAppModule` implementations.
- `IScreenProvider` implementations.
- Feature-specific mock content for dashboard/alerts/events/terminal/waveform.

## What Must NOT Go Here
- Spectre/Avalonia UI widgets.
- Cross-module infrastructure that belongs in `Tva.Application`.
- Domain primitives that belong in `Tva.Core`.

## Dependencies Allowed
- `Tva.Core`.
- `Tva.Application` abstractions/state.

## Dependencies Forbidden
- `Tva.Tui`, `Tva.Desktop`, `Spectre.Console`, `Avalonia`.

## Architectural Rules
- Each module should expose metadata, nav entries, and screen providers.
- Modules may contribute dashboard panels and status items.
- Keep module logic lightweight and composition-friendly.

## How AI Agents Should Add Features
- Add a new module class implementing `IAppModule`.
- Register it in `ModuleBootstrap.CreateModules()`.
- Keep screen models UI-agnostic (`ScreenViewModel`, `PanelModel`, `TableModel`, etc.).
