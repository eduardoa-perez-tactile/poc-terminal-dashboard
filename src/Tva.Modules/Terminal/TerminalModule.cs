using Tva.Contracts;
using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Modules;

public sealed class TerminalModule : IModule
{
    public string Id => "terminal";

    public string DisplayName => "Terminal";

    public void Register(IModuleContext context)
    {
        context.RegisterNavigation(new NavigationEntry(Id, ScreenCatalog.Terminal, "Terminal", "4", 40));
        context.RegisterScreen(ScreenCatalog.Terminal, new TerminalScreenProvider());

        context.RegisterDashboardPanel(state =>
            new PanelModel(
                "Shell",
                [
                    $"Running: {(state.Terminal.IsRunning ? "yes" : "no")}",
                    $"Last exit: {state.Terminal.LastExitCode?.ToString() ?? "-"}",
                    $"History size: {state.Terminal.History.Count}"
                ],
                state.Terminal.IsRunning ? PanelTone.Warning : PanelTone.Normal));

        context.RegisterStatusItem(state =>
            new StatusItem(
                "Shell",
                state.Terminal.IsRunning ? "busy" : "idle",
                state.Terminal.IsRunning ? SeverityLevel.Warning : SeverityLevel.Info));
    }

    private sealed class TerminalScreenProvider : IScreenProvider
    {
        public ScreenViewModel Create(IModuleState state)
        {
            var terminalVm = new TerminalViewModel(
                "$",
                state.Terminal.InputBuffer,
                state.Terminal.IsRunning,
                state.Terminal.ActiveCommand,
                state.Terminal.LastExitCode,
                state.Terminal.History,
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
