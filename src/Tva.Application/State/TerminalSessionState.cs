using Tva.Contracts;
using Tva.Core;

namespace Tva.Application;

public sealed class TerminalSessionState : ITerminalState
{
    public string InputBuffer { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
    public string? ActiveCommand { get; set; }
    public int? LastExitCode { get; set; }
    public List<TerminalOutputChunk> Output { get; } = [];
    public CommandHistory History { get; } = new();

    IReadOnlyList<string> ITerminalState.History => History.Entries;
    IReadOnlyList<TerminalOutputChunk> ITerminalState.Output => Output;
}
