namespace Tva.Core;

public sealed record RepositorySummary(
    string Name,
    string Branch,
    int AheadBy,
    int BehindBy,
    bool HasUncommittedChanges,
    string WorkspacePath);

public sealed record LocalChangeSummary(
    string Path,
    string ChangeKind,
    bool IsStaged);

public sealed record AiSessionSummary(
    string ToolName,
    string ActiveTask,
    string Status,
    DateTimeOffset StartedAt,
    string? Notes = null);

public sealed record CodingSessionModel(
    RepositorySummary Repository,
    AiSessionSummary AiSession,
    IReadOnlyList<LocalChangeSummary> LocalChanges,
    string FocusNote,
    DateTimeOffset RefreshedAt)
{
    public static CodingSessionModel Empty(DateTimeOffset now) => new(
        new RepositorySummary("unknown", "unknown", 0, 0, false, string.Empty),
        new AiSessionSummary("Claude Code", "No active task", "idle", now),
        [],
        "No active coding focus.",
        now);
}
