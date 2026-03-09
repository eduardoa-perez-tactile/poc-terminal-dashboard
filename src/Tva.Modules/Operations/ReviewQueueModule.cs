using Tva.Contracts;
using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Modules;

public sealed class ReviewQueueModule : IModule
{
    public string Id => "reviews";

    public string DisplayName => "Reviews";

    public void Register(IModuleContext context)
    {
        context.RegisterNavigation(new NavigationEntry(Id, ScreenCatalog.Reviews, "Reviews", "6", 60));
        context.RegisterScreen(ScreenCatalog.Reviews, new ReviewQueueScreenProvider());

        context.RegisterDashboardPanel(
            "reviews",
            DashboardRegion.MainRight,
            40,
            state =>
                new PanelModel(
                    "Reviews Waiting",
                    [
                        $"Queue: {state.ReviewQueue.AwaitingReview.Count}",
                        $"Urgent: {state.ReviewQueue.UrgentReviews.Count}",
                        $"Latest: {state.ReviewQueue.AwaitingReview.FirstOrDefault()?.Id ?? "none"}"
                    ],
                    state.ReviewQueue.UrgentReviews.Count > 0 ? PanelTone.Warning : PanelTone.Normal));

        context.RegisterStatusItem(state =>
            new StatusItem(
                "Reviews",
                state.ReviewQueue.AwaitingReview.Count.ToString(),
                state.ReviewQueue.UrgentReviews.Count > 0 ? SeverityLevel.Warning : SeverityLevel.Info));
    }

    private sealed class ReviewQueueScreenProvider : IScreenProvider
    {
        public ScreenViewModel Create(IModuleState state)
        {
            var rows = state.ReviewQueue.AwaitingReview
                .Select(review => (IReadOnlyList<string>)
                [
                    review.Id,
                    review.System.ToString(),
                    review.Author,
                    review.Priority.ToString(),
                    review.Decision.ToString()
                ])
                .ToList();

            return new ScreenViewModel(
                ScreenCatalog.Reviews,
                "Review Queue",
                "Inbound pull request and code review workload",
                [
                    new PanelModel(
                        "Awaiting My Review",
                        state.ReviewQueue.AwaitingReview.Select(review => $"{review.Id} {review.Title}").DefaultIfEmpty("No pending reviews.").ToList()),
                    new PanelModel(
                        "Urgent / Aging",
                        state.ReviewQueue.UrgentReviews.Select(review => $"{review.Id} {review.Repository}").DefaultIfEmpty("No urgent reviews.").ToList(),
                        state.ReviewQueue.UrgentReviews.Count > 0 ? PanelTone.Warning : PanelTone.Normal),
                    new PanelModel(
                        "Review Notes",
                        state.ReviewQueue.RecentNotes.DefaultIfEmpty("No recent notes.").ToList())
                ],
                new TableModel("Review Queue", ["Id", "SCM", "Author", "Priority", "State"], rows),
                state.Alerts.Where(alert => alert.Source is "Reviews").Take(4).ToList(),
                [],
                null,
                null,
                "Keep outbound delivery separate from inbound review work.");
        }
    }
}
