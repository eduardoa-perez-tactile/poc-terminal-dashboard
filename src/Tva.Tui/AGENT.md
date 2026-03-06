# AGENT.md (Tva.Tui)

## Purpose
- Real terminal frontend using Spectre.Console for rendering and input handling.

## What Code Is Allowed Here
- Spectre renderers/presenters.
- Terminal input loop and keyboard shortcuts.
- Mapping from app shell/screen models to Spectre widgets.
- TUI runtime wiring/composition root.

## What Must NOT Go Here
- Domain models and durable application rules.
- Module registration logic beyond app bootstrapping.
- Shell execution internals (belongs in `Tva.Application`).

## Dependencies Allowed
- `Tva.Core`, `Tva.Application`, `Tva.Modules`, `Tva.Theme`.
- `Spectre.Console`.

## Dependencies Forbidden
- `Avalonia` packages.
- Putting business rules directly in renderer code.

## Architectural Rules
- This is the only project that knows Spectre.Console.
- Keep rendering as a translation layer from UI-agnostic models.
- Keep keybinding decisions thin and delegate behavior to `WorkspaceApp`.

## How AI Agents Should Add Features
- Extend `SpectreWorkspaceRenderer` for new visual sections/widgets.
- Add keyboard shortcuts in `TuiRuntime` and call application methods.
- Never move app-state decisions into Spectre view code.
