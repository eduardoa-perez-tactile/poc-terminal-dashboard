namespace Tva.Core;

public enum MeetingKind
{
    Standup,
    Planning,
    Review,
    OneOnOne,
    AdHoc
}

public sealed record MailItemSummary(
    string Id,
    string Sender,
    string Subject,
    bool RequiresAction,
    DateTimeOffset ReceivedAt);

public sealed record MeetingSummary(
    string Id,
    string Title,
    MeetingKind Kind,
    DateTimeOffset StartsAt,
    TimeSpan Duration,
    string Location,
    string? Notes = null);

public sealed record CommunicationsModel(
    IReadOnlyList<MailItemSummary> UnreadMail,
    IReadOnlyList<MeetingSummary> UpcomingMeetings,
    DateTimeOffset RefreshedAt)
{
    public static CommunicationsModel Empty(DateTimeOffset now) => new(
        [],
        [],
        now);
}
