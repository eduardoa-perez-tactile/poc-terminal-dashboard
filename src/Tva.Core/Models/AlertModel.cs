namespace Tva.Core;

public sealed record AlertModel(
    string Source,
    string Message,
    SeverityLevel Severity,
    DateTimeOffset Timestamp);
