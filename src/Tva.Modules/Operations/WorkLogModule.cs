using Tva.Contracts;
using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Modules;

public sealed class WorkLogModule : IModule
{
    public string Id => "work-log";

    public string DisplayName => "Work Log";

    public void Register(IModuleContext context)
    {
        context.RegisterNavigation(new NavigationEntry(Id, ScreenCatalog.WorkLog, "Work Log", "8", 80));
        context.RegisterScreen(ScreenCatalog.WorkLog, new WorkLogScreenProvider());

        context.RegisterDashboardPanel(
            "work-log",
            DashboardRegion.RailBottom,
            60,
            state =>
                new PanelModel(
                    "Daily Log",
                    [
                        $"Entries: {state.WorkLog.Entries.Count}",
                        $"Coverage: {state.WorkLog.Coverage}",
                        $"Summary: {(state.WorkLog.SummaryDraft.IsReadyToShare ? "ready" : "draft")}"
                    ],
                    state.WorkLog.Coverage == LogCoverageStatus.Clear ? PanelTone.Normal : PanelTone.Warning));

        context.RegisterStatusItem(state =>
            new StatusItem(
                "Log",
                state.WorkLog.Coverage.ToString(),
                state.WorkLog.Coverage == LogCoverageStatus.Clear ? SeverityLevel.Info : SeverityLevel.Warning));
    }

    private sealed class WorkLogScreenProvider : IScreenProvider
    {
        public ScreenViewModel Create(IModuleState state)
        {
            var rows = state.WorkLog.Entries
                .OrderByDescending(static entry => entry.Timestamp)
                .Select(entry => (IReadOnlyList<string>)
                [
                    entry.Timestamp.ToString("HH:mm"),
                    entry.Category,
                    entry.Text
                ])
                .ToList();

            return new ScreenViewModel(
                ScreenCatalog.WorkLog,
                "Work Log",
                "Daily notes and summary drafting",
                [
                    new PanelModel(
                        "Summary Draft",
                        [state.WorkLog.SummaryDraft.Summary]),
                    new PanelModel(
                        "Coverage",
                        [
                            $"Status: {state.WorkLog.Coverage}",
                            $"Entries: {state.WorkLog.Entries.Count}",
                            $"Shareable: {(state.WorkLog.SummaryDraft.IsReadyToShare ? "yes" : "no")}"
                        ],
                        state.WorkLog.Coverage == LogCoverageStatus.Clear ? PanelTone.Normal : PanelTone.Warning),
                    new PanelModel(
                        "Missing Notes",
                        state.WorkLog.Coverage == LogCoverageStatus.Clear
                            ? ["No gaps detected."]
                            : ["Capture meeting outcomes and code-review follow-ups before end of day."])
                ],
                new TableModel("Today Timeline", ["Time", "Category", "Note"], rows),
                [],
                [],
                null,
                null,
                "Keep this current so end-of-day summaries are cheap to produce.");
        }
    }
}
