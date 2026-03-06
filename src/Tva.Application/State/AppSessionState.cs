using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Application;

public sealed class AppSessionState
{
    public ScreenId ActiveScreenId { get; set; } = ScreenCatalog.Boot;
    public Stack<ScreenId> BackStack { get; } = new();
    public List<AppNotification> Notifications { get; } = [];
    public List<AlertModel> Alerts { get; } = [];
    public List<string> Events { get; } = [];
    public List<int> WaveformSamples { get; } = [];
    public List<PanelModel> DashboardPanels { get; } = [];
    public TerminalSessionState Terminal { get; } = new();

    public DateTimeOffset Now { get; set; }
    public int TickCount { get; set; }
    public bool ShowWarning { get; set; }
    public string? WarningMessage { get; set; }
}
