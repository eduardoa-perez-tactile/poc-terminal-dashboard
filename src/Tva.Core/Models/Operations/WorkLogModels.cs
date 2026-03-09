namespace Tva.Core;

public enum LogCoverageStatus
{
    Clear,
    MissingNotes,
    NeedsSummary
}

public sealed record WorkLogEntry(
    DateTimeOffset Timestamp,
    string Category,
    string Text);

public sealed record DailySummaryDraft(
    DateOnly Day,
    string Summary,
    bool IsReadyToShare);

public sealed record WorkLogModel(
    IReadOnlyList<WorkLogEntry> Entries,
    DailySummaryDraft SummaryDraft,
    LogCoverageStatus Coverage,
    DateTimeOffset RefreshedAt)
{
    public static WorkLogModel Empty(DateTimeOffset now) => new(
        [],
        new DailySummaryDraft(DateOnly.FromDateTime(now.LocalDateTime), string.Empty, false),
        LogCoverageStatus.MissingNotes,
        now);
}
