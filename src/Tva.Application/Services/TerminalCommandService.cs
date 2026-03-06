using Tva.Core;

namespace Tva.Application;

public sealed class TerminalCommandService
{
    private readonly IShellExecutor _shellExecutor;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly object _sync = new();

    public TerminalCommandService(IShellExecutor shellExecutor)
    {
        _shellExecutor = shellExecutor;
    }

    public async Task ExecuteAsync(AppSessionState state, string rawCommand, CancellationToken cancellationToken)
    {
        var commandText = rawCommand.Trim();
        if (string.IsNullOrWhiteSpace(commandText))
        {
            return;
        }

        if (!await _gate.WaitAsync(0, cancellationToken))
        {
            AppendOutput(state, new TerminalOutputChunk(state.Now, "A command is already running.", true));
            return;
        }

        var command = new TerminalCommand(commandText, state.Now);
        state.Terminal.History.Add(commandText);
        state.Terminal.ActiveCommand = commandText;
        state.Terminal.IsRunning = true;
        AppendOutput(state, new TerminalOutputChunk(state.Now, $"> {commandText}", false));

        try
        {
            var result = await _shellExecutor.ExecuteAsync(
                command,
                chunk =>
                {
                    AppendOutput(state, chunk);
                    return ValueTask.CompletedTask;
                },
                cancellationToken);

            state.Terminal.LastExitCode = result.ExitCode;
            var severity = result.ExitCode == 0 ? SeverityLevel.Info : SeverityLevel.Warning;
            state.Notifications.Add(
                new AppNotification(
                    $"Command '{commandText}' finished with exit code {result.ExitCode} in {result.Duration.TotalMilliseconds:0}ms",
                    severity,
                    state.Now));
        }
        catch (OperationCanceledException)
        {
            AppendOutput(state, new TerminalOutputChunk(state.Now, "Command canceled.", true));
            state.Notifications.Add(new AppNotification("Command canceled.", SeverityLevel.Warning, state.Now));
        }
        catch (Exception ex)
        {
            AppendOutput(state, new TerminalOutputChunk(state.Now, ex.Message, true));
            state.Notifications.Add(new AppNotification($"Command failed: {ex.Message}", SeverityLevel.Critical, state.Now));
        }
        finally
        {
            state.Terminal.IsRunning = false;
            state.Terminal.ActiveCommand = null;
            _gate.Release();
        }
    }

    private void AppendOutput(AppSessionState state, TerminalOutputChunk chunk)
    {
        lock (_sync)
        {
            state.Terminal.Output.Add(chunk);
            if (state.Terminal.Output.Count > 320)
            {
                state.Terminal.Output.RemoveRange(0, state.Terminal.Output.Count - 320);
            }
        }
    }
}
