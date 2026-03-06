using Tva.Contracts;
using Tva.Core;

namespace Tva.Application;

public interface ISessionState
{
    ScreenId ActiveScreenId { get; set; }
    Stack<ScreenId> BackStack { get; }
    List<AppNotification> Notifications { get; }
    List<AlertModel> Alerts { get; }
    List<string> Events { get; }
    List<int> WaveformSamples { get; }
    List<PanelModel> DashboardPanels { get; }
    TerminalSessionState Terminal { get; }

    DateTimeOffset Now { get; set; }
    int TickCount { get; set; }
    bool ShowWarning { get; set; }
    string? WarningMessage { get; set; }

    IModuleState AsModuleState();
}
