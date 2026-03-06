namespace Tva.Core;

public sealed record ScreenViewModel(
    ScreenId Id,
    string Title,
    string Subtitle,
    IReadOnlyList<PanelModel> Panels,
    TableModel? Table,
    IReadOnlyList<AlertModel> Alerts,
    IReadOnlyList<string> LogLines,
    TimelineModel? Timeline,
    TerminalViewModel? Terminal,
    string? Hint = null);
