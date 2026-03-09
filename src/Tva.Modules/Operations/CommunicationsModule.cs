using Tva.Contracts;
using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Modules;

public sealed class CommunicationsModule : IModule
{
    public string Id => "communications";

    public string DisplayName => "Communications";

    public void Register(IModuleContext context)
    {
        context.RegisterNavigation(new NavigationEntry(Id, ScreenCatalog.Communications, "Comms", "7", 70));
        context.RegisterScreen(ScreenCatalog.Communications, new CommunicationsScreenProvider());

        context.RegisterDashboardPanel(
            "communications",
            DashboardRegion.RailTop,
            50,
            state =>
            {
                var nextMeeting = state.Communications.UpcomingMeetings.OrderBy(static meeting => meeting.StartsAt).FirstOrDefault();
                return new PanelModel(
                    "Inbox & Meetings",
                    [
                        $"Mail: {state.Communications.UnreadMail.Count}",
                        $"Next: {nextMeeting?.Title ?? "none"}",
                        $"At: {nextMeeting?.StartsAt.ToString("HH:mm") ?? "--:--"}"
                    ],
                    nextMeeting is not null && nextMeeting.StartsAt <= state.Now.AddMinutes(30) ? PanelTone.Warning : PanelTone.Normal);
            });

        context.RegisterStatusItem(state =>
            new StatusItem(
                "Mail",
                state.Communications.UnreadMail.Count.ToString(),
                state.Communications.UnreadMail.Any(static mail => mail.RequiresAction) ? SeverityLevel.Warning : SeverityLevel.Info));
    }

    private sealed class CommunicationsScreenProvider : IScreenProvider
    {
        public ScreenViewModel Create(IModuleState state)
        {
            var nextMeeting = state.Communications.UpcomingMeetings.OrderBy(static meeting => meeting.StartsAt).FirstOrDefault();
            var rows = state.Communications.UnreadMail
                .Select(mail => (IReadOnlyList<string>)
                [
                    mail.Sender,
                    mail.Subject,
                    mail.RequiresAction ? "yes" : "no",
                    mail.ReceivedAt.ToString("HH:mm")
                ])
                .ToList();

            return new ScreenViewModel(
                ScreenCatalog.Communications,
                "Communications",
                "Email triage and meeting visibility",
                [
                    new PanelModel(
                        "Unread / Action Needed Mail",
                        state.Communications.UnreadMail.Select(mail => $"{mail.Sender}: {mail.Subject}").DefaultIfEmpty("Inbox is clear.").ToList()),
                    new PanelModel(
                        "Next Meetings",
                        state.Communications.UpcomingMeetings.Select(meeting => $"{meeting.StartsAt:HH:mm} {meeting.Title}").DefaultIfEmpty("No upcoming meetings.").ToList(),
                        nextMeeting is not null && nextMeeting.StartsAt <= state.Now.AddMinutes(30) ? PanelTone.Warning : PanelTone.Normal),
                    new PanelModel(
                        "Meeting Prep",
                        [nextMeeting?.Notes ?? "No immediate prep notes."])
                ],
                new TableModel("Mail Queue", ["Sender", "Subject", "Action", "Received"], rows),
                state.Alerts.Where(alert => alert.Source is "Mail" or "Meetings").Take(4).ToList(),
                [],
                null,
                null,
                "Use this to protect coding blocks against inbox and calendar drift.");
        }
    }
}
