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
- Layer rules: `Core` has no dependencies.
- Layer rules: `Contracts` depends only on `Core`.
- Layer rules: `Application` depends on `Core` + `Contracts`.
- Layer rules: `Modules` depends on `Core` + `Contracts`.
- Layer rules: `Theme` depends only on `Core`.
- Layer rules: `Tui` depends on `Application` + `Modules` + `Theme` + `Contracts` + `Core`.
- Layer rules: `Tui` is the only project allowed to use `Spectre.Console`.
- Layer rules: `Desktop` is the future Avalonia frontend.
- Strict rule: modules must never depend on `Application`.
- Rendering rule: view models must remain UI-agnostic and live outside the TUI layer.
- Prefer plain classes/interfaces; no MediatR/CQRS/plugin framework.
- No tests in this prototype phase.

## How AI Agents Should Add Features
- Add or extend domain models in `src/Tva.Core` first.
- Add or extend extension contracts in `src/Tva.Contracts` for module/application seams.
- Add orchestration/state/service logic in `src/Tva.Application`.
- Add feature modules/screens in `src/Tva.Modules`.
- Add or adjust semantic tokens in `src/Tva.Theme`.
- Update rendering/input only in `src/Tva.Tui`.
- Keep project references one-directional toward core/application abstractions.
