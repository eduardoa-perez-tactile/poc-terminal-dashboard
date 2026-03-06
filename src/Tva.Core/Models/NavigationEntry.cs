namespace Tva.Core;

public sealed record NavigationEntry(
    string ModuleId,
    ScreenId ScreenId,
    string Label,
    string Shortcut,
    int Order);
