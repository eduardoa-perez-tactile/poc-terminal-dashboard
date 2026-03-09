namespace Tva.Core;

public enum ReviewPriority
{
    Normal,
    High,
    Critical
}

public enum ReviewDecisionState
{
    Pending,
    Commented,
    Approved,
    ChangesRequested
}

public sealed record ReviewItemSummary(
    string Id,
    string Title,
    string Author,
    SourceControlSystem System,
    ReviewPriority Priority,
    ReviewDecisionState Decision,
    DateTimeOffset UpdatedAt,
    string Repository);

public sealed record ReviewQueueModel(
    IReadOnlyList<ReviewItemSummary> AwaitingReview,
    IReadOnlyList<ReviewItemSummary> UrgentReviews,
    IReadOnlyList<string> RecentNotes,
    DateTimeOffset RefreshedAt)
{
    public static ReviewQueueModel Empty(DateTimeOffset now) => new(
        [],
        [],
        [],
        now);
}
