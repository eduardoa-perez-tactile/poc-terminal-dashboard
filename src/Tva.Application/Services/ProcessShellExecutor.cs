using System.Diagnostics;
using System.Text;
using System.Threading.Channels;
using Tva.Core;

namespace Tva.Application;

public sealed class ProcessShellExecutor : IShellExecutor, ITerminalSessionFactory
{
    public async Task<TerminalCommandResult> ExecuteAsync(
        TerminalCommand command,
        Func<TerminalOutputChunk, ValueTask>? onOutput,
        CancellationToken cancellationToken)
    {
        var (fileName, args) = ResolveShell(command.Text);
        var start = DateTimeOffset.UtcNow;

        using var process = new Process
        {
            StartInfo =
            {
                FileName = fileName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        foreach (var arg in args)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        var stdOut = new StringBuilder();
        var stdErr = new StringBuilder();

        process.Start();

        var stdoutTask = PumpAsync(process.StandardOutput, false, stdOut, onOutput, cancellationToken);
        var stderrTask = PumpAsync(process.StandardError, true, stdErr, onOutput, cancellationToken);

        try
        {
            await process.WaitForExitAsync(cancellationToken);
            await Task.WhenAll(stdoutTask, stderrTask);
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            throw;
        }

        return new TerminalCommandResult(
            command,
            process.ExitCode,
            stdOut.ToString(),
            stdErr.ToString(),
            DateTimeOffset.UtcNow - start);
    }

    private static async Task PumpAsync(
        StreamReader reader,
        bool isError,
        StringBuilder collector,
        Func<TerminalOutputChunk, ValueTask>? onOutput,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync();
            if (line is null)
            {
                break;
            }

            collector.AppendLine(line);
            if (onOutput is not null)
            {
                await onOutput(new TerminalOutputChunk(DateTimeOffset.Now, line, isError));
            }
        }
    }

    private static (string fileName, IReadOnlyList<string> args) ResolveShell(string command)
    {
        if (OperatingSystem.IsWindows())
        {
            var shell = Environment.GetEnvironmentVariable("COMSPEC");
            return (string.IsNullOrWhiteSpace(shell) ? "cmd.exe" : shell, ["/c", command]);
        }

        var unixShell = Environment.GetEnvironmentVariable("SHELL");
        if (string.IsNullOrWhiteSpace(unixShell))
        {
            unixShell = "/bin/bash";
        }

        return (unixShell, ["-lc", command]);
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Best-effort cancellation cleanup.
        }
    }

    public ITerminalSession Create() => new ProcessTerminalSession();

    private sealed class ProcessTerminalSession : ITerminalSession
    {
        private readonly Channel<string> _output = Channel.CreateUnbounded<string>();
        private bool _closed;

        public IAsyncEnumerable<string> Output => _output.Reader.ReadAllAsync();

        public void SendInput(string input)
        {
            if (_closed)
            {
                throw new InvalidOperationException("Terminal session is closed.");
            }

            _output.Writer.TryWrite($"Interactive terminal session is not implemented yet. Input received: {input}");
        }

        public Task CloseAsync()
        {
            if (_closed)
            {
                return Task.CompletedTask;
            }

            _closed = true;
            _output.Writer.TryComplete();
            return Task.CompletedTask;
        }
    }
}
