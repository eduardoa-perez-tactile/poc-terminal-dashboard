using Tva.Core;

namespace Tva.Application;

public interface IShellExecutor
{
    Task<TerminalCommandResult> ExecuteAsync(
        TerminalCommand command,
        Func<TerminalOutputChunk, ValueTask>? onOutput,
        CancellationToken cancellationToken);
}
