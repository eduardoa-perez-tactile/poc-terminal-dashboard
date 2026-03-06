namespace Tva.Core;

public sealed record TerminalOutputChunk(
    DateTimeOffset Timestamp,
    string Text,
    bool IsError);
