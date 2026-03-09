using Tva.Contracts;
using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Modules;

public sealed class WorkQueueModule : IModule
{
    public string Id => "work-queue";

    public string DisplayName => "Work Queue";

    public void Register(IModuleContext context)
    {
        context.RegisterNavigation(new NavigationEntry(Id, ScreenCatalog.WorkQueue, "Work Queue", "2", 20));
        context.RegisterScreen(ScreenCatalog.WorkQueue, new WorkQueueScreenProvider());

        context.RegisterDashboardPanel(
            "work-queue",
            DashboardRegion.HeroLeft,
            10,
            state =>
            {
                var current = state.WorkQueue.CurrentItem;
                return new PanelModel(
                    "Today's Tickets",
                    [
                        $"Current: {current?.Id ?? "none"}",
                        $"In progress: {state.WorkQueue.InProgressItems.Count}",
                        $"Blocked: {state.WorkQueue.BlockedItems.Count}"
                    ],
                    state.WorkQueue.BlockedItems.Count > 0 ? PanelTone.Warning : PanelTone.Accent);
            });

        context.RegisterStatusItem(state =>
            new StatusItem(
                "Ticket",
                state.WorkQueue.CurrentItem?.Id ?? "-",
                state.WorkQueue.BlockedItems.Count > 0 ? SeverityLevel.Warning : SeverityLevel.Info));
    }

    private sealed class WorkQueueScreenProvider : IScreenProvider
    {
        public ScreenViewModel Create(IModuleState state)
        {
            var current = state.WorkQueue.CurrentItem;
            var rows = state.WorkQueue.AssignedItems
                .Select(item => (IReadOnlyList<string>)
                [
                    item.Id,
                    item.Title,
                    item.Priority.ToString(),
                    item.Status.ToString(),
                    item.UpdatedAt.ToString("HH:mm")
                ])
                .ToList();

            return new ScreenViewModel(
                ScreenCatalog.WorkQueue,
                "Work Queue",
                "Assigned Jira execution surface",
                [
                    new PanelModel(
                        "Current Ticket",
                        [
                            current is null ? "No active ticket selected." : $"{current.Id}: {current.Title}",
                            current?.Area is null ? "Area: -" : $"Area: {current.Area}",
                            current?.Repository is null ? "Repo: -" : $"Repo: {current.Repository}"
                        ],
                        PanelTone.Accent),
                    new PanelModel(
                        "In Progress",
                        state.WorkQueue.InProgressItems.Select(item => $"{item.Id} {item.Title}").Take(3).DefaultIfEmpty("No in-progress tickets.").ToList()),
                    new PanelModel(
                        "Blocked / Waiting",
                        state.WorkQueue.BlockedItems.Select(item => $"{item.Id} {item.Notes}").Take(3).DefaultIfEmpty("No blocked tickets.").ToList(),
                        state.WorkQueue.BlockedItems.Count > 0 ? PanelTone.Warning : PanelTone.Normal)
                ],
                new TableModel("Assigned Queue", ["Id", "Title", "Priority", "Status", "Updated"], rows),
                state.Alerts.Where(alert => alert.Source is "Jira").Take(4).ToList(),
                [],
                null,
                null,
                "Start here to decide what to work on next.");
        }
    }
}
