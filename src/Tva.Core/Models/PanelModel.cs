namespace Tva.Core;

public sealed record PanelModel(
    string Title,
    IReadOnlyList<string> Lines,
    PanelTone Tone = PanelTone.Normal);
