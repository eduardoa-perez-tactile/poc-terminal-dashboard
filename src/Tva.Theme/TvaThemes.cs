using Tva.Core;

namespace Tva.Theme;

public static class TvaThemes
{
    public static AppTheme AmberCrt { get; } = new(
        "TVA Amber CRT",
        new ThemeColorTokens(
            Background: "#050200",
            Foreground: "#f6b13a",
            Accent: "#ffb000",
            Warning: "#ffd47a",
            Critical: "#ff6238",
            Muted: "#7d5613",
            Border: "#8d5d10",
            Highlight: "#ffe2a3",
            Header: "#ffbe32"),
        new ThemeFrameTokens(
            PanelPadding: 1,
            HeaderPrefix: "◉",
            SectionPrefix: ">>",
            BulletPrefix: "•"));
}
