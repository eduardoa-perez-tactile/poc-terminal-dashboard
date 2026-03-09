using Tva.Contracts;
using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Application;

public sealed class AppSessionState : ISessionState, IModuleState
{
    public ScreenId ActiveScreenId { get; set; } = ScreenCatalog.Boot;
    public Stack<ScreenId> BackStack { get; } = new();
    public List<AppNotification> Notifications { get; } = [];
    public List<AlertModel> Alerts { get; } = [];
    public List<DashboardPanelModel> DashboardPanels { get; } = [];
    public TerminalSessionState Terminal { get; } = new();
    public WorkQueueModel WorkQueue { get; set; } = WorkQueueModel.Empty(DateTimeOffset.Now);
    public CodingSessionModel CodingSession { get; set; } = CodingSessionModel.Empty(DateTimeOffset.Now);
    public ChangeDeliveryModel ChangeDelivery { get; set; } = ChangeDeliveryModel.Empty(DateTimeOffset.Now);
    public ReviewQueueModel ReviewQueue { get; set; } = ReviewQueueModel.Empty(DateTimeOffset.Now);
    public CommunicationsModel Communications { get; set; } = CommunicationsModel.Empty(DateTimeOffset.Now);
    public WorkLogModel WorkLog { get; set; } = WorkLogModel.Empty(DateTimeOffset.Now);
    public ReadingQueueModel ReadingQueue { get; set; } = ReadingQueueModel.Empty(DateTimeOffset.Now);

    public DateTimeOffset Now { get; set; }
    public int TickCount { get; set; }
    public bool ShowWarning { get; set; }
    public string? WarningMessage { get; set; }

    public IModuleState AsModuleState() => this;

    IReadOnlyList<AlertModel> IModuleState.Alerts => Alerts;
    IReadOnlyList<DashboardPanelModel> IModuleState.DashboardPanels => DashboardPanels;
    ITerminalState IModuleState.Terminal => Terminal;
    WorkQueueModel IWorkQueueState.WorkQueue => WorkQueue;
    CodingSessionModel ICodingSessionState.CodingSession => CodingSession;
    ChangeDeliveryModel IChangeDeliveryState.ChangeDelivery => ChangeDelivery;
    ReviewQueueModel IReviewQueueState.ReviewQueue => ReviewQueue;
    CommunicationsModel ICommunicationsState.Communications => Communications;
    WorkLogModel IWorkLogState.WorkLog => WorkLog;
    ReadingQueueModel IReadingQueueState.ReadingQueue => ReadingQueue;
}
