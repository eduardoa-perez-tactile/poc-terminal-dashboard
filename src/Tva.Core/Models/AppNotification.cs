namespace Tva.Core;

public sealed record AppNotification(
    string Message,
    SeverityLevel Severity,
    DateTimeOffset CreatedAt);
