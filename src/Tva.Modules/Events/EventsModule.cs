using Tva.Contracts;
using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Modules;

public sealed class EventsModule : IModule
{
    public string Id => "events";

    public string DisplayName => "Events";

    public void Register(IModuleContext context)
    {
        context.RegisterNavigation(new NavigationEntry(Id, ScreenCatalog.Events, "Events", "3", 30));
        context.RegisterScreen(ScreenCatalog.Events, new EventsScreenProvider());

        context.RegisterDashboardPanel(state =>
            new PanelModel(
                "Event Bus",
                [
                    $"Buffered events: {state.Events.Count}",
                    $"Recent: {state.Events.FirstOrDefault() ?? "none"}"
                ]));

        context.RegisterStatusItem(state => new StatusItem("Events", state.Events.Count.ToString()));
    }

    private sealed class EventsScreenProvider : IScreenProvider
    {
        public ScreenViewModel Create(IModuleState state)
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
