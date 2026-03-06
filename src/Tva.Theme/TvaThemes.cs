using Tva.Core;

namespace Tva.Theme;

public static class TvaThemes
{
    public static AppTheme AmberCrt { get; } = new(
        "TVA Amber CRT",
        new ThemeColorTokens(
            Background: "#090804",
            Foreground: "#ffcf73",
            Accent: "#ffb347",
            Warning: "#ffd166",
            Critical: "#ff6b35",
            Muted: "#a38a52",
            Border: "#b8933a",
            Highlight: "#fff2c7",
            Header: "#ffe7a5"),
        new ThemeFrameTokens(
            PanelPadding: 1,
            HeaderPrefix: ">>",
            SectionPrefix: "::",
            BulletPrefix: "-"));
}
