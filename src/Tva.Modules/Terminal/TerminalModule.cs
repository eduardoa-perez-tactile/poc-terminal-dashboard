using Tva.Application;
using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Modules;

public sealed class TerminalModule : IAppModule
{
    public AppModuleInfo Metadata { get; } = new(
        "terminal",
        "Terminal",
        "Interactive shell command execution.",
        "[T]");

    public IReadOnlyList<NavigationEntry> NavigationEntries { get; } =
    [
        new("terminal", ScreenCatalog.Terminal, "Terminal", "4", 40)
    ];

    public IReadOnlyList<IScreenProvider> Screens { get; } =
    [
        new TerminalScreenProvider()
    ];

    public IReadOnlyList<PanelModel> GetDashboardPanels(AppSessionState state)
    {
        return
        [
            new PanelModel(
                "Shell",
                [
                    $"Running: {(state.Terminal.IsRunning ? "yes" : "no")}",
                    $"Last exit: {state.Terminal.LastExitCode?.ToString() ?? "-"}",
                    $"History size: {state.Terminal.History.Entries.Count}"
                ],
                state.Terminal.IsRunning ? PanelTone.Warning : PanelTone.Normal)
        ];
    }

    public IReadOnlyList<StatusItem> GetStatusItems(AppSessionState state)
    {
        return
        [
            new StatusItem(
                "Shell",
                state.Terminal.IsRunning ? "busy" : "idle",
                state.Terminal.IsRunning ? SeverityLevel.Warning : SeverityLevel.Info)
        ];
    }

    private sealed class TerminalScreenProvider : IScreenProvider
    {
        public ScreenId ScreenId => ScreenCatalog.Terminal;

        public string ModuleId => "terminal";

        public ScreenViewModel Build(AppSessionState state)
        {
            var terminalVm = new TerminalViewModel(
                "$",
                state.Terminal.InputBuffer,
                state.Terminal.IsRunning,
                state.Terminal.ActiveCommand,
                state.Terminal.LastExitCode,
                state.Terminal.History.Entries,
                state.Terminal.Output);

            return new ScreenViewModel(
                ScreenCatalog.Terminal,
                "Terminal",
                "Command execution via OS shell",
                [
                    new PanelModel(
                        "Shortcuts",
                        [
                            "[Enter] run command",
                            "[Up/Down] command history",
                            "[Backspace] edit input"
                        ],
                        PanelTone.Accent)
                ],
                null,
                [],
                [],
                null,
                terminalVm,
                "Use this screen like a lightweight iTerm-style pane.");
        }
    }
}
