using Tva.Contracts;
using Tva.Core;

namespace Tva.Application;

public sealed class PrototypeWorkTrackerDataSource : IWorkTrackerDataSource
{
    public WorkQueueModel GetSnapshot(DateTimeOffset now)
    {
        var current = new WorkItemSummary(
            "TVA-142",
            "Add operational modules to workspace prototype",
            WorkItemStatus.InProgress,
            WorkItemPriority.High,
            WorkItemSource.Jira,
            "Developer Experience",
            "terminal",
            "feature/add-modules",
            now.AddMinutes(-18),
            "Define module seams, docs, and TUI navigation.");

        return new WorkQueueModel(
            current,
            [
                current,
                new WorkItemSummary(
                    "TVA-149",
                    "Review Android device profile issue",
                    WorkItemStatus.Todo,
                    WorkItemPriority.Medium,
                    WorkItemSource.Jira,
                    "Android",
                    "game-client",
                    null,
                    now.AddHours(-3),
                    "Needs repro notes from QA."),
                new WorkItemSummary(
                    "TVA-155",
                    "Investigate Unity prefab import regression",
                    WorkItemStatus.Todo,
                    WorkItemPriority.High,
                    WorkItemSource.Jira,
                    "Unity",
                    "tools",
                    null,
                    now.AddHours(-5),
                    "Likely tied to asset bundle metadata.")
            ],
            [
                current,
                new WorkItemSummary(
                    "TVA-138",
                    "Prepare iOS crash triage summary",
                    WorkItemStatus.InReview,
                    WorkItemPriority.Medium,
                    WorkItemSource.Jira,
                    "iOS",
                    "mobile-client",
                    "bugfix/ios-crash-triage",
                    now.AddHours(-1),
                    "Waiting on one final repro confirmation.")
            ],
            [
                new WorkItemSummary(
                    "TVA-133",
                    "Sync SVN packaging scripts with release branch",
                    WorkItemStatus.Blocked,
                    WorkItemPriority.High,
                    WorkItemSource.Jira,
                    "Build Pipeline",
                    "release-tools",
                    "/branches/release-2026-03",
                    now.AddHours(-7),
                    "Blocked on release engineering confirmation.")
            ],
            now);
    }
}

public sealed class PrototypeCodingActivityDataSource : ICodingActivityDataSource
{
    public CodingSessionModel GetSnapshot(DateTimeOffset now)
    {
        return new CodingSessionModel(
            new RepositorySummary(
                "terminal",
                "feature/add-modules",
                2,
                0,
                true,
                "/Users/eduardoleale/workspace/terminal"),
            new AiSessionSummary(
                "Claude Code",
                "Implement operational module architecture and docs",
                "active",
                now.AddMinutes(-32),
                "Keep UI concerns in Tva.Tui only."),
            [
                new LocalChangeSummary("src/Tva.Contracts/State/IModuleState.cs", "modified", false),
                new LocalChangeSummary("src/Tva.Modules/ModuleBootstrap.cs", "modified", false),
                new LocalChangeSummary("docs/workspace-operations-dashboard.md", "added", false)
            ],
            "Finish the v1 module skeleton, then wire renderer/navigation changes.",
            now);
    }
}

public sealed class PrototypeChangeDeliveryDataSource : IChangeDeliveryDataSource
{
    public ChangeDeliveryModel GetSnapshot(DateTimeOffset now)
    {
        return new ChangeDeliveryModel(
            [
                new ChangeRequestSummary(
                    "PR-418",
                    "Add operations dashboard modules",
                    SourceControlSystem.Git,
                    "terminal",
                    "feature/add-modules",
                    DeliveryReadiness.ReadyToOpen,
                    false,
                    now.AddMinutes(-11))
            ],
            [
                new ChangeRequestSummary(
                    "SVN-202",
                    "Sync mobile packaging scripts",
                    SourceControlSystem.Svn,
                    "release-tools",
                    "/branches/release-2026-03",
                    DeliveryReadiness.NeedsReview,
                    true,
                    now.AddHours(-2))
            ],
            [
                new DeliveryBlocker(
                    "SVN release sync",
                    "Awaiting release engineering sign-off before opening review.",
                    SeverityLevel.Warning)
            ],
            now);
    }
}

public sealed class PrototypeReviewQueueDataSource : IReviewQueueDataSource
{
    public ReviewQueueModel GetSnapshot(DateTimeOffset now)
    {
        return new ReviewQueueModel(
            [
                new ReviewItemSummary(
                    "PR-411",
                    "Unity addressables startup cleanup",
                    "Marta",
                    SourceControlSystem.Git,
                    ReviewPriority.High,
                    ReviewDecisionState.Pending,
                    now.AddMinutes(-48),
                    "game-client"),
                new ReviewItemSummary(
                    "SVN-198",
                    "iOS entitlement profile update",
                    "Theo",
                    SourceControlSystem.Svn,
                    ReviewPriority.Normal,
                    ReviewDecisionState.Commented,
                    now.AddHours(-4),
                    "mobile-client")
            ],
            [
                new ReviewItemSummary(
                    "PR-411",
                    "Unity addressables startup cleanup",
                    "Marta",
                    SourceControlSystem.Git,
                    ReviewPriority.High,
                    ReviewDecisionState.Pending,
                    now.AddMinutes(-48),
                    "game-client")
            ],
            [
                "Check Android asset loading impact before approving PR-411.",
                "SVN-198 needs confirmation from build signing owner."
            ],
            now);
    }
}

public sealed class PrototypeCommunicationsDataSource : ICommunicationsDataSource
{
    public CommunicationsModel GetSnapshot(DateTimeOffset now)
    {
        return new CommunicationsModel(
            [
                new MailItemSummary(
                    "MAIL-901",
                    "Build Ops",
                    "Android nightly regression follow-up",
                    true,
                    now.AddMinutes(-23)),
                new MailItemSummary(
                    "MAIL-902",
                    "Tech Art",
                    "Unity importer edge-case repro",
                    true,
                    now.AddHours(-1))
            ],
            [
                new MeetingSummary(
                    "MEET-31",
                    "Gameplay standup",
                    MeetingKind.Standup,
                    now.AddMinutes(25),
                    TimeSpan.FromMinutes(15),
                    "Teams",
                    "Mention TVA-142 and Unity regression risk."),
                new MeetingSummary(
                    "MEET-32",
                    "Mobile crash triage",
                    MeetingKind.Review,
                    now.AddHours(2),
                    TimeSpan.FromMinutes(30),
                    "Zoom",
                    "Bring the latest iOS repro notes.")
            ],
            now);
    }
}

public sealed class PrototypeWorkLogDataSource : IWorkLogDataSource
{
    public WorkLogModel GetSnapshot(DateTimeOffset now)
    {
        return new WorkLogModel(
            [
                new WorkLogEntry(now.AddHours(-5), "Standup", "Aligned on Unity importer investigation."),
                new WorkLogEntry(now.AddHours(-3), "Coding", "Refactored module contracts for typed state slices."),
                new WorkLogEntry(now.AddHours(-1), "Review", "Left review notes on startup cleanup PR.")
            ],
            new DailySummaryDraft(
                DateOnly.FromDateTime(now.LocalDateTime),
                "Implemented the v1 operations dashboard structure, kept the terminal as a dedicated workspace screen, and mapped future integrations behind datasource contracts.",
                true),
            LogCoverageStatus.NeedsSummary,
            now);
    }
}

public sealed class PrototypeReadingListDataSource : IReadingListDataSource
{
    public ReadingQueueModel GetSnapshot(DateTimeOffset now)
    {
        return new ReadingQueueModel(
            [
                new ReadingItemSummary(
                    "READ-71",
                    "Unity memory profiling for large scenes",
                    "Internal Wiki",
                    ReadingStatus.Saved,
                    [new ReadingTopic("Unity"), new ReadingTopic("Performance")],
                    now.AddDays(-1))
            ],
            now);
    }
}
