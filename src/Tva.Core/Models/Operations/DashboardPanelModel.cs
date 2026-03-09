namespace Tva.Core;

public sealed record DashboardPanelModel(
    string Key,
    DashboardRegion Region,
    int Order,
    PanelModel Panel);
