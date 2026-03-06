using Tva.Contracts;
using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Modules;

public sealed class DashboardModule : IModule
{
    public string Id => "dashboard";

    public string DisplayName => "Dashboard";

    public void Register(IModuleContext context)
    {
        context.RegisterNavigation(new NavigationEntry(Id, ScreenCatalog.Home, "Home", "1", 10));
        context.RegisterScreen(ScreenCatalog.Home, new HomeScreenProvider());

        context.RegisterDashboardPanel(state =>
            new PanelModel(
                "Workspace",
                [
                    $"Active screen: {state.ActiveScreenId.Value}",
                    $"Events buffered: {state.Events.Count}",
                    $"Wave samples: {state.WaveformSamples.Count}"
                ],
                PanelTone.Accent));

        context.RegisterStatusItem(_ => new StatusItem("Mode", "Prototype"));
    }

    private sealed class HomeScreenProvider : IScreenProvider
    {
        public ScreenViewModel Create(IModuleState state)
        {
            var rows = new List<IReadOnlyList<string>>
            {
                new[] { "Alerts", state.Alerts.Count.ToString() },
                new[] { "Events", state.Events.Count.ToString() },
                new[] { "Terminal lines", state.Terminal.Output.Count.ToString() },
                new[] { "Waveform points", state.WaveformSamples.Count.ToString() }
            };

            return new ScreenViewModel(
                ScreenCatalog.Home,
                "Home Dashboard",
                "TVA-inspired workspace overview",
                state.DashboardPanels.Take(6).ToList(),
                new TableModel("System Snapshot", ["Metric", "Value"], rows),
                state.Alerts.Take(3).ToList(),
                [],
                null,
                null,
                "[Tab] cycle screens, [W] warning modal, [Q] quit.");
        }
    }
}
