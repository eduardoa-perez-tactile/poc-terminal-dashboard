using Spectre.Console;
using Tva.Application;
using Tva.Core;
using Tva.Core.Ids;
using Tva.Tui.Rendering;

namespace Tva.Tui.Runtime;

public sealed class TuiRuntime
{
    private readonly WorkspaceApp _app;
    private readonly SpectreWorkspaceRenderer _renderer;

    public TuiRuntime(WorkspaceApp app, SpectreWorkspaceRenderer renderer)
    {
        _app = app;
        _renderer = renderer;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _app.Initialize();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var shouldExit = false;

        await AnsiConsole.Live(new Markup("Booting TVA workspace..."))
            .StartAsync(async context =>
            {
                while (!shouldExit && !cts.IsCancellationRequested)
                {
                    _app.Tick();
                    var shell = _app.BuildShellModel();
                    context.UpdateTarget(_renderer.Render(shell));
                    context.Refresh();

                    while (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true);
                        if (HandleKey(key, cts.Token))
                        {
                            shouldExit = true;
                            break;
                        }
                    }

                    await Task.Delay(85, cts.Token);
                }
            });
    }

    private bool HandleKey(ConsoleKeyInfo key, CancellationToken cancellationToken)
    {
        if (key.Key == ConsoleKey.Q && key.Modifiers == 0)
        {
            return true;
        }

        if (key.Key == ConsoleKey.Escape)
        {
            if (_app.WarningVisible)
            {
                _app.DismissWarning();
                return false;
            }

            return true;
        }

        if (key.Key == ConsoleKey.Tab)
        {
            if ((key.Modifiers & ConsoleModifiers.Shift) != 0)
            {
                _app.NavigatePrevious();
            }
            else
            {
                _app.NavigateNext();
            }

            return false;
        }

        if (key.Key == ConsoleKey.B && key.Modifiers == 0)
        {
            _app.NavigateBack();
            return false;
        }

        if (key.Key == ConsoleKey.W && key.Modifiers == 0)
        {
            if (_app.WarningVisible)
            {
                _app.DismissWarning();
            }
            else
            {
                _app.ShowWarning("Temporal variance threshold exceeded in synthetic feed.");
            }

            return false;
        }

        switch (key.Key)
        {
            case ConsoleKey.D1:
            case ConsoleKey.NumPad1:
                _app.NavigateTo(ScreenCatalog.Home);
                return false;
            case ConsoleKey.D2:
            case ConsoleKey.NumPad2:
                _app.NavigateTo(ScreenCatalog.Alerts);
                return false;
            case ConsoleKey.D3:
            case ConsoleKey.NumPad3:
                _app.NavigateTo(ScreenCatalog.Events);
                return false;
            case ConsoleKey.D4:
            case ConsoleKey.NumPad4:
                _app.NavigateTo(ScreenCatalog.Terminal);
                return false;
            case ConsoleKey.D5:
            case ConsoleKey.NumPad5:
                _app.NavigateTo(ScreenCatalog.Waveform);
                return false;
        }

        if (!_app.IsTerminalActive)
        {
            return false;
        }

        if (key.Key == ConsoleKey.Enter)
        {
            _ = RunCommandAsync(cancellationToken);
            return false;
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            _app.BackspaceTerminalInput();
            return false;
        }

        if (key.Key == ConsoleKey.UpArrow)
        {
            _app.RecallPreviousCommand();
            return false;
        }

        if (key.Key == ConsoleKey.DownArrow)
        {
            _app.RecallNextCommand();
            return false;
        }

        if (!char.IsControl(key.KeyChar) && key.Modifiers == 0)
        {
            _app.AppendTerminalInput(key.KeyChar);
        }

        return false;
    }

    private async Task RunCommandAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _app.ExecuteTerminalInputAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _app.Notify($"Terminal execution error: {ex.Message}", SeverityLevel.Critical);
        }
    }
}
