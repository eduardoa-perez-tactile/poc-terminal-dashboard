namespace Tva.Core;

public enum WorkItemStatus
{
    Todo,
    InProgress,
    Blocked,
    InReview,
    Done
}

public enum WorkItemPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum WorkItemSource
{
    Jira,
    Git,
    Svn,
    Email,
    Calendar,
    Notes,
    Article
}

public sealed record WorkItemSummary(
    string Id,
    string Title,
    WorkItemStatus Status,
    WorkItemPriority Priority,
    WorkItemSource Source,
    string Area,
    string? Repository,
    string? Branch,
    DateTimeOffset UpdatedAt,
    string? Notes = null);

public sealed record WorkQueueModel(
    WorkItemSummary? CurrentItem,
    IReadOnlyList<WorkItemSummary> AssignedItems,
    IReadOnlyList<WorkItemSummary> InProgressItems,
    IReadOnlyList<WorkItemSummary> BlockedItems,
    DateTimeOffset RefreshedAt)
{
    public static WorkQueueModel Empty(DateTimeOffset now) => new(
        null,
        [],
        [],
        [],
        now);
}
