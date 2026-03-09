using Tva.Contracts;
using Tva.Core;

namespace Tva.Application;

public interface ISessionState
{
    ScreenId ActiveScreenId { get; set; }
    Stack<ScreenId> BackStack { get; }
    List<AppNotification> Notifications { get; }
    List<AlertModel> Alerts { get; }
    List<DashboardPanelModel> DashboardPanels { get; }
    TerminalSessionState Terminal { get; }
    WorkQueueModel WorkQueue { get; set; }
    CodingSessionModel CodingSession { get; set; }
    ChangeDeliveryModel ChangeDelivery { get; set; }
    ReviewQueueModel ReviewQueue { get; set; }
    CommunicationsModel Communications { get; set; }
    WorkLogModel WorkLog { get; set; }
    ReadingQueueModel ReadingQueue { get; set; }

    DateTimeOffset Now { get; set; }
    int TickCount { get; set; }
    bool ShowWarning { get; set; }
    string? WarningMessage { get; set; }

    IModuleState AsModuleState();
}
