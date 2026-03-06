namespace Tva.Core;

public sealed record AppShellModel(
    string AppTitle,
    string HeaderSubtitle,
    ScreenId ActiveScreenId,
    IReadOnlyList<NavigationEntry> Navigation,
    ScreenViewModel ActiveScreen,
    IReadOnlyList<AlertModel> RecentAlerts,
    IReadOnlyList<StatusItem> StatusItems,
    AppNotification? ActiveNotification,
    bool ShowWarning,
    string? WarningMessage,
    AppTheme Theme);
