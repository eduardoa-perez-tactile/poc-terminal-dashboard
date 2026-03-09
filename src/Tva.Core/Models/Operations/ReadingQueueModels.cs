namespace Tva.Core;

public enum ReadingStatus
{
    Saved,
    InProgress,
    Done
}

public sealed record ReadingTopic(string Name);

public sealed record ReadingItemSummary(
    string Id,
    string Title,
    string Source,
    ReadingStatus Status,
    IReadOnlyList<ReadingTopic> Topics,
    DateTimeOffset SavedAt);

public sealed record ReadingQueueModel(
    IReadOnlyList<ReadingItemSummary> Items,
    DateTimeOffset RefreshedAt)
{
    public static ReadingQueueModel Empty(DateTimeOffset now) => new(
        [],
        now);
}
