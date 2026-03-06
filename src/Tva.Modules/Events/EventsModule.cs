using Tva.Application;
using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Modules;

public sealed class EventsModule : IAppModule
{
    public AppModuleInfo Metadata { get; } = new(
        "events",
        "Events",
        "Rolling event and log stream.",
        "[E]");

    public IReadOnlyList<NavigationEntry> NavigationEntries { get; } =
    [
        new("events", ScreenCatalog.Events, "Events", "3", 30)
    ];

    public IReadOnlyList<IScreenProvider> Screens { get; } =
    [
        new EventsScreenProvider()
    ];

    public IReadOnlyList<PanelModel> GetDashboardPanels(AppSessionState state)
    {
        return
        [
            new PanelModel(
                "Event Bus",
                [
                    $"Buffered events: {state.Events.Count}",
                    $"Recent: {state.Events.FirstOrDefault() ?? "none"}"
                ])
        ];
    }

    public IReadOnlyList<StatusItem> GetStatusItems(AppSessionState state)
    {
        return
        [
            new StatusItem("Events", state.Events.Count.ToString())
        ];
    }

    private sealed class EventsScreenProvider : IScreenProvider
    {
        public ScreenId ScreenId => ScreenCatalog.Events;

        public string ModuleId => "events";

        public ScreenViewModel Build(AppSessionState state)
        {
            var rows = state.Events
                .Take(30)
                .Select((line, index) => (IReadOnlyList<string>)
                [
                    index.ToString("000"),
                    line
                ])
                .ToList();

            return new ScreenViewModel(
                ScreenCatalog.Events,
                "Events / Logs",
                "Rolling synthetic activity stream",
                [],
                new TableModel("Event Stream", ["#", "Message"], rows),
                [],
                state.Events.Take(12).ToList(),
                null,
                null,
                "Feed updates via the application tick loop.");
        }
    }
}
