namespace Tva.Core;

public sealed record TerminalViewModel(
    string Prompt,
    string InputBuffer,
    bool IsRunning,
    string? ActiveCommand,
    int? LastExitCode,
    IReadOnlyList<string> History,
    IReadOnlyList<TerminalOutputChunk> Output);
