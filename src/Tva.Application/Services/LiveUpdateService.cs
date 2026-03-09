using Tva.Contracts;
using Tva.Core;

namespace Tva.Application;

public sealed class LiveUpdateService
{
    private readonly IClock _clock;
    private readonly IWorkTrackerDataSource _workTracker;
    private readonly ICodingActivityDataSource _codingActivity;
    private readonly IChangeDeliveryDataSource _changeDelivery;
    private readonly IReviewQueueDataSource _reviewQueue;
    private readonly ICommunicationsDataSource _communications;
    private readonly IWorkLogDataSource _workLog;
    private readonly IReadingListDataSource _readingList;

    public LiveUpdateService(
        IClock clock,
        IWorkTrackerDataSource workTracker,
        ICodingActivityDataSource codingActivity,
        IChangeDeliveryDataSource changeDelivery,
        IReviewQueueDataSource reviewQueue,
        ICommunicationsDataSource communications,
        IWorkLogDataSource workLog,
        IReadingListDataSource readingList)
    {
        _clock = clock;
        _workTracker = workTracker;
        _codingActivity = codingActivity;
        _changeDelivery = changeDelivery;
        _reviewQueue = reviewQueue;
        _communications = communications;
        _workLog = workLog;
        _readingList = readingList;
    }

    public void Initialize(ISessionState state)
    {
        state.Now = _clock.UtcNow.ToLocalTime();
        Refresh(state);
        state.Notifications.Add(new AppNotification("Workspace datasources initialized.", SeverityLevel.Info, state.Now));
    }

    public void Tick(ISessionState state)
    {
        state.TickCount++;
        state.Now = _clock.UtcNow.ToLocalTime();
        if (state.TickCount % 8 == 0)
        {
            Refresh(state);
        }

        if (state.Notifications.Count > 12)
        {
            state.Notifications.RemoveRange(0, state.Notifications.Count - 12);
        }
    }

    private void Refresh(ISessionState state)
    {
        state.WorkQueue = _workTracker.GetSnapshot(state.Now);
        state.CodingSession = _codingActivity.GetSnapshot(state.Now);
        state.ChangeDelivery = _changeDelivery.GetSnapshot(state.Now);
        state.ReviewQueue = _reviewQueue.GetSnapshot(state.Now);
        state.Communications = _communications.GetSnapshot(state.Now);
        state.WorkLog = _workLog.GetSnapshot(state.Now);
        state.ReadingQueue = _readingList.GetSnapshot(state.Now);

        RebuildAlerts(state);
    }

    private static void RebuildAlerts(ISessionState state)
    {
        state.Alerts.Clear();

        foreach (var blocked in state.WorkQueue.BlockedItems)
        {
            state.Alerts.Add(new AlertModel("Jira", $"{blocked.Id} is blocked: {blocked.Title}", SeverityLevel.Warning, blocked.UpdatedAt));
        }

        foreach (var blocker in state.ChangeDelivery.Blockers)
        {
            state.Alerts.Add(new AlertModel("Delivery", blocker.Reason, blocker.Severity, state.Now));
        }

        foreach (var review in state.ReviewQueue.UrgentReviews)
        {
            state.Alerts.Add(new AlertModel("Reviews", $"{review.Id} is waiting for review", SeverityLevel.Warning, review.UpdatedAt));
        }

        foreach (var mail in state.Communications.UnreadMail.Where(static item => item.RequiresAction))
        {
            state.Alerts.Add(new AlertModel("Mail", mail.Subject, SeverityLevel.Info, mail.ReceivedAt));
        }

        var nextMeeting = state.Communications.UpcomingMeetings
            .OrderBy(static meeting => meeting.StartsAt)
            .FirstOrDefault();

        if (nextMeeting is not null && nextMeeting.StartsAt <= state.Now.AddMinutes(30))
        {
            state.Alerts.Add(new AlertModel("Meetings", $"{nextMeeting.Title} starts at {nextMeeting.StartsAt:HH:mm}", SeverityLevel.Warning, state.Now));
        }

        if (state.Alerts.Count > 12)
        {
            state.Alerts.RemoveRange(12, state.Alerts.Count - 12);
        }
    }
}
