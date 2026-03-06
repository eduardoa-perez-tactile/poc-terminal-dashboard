# AGENT.md (/src)

## Purpose
- Host all runtime projects for the TVA workspace prototype.

## What Code Is Allowed Here
- Project directories only (`Tva.Core`, `Tva.Application`, `Tva.Modules`, `Tva.Theme`, `Tva.Tui`, `Tva.Desktop`).

## What Must NOT Go Here
- Loose feature files directly under `/src`.
- Cross-layer shortcuts that bypass project boundaries.

## Dependencies Allowed
- Project-to-project references according to layer rules.

## Dependencies Forbidden
- Referencing UI projects (`Tva.Tui`, future `Tva.Desktop`) from `Tva.Core`, `Tva.Application`, or `Tva.Modules`.

## Architectural Rules
- Keep dependency direction inward: UI -> Modules/Application/Core/Theme.
- Theme intent lives in `Tva.Theme`; rendering details live in UI projects.

## How AI Agents Should Add Features
- Place new files in the correct project directory by responsibility.
- If a change crosses multiple layers, implement from inner to outer: Core -> Application -> Modules -> UI.
