# AGENT.md (Tva.Theme)

## Purpose
- Define design intent through portable semantic tokens and conventions.

## What Code Is Allowed Here
- Theme presets (palette tokens, frame conventions, glyph constants, spacing constants).
- Theme composition helpers that return core theme models.

## What Must NOT Go Here
- Spectre-specific style/rendering types.
- App orchestration logic.
- Module-specific feature logic.

## Dependencies Allowed
- `Tva.Core`.

## Dependencies Forbidden
- `Tva.Tui`, `Tva.Desktop`, `Spectre.Console`, `Avalonia`.

## Architectural Rules
- Tokens represent semantics (accent, warning, critical), not widget implementation.
- Keep themes frontend-agnostic so both TUI and desktop can map the same tokens.

## How AI Agents Should Add Features
- Add new semantic tokens to core models only if needed across frontends.
- Add concrete preset values and conventions here.
- Avoid leaking renderer-specific assumptions.
