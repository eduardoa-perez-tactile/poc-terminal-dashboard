using Tva.Application;
using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Modules;

public sealed class DashboardModule : IAppModule
{
    public AppModuleInfo Metadata { get; } = new(
        "dashboard",
        "Dashboard",
        "Landing workspace view with module summaries.",
        "[H]");

    public IReadOnlyList<NavigationEntry> NavigationEntries { get; } =
    [
        new("dashboard", ScreenCatalog.Home, "Home", "1", 10)
    ];

    public IReadOnlyList<IScreenProvider> Screens { get; } =
    [
        new HomeScreenProvider()
    ];

    public IReadOnlyList<PanelModel> GetDashboardPanels(AppSessionState state)
    {
        return
        [
            new PanelModel(
                "Workspace",
                [
                    $"Active screen: {state.ActiveScreenId.Value}",
                    $"Events buffered: {state.Events.Count}",
                    $"Wave samples: {state.WaveformSamples.Count}"
                ],
                PanelTone.Accent)
        ];
    }

    public IReadOnlyList<StatusItem> GetStatusItems(AppSessionState state)
    {
        return
        [
            new StatusItem("Mode", "Prototype")
        ];
    }

    private sealed class HomeScreenProvider : IScreenProvider
    {
        public ScreenId ScreenId => ScreenCatalog.Home;

        public string ModuleId => "dashboard";

        public ScreenViewModel Build(AppSessionState state)
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
