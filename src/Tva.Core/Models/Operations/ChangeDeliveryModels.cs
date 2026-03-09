namespace Tva.Core;

public enum SourceControlSystem
{
    Git,
    Svn
}

public enum DeliveryReadiness
{
    NotReady,
    NeedsReview,
    ReadyToOpen,
    Opened
}

public sealed record DeliveryBlocker(
    string Title,
    string Reason,
    SeverityLevel Severity);

public sealed record ChangeRequestSummary(
    string Id,
    string Title,
    SourceControlSystem System,
    string Repository,
    string BranchOrPath,
    DeliveryReadiness Readiness,
    bool IsReviewRequested,
    DateTimeOffset UpdatedAt);

public sealed record ChangeDeliveryModel(
    IReadOnlyList<ChangeRequestSummary> GitChanges,
    IReadOnlyList<ChangeRequestSummary> SvnChanges,
    IReadOnlyList<DeliveryBlocker> Blockers,
    DateTimeOffset RefreshedAt)
{
    public static ChangeDeliveryModel Empty(DateTimeOffset now) => new(
        [],
        [],
        [],
        now);
}
