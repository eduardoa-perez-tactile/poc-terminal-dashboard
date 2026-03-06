namespace Tva.Core;

public sealed record TerminalCommandResult(
    TerminalCommand Command,
    int ExitCode,
    string StdOut,
    string StdErr,
    TimeSpan Duration);
