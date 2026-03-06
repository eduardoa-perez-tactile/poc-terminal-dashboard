namespace Tva.Core;

public sealed record TerminalCommand(
    string Text,
    DateTimeOffset StartedAt);
