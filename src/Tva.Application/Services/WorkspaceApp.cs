using Tva.Contracts;
using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Application;

public sealed class WorkspaceApp
{
    private readonly IModuleCatalog _moduleCatalog;
    private readonly INavigationService _navigation;
    private readonly ISessionState _state;
    private readonly INotificationService _notifications;
    private readonly TerminalCommandService _terminalCommands;
    private readonly LiveUpdateService _liveUpdates;
    private readonly AppTheme _theme;

    public WorkspaceApp(
        IModuleCatalog moduleCatalog,
        INavigationService navigation,
        ISessionState state,
        INotificationService notifications,
        TerminalCommandService terminalCommands,
        LiveUpdateService liveUpdates,
        AppTheme theme)
    {
        _moduleCatalog = moduleCatalog;
        _navigation = navigation;
        _state = state;
        _notifications = notifications;
        _terminalCommands = terminalCommands;
        _liveUpdates = liveUpdates;
        _theme = theme;
    }

    public ScreenId ActiveScreenId => _navigation.ActiveScreenId;

    public bool IsTerminalActive => _state.ActiveScreenId == ScreenCatalog.Terminal;

    public bool WarningVisible => _state.ShowWarning;

    public void Initialize()
    {
        _liveUpdates.Initialize(_state);
        _notifications.Add("Boot sequence started.", SeverityLevel.Info);
    }

    public void Tick()
    {
        _liveUpdates.Tick(_state);

        _state.DashboardPanels.Clear();
        _state.DashboardPanels.AddRange(_moduleCatalog.BuildDashboardPanels(_state.AsModuleState()));

        if (_state.ActiveScreenId == ScreenCatalog.Boot && _state.TickCount > 10)
        {
            _navigation.NavigateTo(ScreenCatalog.Home);
            _notifications.Add("Boot complete.", SeverityLevel.Info);
        }
    }

    public AppShellModel BuildShellModel()
    {
        var activeScreen = _state.ActiveScreenId == ScreenCatalog.Boot
            ? BuildBootScreen()
            : _moduleCatalog.BuildScreen(_state.ActiveScreenId, _state.AsModuleState());

        var statusItems = new List<StatusItem>
        {
            new("Screen", _state.ActiveScreenId.Value),
            new("Clock", _state.Now.ToString("HH:mm:ss")),
            new("Alerts", _state.Alerts.Count.ToString())
        };

        statusItems.AddRange(_moduleCatalog.BuildStatusItems(_state.AsModuleState()));

        return new AppShellModel(
            "TVA Workspace Prototype",
            "Software engineer daily operations dashboard",
            _state.ActiveScreenId,
            _moduleCatalog.Navigation,
            activeScreen,
            _state.Alerts.Take(6).ToList(),
            statusItems,
            _notifications.Latest,
            _state.ShowWarning,
            _state.WarningMessage,
            _theme);
    }

    public void NavigateTo(ScreenId screenId)
    {
        _navigation.NavigateTo(screenId);
    }

    public void NavigateNext()
    {
        _navigation.NavigateNext(_moduleCatalog.Navigation);
    }

    public void NavigatePrevious()
    {
        _navigation.NavigatePrevious(_moduleCatalog.Navigation);
    }

    public void NavigateBack()
    {
        _navigation.NavigateBack();
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
        _notifications.Add(message, severity);
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
                        "Loading operational modules...",
                        "Refreshing workspace datasources...",
                        "Priming in-app terminal..."
                    ],
                    PanelTone.Accent)
            ],
            null,
            [],
            ["Stand by. Transition to the engineer dashboard is imminent."],
            null,
            null,
            "Boot automatically advances to Home.");
    }
}
