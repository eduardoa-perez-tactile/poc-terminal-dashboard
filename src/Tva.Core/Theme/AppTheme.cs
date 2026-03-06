namespace Tva.Core;

public sealed record ThemeColorTokens(
    string Background,
    string Foreground,
    string Accent,
    string Warning,
    string Critical,
    string Muted,
    string Border,
    string Highlight,
    string Header);

public sealed record ThemeFrameTokens(
    int PanelPadding,
    string HeaderPrefix,
    string SectionPrefix,
    string BulletPrefix);

public sealed record AppTheme(
    string Name,
    ThemeColorTokens Colors,
    ThemeFrameTokens Frame);
