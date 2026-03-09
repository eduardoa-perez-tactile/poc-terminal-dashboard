using Tva.Core;

namespace Tva.Contracts;

public interface IModuleState
    : IWorkQueueState,
      ICodingSessionState,
      IChangeDeliveryState,
      IReviewQueueState,
      ICommunicationsState,
      IWorkLogState,
      IReadingQueueState
{
    ScreenId ActiveScreenId { get; }
    DateTimeOffset Now { get; }
    IReadOnlyList<AlertModel> Alerts { get; }
    IReadOnlyList<DashboardPanelModel> DashboardPanels { get; }
    ITerminalState Terminal { get; }
}
