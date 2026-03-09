# TVA Workspace Operations Dashboard

## What Changed

The prototype now uses operational modules instead of the earlier demo modules.

Registered v1 modules:

- `DashboardModule`
- `WorkQueueModule`
- `CodingSessionModule`
- `TerminalModule`
- `ChangeDeliveryModule`
- `ReviewQueueModule`
- `CommunicationsModule`
- `WorkLogModule`

Planned v2 module:

- `ReadingQueueModule`

The module lineup is registered in `/Users/eduardoleale/workspace/terminal/src/Tva.Modules/ModuleBootstrap.cs`.

## Module Purpose And Screens

### Home / Dashboard

- Purpose: aggregate the operational state into one software engineer daily dashboard.
- Screen: `Home`
- Main content:
  - daily snapshot table
  - ordered dashboard panels contributed by the v1 modules
  - alert rail for items that need attention

### Work Queue

- Purpose: show assigned Jira work, current ticket, in-progress work, and blocked items.
- Screen: `Work Queue`
- Main content:
  - `Current Ticket` panel
  - `In Progress` panel
  - `Blocked / Waiting` panel
  - `Assigned Queue` table

### Coding Session

- Purpose: show active repo, branch, Claude Code context, and local change summary.
- Screen: `Coding Session`
- Main content:
  - `Active Repo` panel
  - `Claude Code` panel
  - `Focus` panel
  - `Local Changes` table

### Terminal

- Purpose: provide an in-app shell screen for command execution.
- Screen: `Terminal`
- Main content:
  - live command output
  - input buffer
  - command history
  - execution status

### Change Delivery

- Purpose: track outbound Git and SVN work that is being prepared for review.
- Screen: `Change Delivery`
- Main content:
  - `Git Outbound` panel
  - `SVN Outbound` panel
  - `Delivery Blockers` panel
  - `Outbound Changes` table

### Review Queue

- Purpose: track inbound review work separately from outbound change delivery.
- Screen: `Review Queue`
- Main content:
  - `Awaiting My Review` panel
  - `Urgent / Aging` panel
  - `Review Notes` panel
  - `Review Queue` table

### Communications

- Purpose: keep unread mail and upcoming meetings visible so coding time is protected.
- Screen: `Communications`
- Main content:
  - `Unread / Action Needed Mail` panel
  - `Next Meetings` panel
  - `Meeting Prep` panel
  - `Mail Queue` table

### Work Log

- Purpose: keep a running daily log and a summary draft for end-of-day reporting.
- Screen: `Work Log`
- Main content:
  - `Summary Draft` panel
  - `Coverage` panel
  - `Missing Notes` panel
  - `Today Timeline` table

## Home Dashboard Layout

The home dashboard is organized around these regions:

- `HeroLeft`: work queue
- `HeroRight`: coding session
- `MainLeft`: change delivery
- `MainRight`: review queue
- `RailTop`: communications
- `RailBottom`: work log
- `Footer`: terminal status

These placements are represented by `DashboardRegion` and `DashboardPanelModel` in `/Users/eduardoleale/workspace/terminal/src/Tva.Core/Models/Operations`.

The renderer in `/Users/eduardoleale/workspace/terminal/src/Tva.Tui/Rendering/SpectreWorkspaceRenderer.cs` stays responsible for deciding how those regions look in Spectre.Console.

## Strict Layering

### `Tva.Core`

Holds UI-agnostic shared models:

- dashboard placement models
- work queue models
- coding session models
- change delivery models
- review queue models
- communications models
- work log models
- reading queue models for the deferred v2 module

Key files live under `/Users/eduardoleale/workspace/terminal/src/Tva.Core/Models/Operations`.

### `Tva.Contracts`

Holds shared contracts:

- module contracts
- state slice interfaces
- datasource interfaces

Datasource contracts live under `/Users/eduardoleale/workspace/terminal/src/Tva.Contracts/DataSources`.

State slice contracts live under `/Users/eduardoleale/workspace/terminal/src/Tva.Contracts/State`.

### `Tva.Application`

Owns orchestration and datasource integration:

- application session state
- refresh loop
- terminal execution
- prototype datasource implementations

The refresh loop is in `/Users/eduardoleale/workspace/terminal/src/Tva.Application/Services/LiveUpdateService.cs`.

Prototype datasource implementations live in `/Users/eduardoleale/workspace/terminal/src/Tva.Application/DataSources/PrototypeDataSources.cs`.

### `Tva.Modules`

Owns feature composition only:

- navigation entries
- dashboard panel contribution
- screen providers

Modules read normalized state through `IModuleState` and do not know whether the underlying data came from a CLI, REST API, or local file.

### `Tva.Tui`

Owns Spectre.Console rendering and keyboard input only.

- screen navigation
- screen layout
- terminal input handling

No datasource logic or module business rules should move here.

## How Tool Integration Is Handled

The app now has explicit datasource seams in `Tva.Contracts`:

- `IWorkTrackerDataSource`
- `ICodingActivityDataSource`
- `IChangeDeliveryDataSource`
- `IReviewQueueDataSource`
- `ICommunicationsDataSource`
- `IWorkLogDataSource`
- `IReadingListDataSource`

The current prototype uses in-memory implementations in `PrototypeDataSources.cs`.

The intended real integration path is:

- Jira:
  - replace `PrototypeWorkTrackerDataSource` with a Jira-backed adapter
  - map issues into `WorkQueueModel` and `WorkItemSummary`
- Git:
  - replace the prototype coding and delivery datasources with adapters that call Git CLI or parse repo state directly
  - map branch and local change data into `CodingSessionModel` and `ChangeDeliveryModel`
- SVN:
  - implement the SVN side inside `IChangeDeliveryDataSource`
  - normalize SVN review-prep state into the same delivery models used by Git
- Claude Code:
  - feed task/session context into `ICodingActivityDataSource`
  - keep actual command execution in the terminal path
- Email and calendar:
  - replace `PrototypeCommunicationsDataSource`
  - map messages and meetings into `CommunicationsModel`
- Work log:
  - replace `PrototypeWorkLogDataSource`
  - back it with a file, notes store, or API

This design keeps integrations replaceable. A datasource implementation can change without forcing module or TUI changes, as long as it still produces the shared core models.

## Terminal Design

The terminal remains a first-class screen, not a side feature.

Relevant files:

- `/Users/eduardoleale/workspace/terminal/src/Tva.Modules/Terminal/TerminalModule.cs`
- `/Users/eduardoleale/workspace/terminal/src/Tva.Application/Services/TerminalCommandService.cs`
- `/Users/eduardoleale/workspace/terminal/src/Tva.Application/Services/ProcessShellExecutor.cs`
- `/Users/eduardoleale/workspace/terminal/src/Tva.Tui/Runtime/TuiRuntime.cs`

Current behavior:

- the user opens the `Terminal` screen
- typed input is stored in terminal session state
- pressing `Enter` executes the buffered command through `TerminalCommandService`
- `ProcessShellExecutor` runs the command in the OS shell
- stdout/stderr stream back into terminal output

Important limitation:

- this is an in-app command terminal
- it is not yet a long-lived PTY session with shell state shared across commands

That limitation is deliberate for the prototype because it keeps the execution path simple and compatible with the current layered design. If a full interactive shell is needed later, the right place to extend is the application terminal session abstraction, not the modules or the TUI screen contract.

## Keyboard Navigation

The current v1 screen shortcuts are:

- `1` Home
- `2` Work Queue
- `3` Coding Session
- `4` Terminal
- `5` Change Delivery
- `6` Reviews
- `7` Communications
- `8` Work Log

`Tab` and `Shift+Tab` cycle through screens, and `B` navigates back.

## Current Prototype Data Flow

1. `Program.cs` creates the compile-time module list and the prototype datasource implementations.
2. `LiveUpdateService` refreshes all datasource snapshots into `AppSessionState`.
3. `ModuleCatalog` asks each module for dashboard panels, status items, and screens.
4. `WorkspaceApp` builds the `AppShellModel`.
5. `SpectreWorkspaceRenderer` turns that shell model into the terminal UI.

## Deferred v2 Scope

The reading queue models and datasource contract were added, but the module is intentionally not registered yet.

That keeps the current scope aligned with the requested operational dashboard while preserving a clean extension point for the v2 reading module.
