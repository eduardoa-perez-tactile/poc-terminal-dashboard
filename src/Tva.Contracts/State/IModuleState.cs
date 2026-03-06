using Tva.Core;

namespace Tva.Contracts;

public interface IModuleState
{
    ScreenId ActiveScreenId { get; }
    DateTimeOffset Now { get; }
    IReadOnlyList<AlertModel> Alerts { get; }
    IReadOnlyList<string> Events { get; }
    IReadOnlyList<int> WaveformSamples { get; }
    IReadOnlyList<PanelModel> DashboardPanels { get; }
    ITerminalState Terminal { get; }
}
