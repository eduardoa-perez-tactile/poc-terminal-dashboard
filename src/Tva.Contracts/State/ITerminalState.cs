using Tva.Core;

namespace Tva.Contracts;

public interface ITerminalState
{
    string InputBuffer { get; }
    bool IsRunning { get; }
    string? ActiveCommand { get; }
    int? LastExitCode { get; }
    IReadOnlyList<string> History { get; }
    IReadOnlyList<TerminalOutputChunk> Output { get; }
}
