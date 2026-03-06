# AGENT.md (Repository Root)

## Purpose
- Define high-level architecture and guardrails for the TVA workspace prototype.
- Keep the solution fast to iterate while preserving strict UI/core boundaries.

## What Code Is Allowed Here
- Solution-level files (`.sln`, `Directory.Build.props`, build docs).
- Cross-repo documentation and architecture notes.

## What Must NOT Go Here
- Feature/domain logic.
- Spectre/Avalonia UI code.
- Module implementations.

## Dependencies Allowed
- None for runtime code at root.

## Dependencies Forbidden
- Any runtime package references in root files.

## Architectural Rules
- `Tva.Core` and `Tva.Application` must remain UI-framework-agnostic.
- `Tva.Tui` is the only Spectre.Console-dependent layer.
- `Tva.Desktop` is a placeholder until Avalonia is introduced.
- Prefer plain classes/interfaces; no MediatR/CQRS/plugin framework.
- No tests in this prototype phase.

## How AI Agents Should Add Features
- Add or extend domain models in `src/Tva.Core` first.
- Add orchestration/state/service logic in `src/Tva.Application`.
- Add feature modules/screens in `src/Tva.Modules`.
- Add or adjust semantic tokens in `src/Tva.Theme`.
- Update rendering/input only in `src/Tva.Tui`.
- Keep project references one-directional toward core/application abstractions.
