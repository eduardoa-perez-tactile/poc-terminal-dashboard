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
        context.RegisterStatusItem(_ => new StatusItem("Mode", "Ops Dashboard"));
    }

    private sealed class HomeScreenProvider : IScreenProvider
    {
        public ScreenViewModel Create(IModuleState state)
        {
            var nextMeeting = state.Communications.UpcomingMeetings
                .OrderBy(static meeting => meeting.StartsAt)
                .FirstOrDefault();

            var rows = new List<IReadOnlyList<string>>
            {
                new[] { "Current Ticket", state.WorkQueue.CurrentItem?.Id ?? "-" },
                new[] { "Repository", state.CodingSession.Repository.Name },
                new[] { "Branch", state.CodingSession.Repository.Branch },
                new[] { "Ready To Open", state.ChangeDelivery.GitChanges.Count(change => change.Readiness == DeliveryReadiness.ReadyToOpen).ToString() },
                new[] { "Reviews Waiting", state.ReviewQueue.AwaitingReview.Count.ToString() },
                new[] { "Unread Mail", state.Communications.UnreadMail.Count.ToString() },
                new[] { "Next Meeting", nextMeeting?.Title ?? "none" },
                new[] { "Work Log", state.WorkLog.Coverage.ToString() }
            };

            var orderedPanels = state.DashboardPanels
                .OrderBy(static panel => panel.Region)
                .ThenBy(static panel => panel.Order)
                .Select(static panel => panel.Panel)
                .ToList();

            return new ScreenViewModel(
                ScreenCatalog.Home,
                "Engineer Dashboard",
                "Daily operations overview for delivery, reviews, communication, and logging",
                orderedPanels,
                new TableModel("Daily Snapshot", ["Metric", "Value"], rows),
                state.Alerts.Take(6).ToList(),
                [],
                null,
                null,
                "[1-8] switch screens, [Tab] cycle, [B] back, [Q] quit.");
        }
    }
}
