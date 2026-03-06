using Spectre.Console;
using Spectre.Console.Rendering;
using System.Text;
using Tva.Core;
using Tva.Theme;

namespace Tva.Tui.Rendering;

public sealed class SpectreWorkspaceRenderer
{
    public IRenderable Render(AppShellModel shell)
    {
        var layout = new Layout("root")
            .SplitRows(
                new Layout("header").Size(3),
                new Layout("body"),
                new Layout("footer").Size(5));

        layout["body"].SplitColumns(
            new Layout("nav").Size(30),
            new Layout("content"),
            new Layout("rail").Size(36));

        layout["header"].Update(BuildHeader(shell));
        layout["nav"].Update(BuildNavigation(shell));
        layout["content"].Update(BuildContent(shell));
        layout["rail"].Update(BuildRail(shell));
        layout["footer"].Update(BuildFooter(shell));

        return layout;
    }

    private static IRenderable BuildHeader(AppShellModel shell)
    {
        var clock = GetStatusValue(shell, "Clock", "--:--:--");
        var table = new Table
        {
            Border = TableBorder.None,
            Expand = true
        };

        table.HideHeaders();
        table.AddColumn(string.Empty);
        table.AddColumn(string.Empty);
        table.AddColumn(string.Empty);
        table.AddRow(
            new Markup($"[{shell.Theme.Colors.Header}]{shell.Theme.Frame.HeaderPrefix} {Markup.Escape(ToSignalLabel(shell.AppTitle))}_V2_0[/]"),
            new Markup(
                $"[{shell.Theme.Colors.Muted}]BOOT_STATUS:[/] [{shell.Theme.Colors.Accent}]NOMINAL[/] " +
                $"[{shell.Theme.Colors.Muted}]AUTH:[/] [{shell.Theme.Colors.Highlight}]LVL_7_OVERRIDE[/]"),
            Align.Right(
                new Markup(
                    $"[{shell.Theme.Colors.Header}]TEMPORAL_AURA:[/] [{shell.Theme.Colors.Highlight}]VALID[/] " +
                    $"[{shell.Theme.Colors.Accent}]{Markup.Escape(clock)}[/]")));

        var panel = CreateFramePanel(shell, table);
        panel.Header = null;
        return panel;
    }

    private static IRenderable BuildNavigation(AppShellModel shell)
    {
        var lines = shell.Navigation.OrderBy(entry => entry.Order).Select(entry =>
        {
            var label = $"{entry.Shortcut.PadLeft(2, '0')}  {ToHeaderLabel(entry.Label)}";
            return entry.ScreenId == shell.ActiveScreenId
                ? $"[black on {shell.Theme.Colors.Accent}] {Markup.Escape(label)} [/] "
                : $"[{shell.Theme.Colors.Accent}]  {Markup.Escape(label)}[/]";
        });

        var navBody = new Rows(
            new Markup(string.Join("\n\n", lines)),
            new Text(string.Empty),
            new Markup($"[{shell.Theme.Colors.Muted}]TAB[/] cycle"),
            new Markup($"[{shell.Theme.Colors.Muted}]B[/] back  [{shell.Theme.Colors.Muted}]W[/] modal  [{shell.Theme.Colors.Muted}]Q[/] quit"));

        return new Rows(
            CreateFrame(shell, navBody, "Navigation"),
            CreateFrame(shell, BuildSystemHealth(shell), "System Health"),
            CreateFrame(
                shell,
                new Markup(
                    $"[{shell.Theme.Colors.Muted}]AGENT_LINK:[/] [{shell.Theme.Colors.Foreground}]ENCRYPTED_LINE_8[/]\n" +
                    $"[{shell.Theme.Colors.Muted}]SYNC_MODE:[/] [{shell.Theme.Colors.Highlight}]DATABASE_CONNECTED[/]"),
                "Node Link"));
    }

    private static IRenderable BuildContent(AppShellModel shell)
    {
        var parts = new List<IRenderable>();

        parts.Add(BuildScreenBanner(shell));

        if (shell.ShowWarning && !string.IsNullOrWhiteSpace(shell.WarningMessage))
        {
            parts.Add(
                CreateFrame(
                    shell,
                    new Markup($"[{shell.Theme.Colors.Warning}]TEMPORAL_WARNING:[/] {Markup.Escape(shell.WarningMessage)}"),
                    "Override",
                    titleColor: shell.Theme.Colors.Warning));
        }

        if (shell.ActiveScreen.Timeline is not null)
        {
            parts.Add(BuildTimeline(shell, shell.ActiveScreen.Timeline));
        }

        if (shell.ActiveScreen.Panels.Count > 0)
        {
            var panelRenderables = shell.ActiveScreen.Panels.Select(panelModel => BuildPanel(shell, panelModel)).ToArray();
            parts.Add(new Columns(panelRenderables) { Expand = true });
        }

        if (shell.ActiveScreen.Terminal is not null)
        {
            parts.Add(BuildTerminal(shell, shell.ActiveScreen.Terminal));
        }

        if (shell.ActiveScreen.Table is not null)
        {
            parts.Add(BuildTable(shell, shell.ActiveScreen.Table));
        }

        if (shell.ActiveScreen.Alerts.Count > 0)
        {
            parts.Add(BuildAlerts(shell, shell.ActiveScreen.Alerts));
        }

        if (shell.ActiveScreen.LogLines.Count > 0)
        {
            var logLines = shell.ActiveScreen.LogLines.Take(ThemeConventions.MaxEventLines).Select(line =>
                $"[{shell.Theme.Colors.Foreground}]{Markup.Escape(line)}[/]");
            parts.Add(CreateFrame(shell, new Markup(string.Join("\n", logLines)), "Logs"));
        }

        if (!string.IsNullOrWhiteSpace(shell.ActiveScreen.Hint))
        {
            parts.Add(
                CreateFrame(
                    shell,
                    new Markup($"[{shell.Theme.Colors.Muted}]OP_NOTE:[/] {Markup.Escape(shell.ActiveScreen.Hint)}"),
                    "Operator Note",
                    titleColor: shell.Theme.Colors.Muted));
        }

        return new Rows(parts.ToArray());
    }

    private static IRenderable BuildPanel(AppShellModel shell, PanelModel panelModel)
    {
        var color = panelModel.Tone switch
        {
            PanelTone.Accent => shell.Theme.Colors.Accent,
            PanelTone.Warning => shell.Theme.Colors.Warning,
            PanelTone.Critical => shell.Theme.Colors.Critical,
            _ => shell.Theme.Colors.Foreground
        };

        var text = string.Join(
            "\n",
            panelModel.Lines.Select(line => $"[{color}]{Markup.Escape(line)}[/]"));

        return CreateFrame(shell, new Markup(text), panelModel.Title, titleColor: color);
    }

    private static IRenderable BuildTable(AppShellModel shell, TableModel tableModel)
    {
        var table = new Table
        {
            Border = TableBorder.Square,
            Title = CreateTableTitle(shell, ToHeaderLabel(tableModel.Title), shell.Theme.Colors.Highlight),
            Expand = true
        };
        table.BorderStyle = CreateStyle(shell.Theme.Colors.Border);

        foreach (var column in tableModel.Columns)
        {
            table.AddColumn(new TableColumn($"[{shell.Theme.Colors.Accent}]{Markup.Escape(ToHeaderLabel(column))}[/]"));
        }

        foreach (var row in tableModel.Rows)
        {
            table.AddRow(row.Select(cell => $"[{shell.Theme.Colors.Foreground}]{Markup.Escape(cell)}[/]").ToArray());
        }

        return table;
    }

    private static IRenderable BuildAlerts(AppShellModel shell, IReadOnlyList<AlertModel> alerts)
    {
        var table = new Table
        {
            Border = TableBorder.Square,
            Title = CreateTableTitle(shell, "ACTIVE TICKETS", shell.Theme.Colors.Warning),
            Expand = true
        };
        table.BorderStyle = CreateStyle(shell.Theme.Colors.Border);

        table.AddColumn(new TableColumn($"[{shell.Theme.Colors.Accent}]TIME[/]"));
        table.AddColumn(new TableColumn($"[{shell.Theme.Colors.Accent}]SOURCE[/]"));
        table.AddColumn(new TableColumn($"[{shell.Theme.Colors.Accent}]MESSAGE[/]"));

        foreach (var alert in alerts.Take(8))
        {
            var color = ColorForSeverity(shell, alert.Severity);

            table.AddRow(
                $"[{shell.Theme.Colors.Muted}]{alert.Timestamp:HH:mm:ss}[/]",
                $"[{shell.Theme.Colors.Foreground}]{Markup.Escape(ToHeaderLabel(alert.Source))}[/]",
                $"[{color}]{Markup.Escape(alert.Message)}[/]");
        }

        return table;
    }

    private static IRenderable BuildTimeline(AppShellModel shell, TimelineModel timeline)
    {
        var sparkline = BuildSparkline(timeline.Samples, timeline.Min, timeline.Max, 72);
        var meta = new Table
        {
            Border = TableBorder.None,
            Expand = true
        };

        meta.HideHeaders();
        meta.AddColumn(string.Empty);
        meta.AddColumn(string.Empty);
        meta.AddRow(
            new Markup($"[{shell.Theme.Colors.Muted}]LIVE_VIEW:[/] [{shell.Theme.Colors.Accent}]{timeline.Samples.Count}_SAMPLES[/]"),
            Align.Right(new Markup($"[{shell.Theme.Colors.Muted}]CURRENT_T_INDEX:[/] [{shell.Theme.Colors.Accent}]{timeline.Samples.LastOrDefault():000}[/]")));
        meta.AddRow(
            new Markup($"[{shell.Theme.Colors.Accent}]{Markup.Escape(sparkline)}[/]"),
            new Text(string.Empty));
        meta.AddRow(
            new Markup($"[{shell.Theme.Colors.Muted}]T_MINUS_24H[/]"),
            Align.Right(new Markup($"[{shell.Theme.Colors.Muted}]MIN:[/] {timeline.Min}  [{shell.Theme.Colors.Muted}]MAX:[/] {timeline.Max}")));

        return CreateFrame(shell, meta, timeline.Title);
    }

    private static IRenderable BuildTerminal(AppShellModel shell, TerminalViewModel terminal)
    {
        var outputTable = new Table
        {
            Border = TableBorder.Square,
            Expand = true
        };
        outputTable.AddColumn(new TableColumn($"[{shell.Theme.Colors.Accent}]Time[/]"));
        outputTable.AddColumn(new TableColumn($"[{shell.Theme.Colors.Accent}]Output[/]"));

        foreach (var chunk in terminal.Output.TakeLast(ThemeConventions.MaxTerminalLines))
        {
            var color = chunk.IsError ? shell.Theme.Colors.Critical : shell.Theme.Colors.Foreground;
            outputTable.AddRow(
                chunk.Timestamp.ToString("HH:mm:ss"),
                $"[{color}]{Markup.Escape(chunk.Text)}[/]");
        }

        var status = terminal.IsRunning
            ? $"[{shell.Theme.Colors.Warning}]Running: {Markup.Escape(terminal.ActiveCommand ?? "unknown")}[/]"
            : $"[{shell.Theme.Colors.Muted}]Idle (last exit: {terminal.LastExitCode?.ToString() ?? "-"})[/]";

        var input = new Markup($"[{shell.Theme.Colors.Highlight}]{Markup.Escape(terminal.Prompt)}[/] {Markup.Escape(terminal.InputBuffer)}");

        var content = new Rows(outputTable, new Rows(new Markup(status), input));
        return CreateFrame(shell, content, "AI Process Manager");
    }

    private static IRenderable BuildRail(AppShellModel shell)
    {
        return new Rows(
            BuildTicketRail(shell),
            BuildWatchPanel(shell));
    }

    private static IRenderable BuildFooter(AppShellModel shell)
    {
        var feedMessage = shell.ActiveNotification is not null
            ? $"{shell.ActiveNotification.CreatedAt:HH:mm:ss} :: {shell.ActiveNotification.Message}"
            : shell.RecentAlerts.FirstOrDefault() is { } alert
                ? $"{alert.Timestamp:HH:mm:ss} :: {alert.Message}"
                : "NOMINAL_FEED :: no active anomalies";

        var menu = new Table
        {
            Border = TableBorder.None,
            Expand = true
        };

        menu.HideHeaders();
        menu.AddColumn(string.Empty);
        menu.AddColumn(string.Empty);
        menu.AddRow(
            new Markup($"[{shell.Theme.Colors.Highlight}]SYSTEM_LOG_FEED[/] [{shell.Theme.Colors.Foreground}]{Markup.Escape(feedMessage)}[/]"),
            Align.Right(
                new Markup(
                    $"[{shell.Theme.Colors.Accent}]FILE[/]  [{shell.Theme.Colors.Accent}]EDIT[/]  [{shell.Theme.Colors.Accent}]VIEW[/]  " +
                    $"[{shell.Theme.Colors.Accent}]MODE[/]  [{shell.Theme.Colors.Accent}]HELP[/]")));
        menu.AddRow(
            new Markup(BuildStatusSummary(shell)),
            Align.Right(
                new Markup(
                    $"[{shell.Theme.Colors.Muted}]ENCRYPTED_LINE_8[/]  [{shell.Theme.Colors.Muted}]DATABASE_CONNECTED[/]")));

        return CreateFrame(shell, menu, "Status Bus", titleColor: shell.Theme.Colors.Muted);
    }

    private static IRenderable BuildScreenBanner(AppShellModel shell)
    {
        var header = new Table
        {
            Border = TableBorder.None,
            Expand = true
        };

        header.HideHeaders();
        header.AddColumn(string.Empty);
        header.AddColumn(string.Empty);
        header.AddRow(
            new Markup($"[{shell.Theme.Colors.Highlight}]{Markup.Escape(ToHeaderLabel(shell.ActiveScreen.Title))}[/]"),
            Align.Right(new Markup($"[{shell.Theme.Colors.Muted}]ACTIVE SCREEN:[/] [{shell.Theme.Colors.Accent}]{Markup.Escape(ToHeaderLabel(shell.ActiveScreenId.Value))}[/]")));
        header.AddRow(
            new Markup($"[{shell.Theme.Colors.Muted}]{Markup.Escape(ToHeaderLabel(shell.ActiveScreen.Subtitle))}[/]"),
            Align.Right(new Markup($"[{shell.Theme.Colors.Muted}]MODULES ONLINE:[/] [{shell.Theme.Colors.Accent}]{shell.Navigation.Count:00}[/]")));

        return CreateFrame(shell, header, shell.ActiveScreen.Title);
    }

    private static IRenderable BuildSystemHealth(AppShellModel shell)
    {
        var alerts = Math.Clamp(GetStatusInt(shell, "Alerts", 0), 0, 100);
        var critical = Math.Clamp(GetStatusInt(shell, "Critical", 0) * 20, 0, 100);
        var pulse = Math.Clamp(GetStatusInt(shell, "Pulse", 58), 0, 100);
        var shellBusy = string.Equals(GetStatusValue(shell, "Shell", "idle"), "busy", StringComparison.OrdinalIgnoreCase) ? 92 : 18;

        var lines = new[]
        {
            $"[{shell.Theme.Colors.Muted}]ALERT_LOAD[/]    [{shell.Theme.Colors.Foreground}]{BuildMeter(alerts, 18)}[/] [{shell.Theme.Colors.Accent}]{alerts,3}%[/]",
            $"[{shell.Theme.Colors.Muted}]CRITICAL[/]      [{shell.Theme.Colors.Foreground}]{BuildMeter(critical, 18)}[/] [{ColorForSeverity(shell, critical > 0 ? SeverityLevel.Critical : SeverityLevel.Info)}]{critical,3}%[/]",
            $"[{shell.Theme.Colors.Muted}]PULSE_SYNC[/]    [{shell.Theme.Colors.Foreground}]{BuildMeter(pulse, 18)}[/] [{shell.Theme.Colors.Accent}]{pulse,3}%[/]",
            $"[{shell.Theme.Colors.Muted}]SHELL_BUSY[/]    [{shell.Theme.Colors.Foreground}]{BuildMeter(shellBusy, 18)}[/] [{(shellBusy > 50 ? shell.Theme.Colors.Warning : shell.Theme.Colors.Highlight)}]{shellBusy,3}%[/]"
        };

        return new Markup(string.Join("\n\n", lines));
    }

    private static IRenderable BuildTicketRail(AppShellModel shell)
    {
        if (shell.RecentAlerts.Count == 0)
        {
            return CreateFrame(
                shell,
                new Markup($"[{shell.Theme.Colors.Muted}]NO_ACTIVE_TICKETS[/]"),
                "Active Tickets",
                "00 Pending");
        }

        var tickets = shell.RecentAlerts.Take(4).Select(alert =>
        {
            var color = ColorForSeverity(shell, alert.Severity);
            var body = new Markup(
                $"[{shell.Theme.Colors.Accent}]#{alert.Timestamp:HHmmss}[/]  [{color}]{Markup.Escape(alert.Severity.ToString().ToUpperInvariant())}[/]\n" +
                $"[{shell.Theme.Colors.Foreground}]{Markup.Escape(ToHeaderLabel(alert.Source))}[/]\n" +
                $"[{shell.Theme.Colors.Muted}]{Markup.Escape(alert.Message)}[/]\n" +
                $"[{shell.Theme.Colors.Muted}]STAMP:[/] [{shell.Theme.Colors.Highlight}]{alert.Timestamp:HH:mm:ss}[/]");
            return CreateFrame(shell, body, "Ticket", titleColor: color);
        });

        return CreateFrame(
            shell,
            new Rows(tickets.ToArray()),
            "Active Tickets",
            $"{shell.RecentAlerts.Count:00} Pending");
    }

    private static IRenderable BuildWatchPanel(AppShellModel shell)
    {
        var lines = shell.StatusItems.Select(item =>
        {
            var color = ColorForSeverity(shell, item.Severity);
            return $"[{shell.Theme.Colors.Muted}]{Markup.Escape(ToHeaderLabel(item.Key))}:[/] [{color}]{Markup.Escape(item.Value)}[/]";
        }).ToList();

        if (shell.ActiveNotification is not null)
        {
            var color = ColorForSeverity(shell, shell.ActiveNotification.Severity);
            lines.Add(string.Empty);
            lines.Add($"[{shell.Theme.Colors.Muted}]NOTICE:[/] [{color}]{Markup.Escape(shell.ActiveNotification.Message)}[/]");
        }

        return CreateFrame(shell, new Markup(string.Join("\n", lines)), "Watch Channel");
    }

    private static Panel CreateFrame(
        AppShellModel shell,
        IRenderable content,
        string title,
        string? badge = null,
        string? titleColor = null)
    {
        var resolvedTitleColor = titleColor ?? shell.Theme.Colors.Highlight;
        var header = $"[{resolvedTitleColor}]{Markup.Escape(ToHeaderLabel(title))}[/]";

        if (!string.IsNullOrWhiteSpace(badge))
        {
            header += $" [{shell.Theme.Colors.Muted}]::[/] [{shell.Theme.Colors.Accent}]{Markup.Escape(ToHeaderLabel(badge))}[/]";
        }

        var panel = CreateFramePanel(shell, content);
        panel.Header = new PanelHeader(header, Justify.Left);
        return panel;
    }

    private static string BuildStatusSummary(AppShellModel shell)
    {
        return string.Join(
            $" [{shell.Theme.Colors.Muted}]{ThemeConventions.Separator}[/] ",
            shell.StatusItems.Select(item =>
            {
                var color = ColorForSeverity(shell, item.Severity);
                return $"[{shell.Theme.Colors.Muted}]{Markup.Escape(ToHeaderLabel(item.Key))}:[/] [{color}]{Markup.Escape(item.Value)}[/]";
            }));
    }

    private static Panel CreateFramePanel(AppShellModel shell, IRenderable content)
    {
        return new Panel(content)
        {
            Border = BoxBorder.Square,
            BorderStyle = CreateStyle(shell.Theme.Colors.Border),
            Padding = new Padding(shell.Theme.Frame.PanelPadding, 0, shell.Theme.Frame.PanelPadding, 0)
        };
    }

    private static TableTitle CreateTableTitle(AppShellModel shell, string text, string color)
    {
        return new TableTitle($"[{color}]{Markup.Escape(text)}[/]");
    }

    private static Style CreateStyle(string color)
    {
        return new Style(foreground: ParseColor(color));
    }

    private static Color ParseColor(string hexColor)
    {
        var trimmed = hexColor.TrimStart('#');
        if (trimmed.Length != 6)
        {
            return Color.Default;
        }

        return new Color(
            Convert.ToByte(trimmed[..2], 16),
            Convert.ToByte(trimmed.Substring(2, 2), 16),
            Convert.ToByte(trimmed.Substring(4, 2), 16));
    }

    private static string GetStatusValue(AppShellModel shell, string key, string fallback)
    {
        return shell.StatusItems.FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))?.Value ?? fallback;
    }

    private static int GetStatusInt(AppShellModel shell, string key, int fallback)
    {
        return int.TryParse(GetStatusValue(shell, key, fallback.ToString()), out var value) ? value : fallback;
    }

    private static string BuildMeter(int value, int width)
    {
        var clamped = Math.Clamp(value, 0, 100);
        var filled = (int)Math.Round(clamped / 100d * width);
        return new string('█', filled) + new string('░', Math.Max(width - filled, 0));
    }

    private static string BuildSparkline(IReadOnlyList<int> samples, int min, int max, int width)
    {
        if (samples.Count == 0)
        {
            return "·";
        }

        const string ramp = "▁▂▃▄▅▆▇█";
        var builder = new StringBuilder(width);
        for (var i = 0; i < width; i++)
        {
            var sampleIndex = (int)Math.Round(i * (samples.Count - 1) / (double)Math.Max(width - 1, 1));
            var normalized = Math.Clamp(samples[sampleIndex], min, max);
            var ratio = max == min ? 0d : (normalized - min) / (double)(max - min);
            var rampIndex = (int)Math.Round(ratio * (ramp.Length - 1));
            builder.Append(ramp[rampIndex]);
        }

        return builder.ToString();
    }

    private static string ToSignalLabel(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "UNNAMED";
        }

        var builder = new StringBuilder(value.Length);
        var previousWasSeparator = false;

        foreach (var ch in value.Trim())
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToUpperInvariant(ch));
                previousWasSeparator = false;
                continue;
            }

            if (previousWasSeparator)
            {
                continue;
            }

            builder.Append('_');
            previousWasSeparator = true;
        }

        return builder.ToString().Trim('_');
    }

    private static string ToHeaderLabel(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "UNNAMED";
        }

        var builder = new StringBuilder(value.Length);
        var previousWasSpace = false;

        foreach (var ch in value.Trim())
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToUpperInvariant(ch));
                previousWasSpace = false;
                continue;
            }

            if (previousWasSpace)
            {
                continue;
            }

            builder.Append(' ');
            previousWasSpace = true;
        }

        return builder.ToString().Trim();
    }

    private static string ColorForSeverity(AppShellModel shell, SeverityLevel severity)
    {
        return severity switch
        {
            SeverityLevel.Warning => shell.Theme.Colors.Warning,
            SeverityLevel.Critical => shell.Theme.Colors.Critical,
            _ => shell.Theme.Colors.Foreground
        };
    }
}
