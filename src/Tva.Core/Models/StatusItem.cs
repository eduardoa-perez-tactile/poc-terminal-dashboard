namespace Tva.Core;

public sealed record StatusItem(
    string Key,
    string Value,
    SeverityLevel Severity = SeverityLevel.Info);
