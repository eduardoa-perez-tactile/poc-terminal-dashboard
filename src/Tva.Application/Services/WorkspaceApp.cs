using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Application;

public sealed class WorkspaceApp
{
    private readonly ModuleRegistry _moduleRegistry;
    private readonly TerminalCommandService _terminalCommands;
    private readonly LiveUpdateService _liveUpdates;
    private readonly AppTheme _theme;
    private readonly AppSessionState _state;

    public WorkspaceApp(
        ModuleRegistry moduleRegistry,
        TerminalCommandService terminalCommands,
        LiveUpdateService liveUpdates,
        AppTheme theme)
    {
        _moduleRegistry = moduleRegistry;
        _terminalCommands = terminalCommands;
        _liveUpdates = liveUpdates;
        _theme = theme;
        _state = new AppSessionState();
    }

    public ScreenId ActiveScreenId => _state.ActiveScreenId;

    public bool IsTerminalActive => _state.ActiveScreenId == ScreenCatalog.Terminal;

    public bool WarningVisible => _state.ShowWarning;

    public void Initialize()
    {
        _liveUpdates.Initialize(_state);
        _state.Notifications.Add(new AppNotification("Boot sequence started.", SeverityLevel.Info, _state.Now));
    }

    public void Tick()
    {
        _liveUpdates.Tick(_state);

        _state.DashboardPanels.Clear();
        _state.DashboardPanels.AddRange(_moduleRegistry.BuildDashboardPanels(_state));

        if (_state.ActiveScreenId == ScreenCatalog.Boot && _state.TickCount > 10)
        {
            NavigateTo(ScreenCatalog.Home);
            _state.Notifications.Add(new AppNotification("Boot complete.", SeverityLevel.Info, _state.Now));
        }
    }

    public AppShellModel BuildShellModel()
    {
        var activeScreen = _state.ActiveScreenId == ScreenCatalog.Boot
            ? BuildBootScreen()
            : _moduleRegistry.BuildScreen(_state.ActiveScreenId, _state);

        var statusItems = new List<StatusItem>
        {
            new("Screen", _state.ActiveScreenId.Value),
            new("Clock", _state.Now.ToString("HH:mm:ss")),
            new("Alerts", _state.Alerts.Count.ToString())
        };

        statusItems.AddRange(_moduleRegistry.BuildStatusItems(_state));

        return new AppShellModel(
            "TVA Workspace Prototype",
            "Retro command nexus",
            _state.ActiveScreenId,
            _moduleRegistry.Navigation,
            activeScreen,
            statusItems,
            _state.Notifications.LastOrDefault(),
            _state.ShowWarning,
            _state.WarningMessage,
            _theme);
    }

    public void NavigateTo(ScreenId screenId)
    {
        if (_state.ActiveScreenId == screenId)
        {
            return;
        }

        _state.BackStack.Push(_state.ActiveScreenId);
        _state.ActiveScreenId = screenId;
    }

    public void NavigateNext()
    {
        var entries = _moduleRegistry.Navigation;
        if (entries.Count == 0)
        {
            return;
        }

        var currentIndex = entries
            .Select(static (entry, index) => new { entry, index })
            .FirstOrDefault(x => x.entry.ScreenId == _state.ActiveScreenId)?.index ?? -1;

        var nextIndex = currentIndex < 0 ? 0 : (currentIndex + 1) % entries.Count;
        NavigateTo(entries[nextIndex].ScreenId);
    }

    public void NavigatePrevious()
    {
        var entries = _moduleRegistry.Navigation;
        if (entries.Count == 0)
        {
            return;
        }

        var currentIndex = entries
            .Select(static (entry, index) => new { entry, index })
            .FirstOrDefault(x => x.entry.ScreenId == _state.ActiveScreenId)?.index ?? 0;

        var previousIndex = currentIndex - 1;
        if (previousIndex < 0)
        {
            previousIndex = entries.Count - 1;
        }

        NavigateTo(entries[previousIndex].ScreenId);
    }

    public void NavigateBack()
    {
        if (_state.BackStack.Count == 0)
        {
            return;
        }

        _state.ActiveScreenId = _state.BackStack.Pop();
    }

    public void ShowWarning(string message)
    {
        _state.ShowWarning = true;
        _state.WarningMessage = message;
    }

    public void DismissWarning()
    {
        _state.ShowWarning = false;
        _state.WarningMessage = null;
    }

    public void AppendTerminalInput(char value)
    {
        _state.Terminal.InputBuffer += value;
    }

    public void BackspaceTerminalInput()
    {
        if (_state.Terminal.InputBuffer.Length == 0)
        {
            return;
        }

        _state.Terminal.InputBuffer = _state.Terminal.InputBuffer[..^1];
    }

    public void RecallPreviousCommand()
    {
        _state.Terminal.InputBuffer = _state.Terminal.History.Previous(_state.Terminal.InputBuffer);
    }

    public void RecallNextCommand()
    {
        _state.Terminal.InputBuffer = _state.Terminal.History.Next();
    }

    public async Task ExecuteTerminalInputAsync(CancellationToken cancellationToken)
    {
        var commandText = _state.Terminal.InputBuffer;
        _state.Terminal.InputBuffer = string.Empty;

        await _terminalCommands.ExecuteAsync(_state, commandText, cancellationToken);
    }

    public void Notify(string message, SeverityLevel severity)
    {
        _state.Notifications.Add(new AppNotification(message, severity, _state.Now));
    }

    private static ScreenViewModel BuildBootScreen()
    {
        return new ScreenViewModel(
            ScreenCatalog.Boot,
            "Boot Splash",
            "Synchronizing TVA workspace lattice",
            [
                new PanelModel(
                    "Initialization",
                    [
                        "Loading module graph...",
                        "Hydrating telemetry channels...",
                        "Priming shell subsystem..."
                    ],
                    PanelTone.Accent)
            ],
            null,
            [],
            ["Stand by. Transition to dashboard imminent."],
            null,
            null,
            "Boot automatically advances to Home.");
    }
}
