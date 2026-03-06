using Tva.Application;
using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Modules;

public sealed class AlertsModule : IAppModule
{
    public AppModuleInfo Metadata { get; } = new(
        "alerts",
        "Alerts",
        "Warning and critical signal feed.",
        "[A]");

    public IReadOnlyList<NavigationEntry> NavigationEntries { get; } =
    [
        new("alerts", ScreenCatalog.Alerts, "Alerts", "2", 20)
    ];

    public IReadOnlyList<IScreenProvider> Screens { get; } =
    [
        new AlertsScreenProvider()
    ];

    public IReadOnlyList<PanelModel> GetDashboardPanels(AppSessionState state)
    {
        var critical = state.Alerts.Count(alert => alert.Severity == SeverityLevel.Critical);
        var warning = state.Alerts.Count(alert => alert.Severity == SeverityLevel.Warning);

        return
        [
            new PanelModel(
                "Alert Watch",
                [
                    $"Critical: {critical}",
                    $"Warning: {warning}",
                    $"Latest: {state.Alerts.FirstOrDefault()?.Message ?? "none"}"
                ],
                critical > 0 ? PanelTone.Critical : PanelTone.Warning)
        ];
    }

    public IReadOnlyList<StatusItem> GetStatusItems(AppSessionState state)
    {
        var critical = state.Alerts.Count(alert => alert.Severity == SeverityLevel.Critical);
        var severity = critical > 0 ? SeverityLevel.Critical : SeverityLevel.Info;
        return
        [
            new StatusItem("Critical", critical.ToString(), severity)
        ];
    }

    private sealed class AlertsScreenProvider : IScreenProvider
    {
        public ScreenId ScreenId => ScreenCatalog.Alerts;

        public string ModuleId => "alerts";

        public ScreenViewModel Build(AppSessionState state)
        {
            var rows = state.Alerts
                .Take(24)
                .Select(alert => (IReadOnlyList<string>)
                [
                    alert.Timestamp.ToString("HH:mm:ss"),
                    alert.Source,
                    alert.Severity.ToString(),
                    alert.Message
                ])
                .ToList();

            return new ScreenViewModel(
                ScreenCatalog.Alerts,
                "Alerts",
                "Synthetic operational alerts",
                [],
                new TableModel("Alert Feed", ["Time", "Source", "Severity", "Message"], rows),
                state.Alerts.Take(8).ToList(),
                [],
                null,
                null,
                "Use Home screen for aggregate summary.");
        }
    }
}
