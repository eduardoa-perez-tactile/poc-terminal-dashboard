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
                new Layout("footer").Size(6));

        layout["body"].SplitColumns(
            new Layout("nav").Size(28),
            new Layout("content"),
            new Layout("rail").Size(34));

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
            var icon = entry.ScreenId.Value switch
            {
                "home" => "▣",
                "work-queue" => "◇",
                "coding" => "◫",
                "terminal" => "⌘",
                "change-delivery" => "⇡",
                "reviews" => "✓",
                "communications" => "✉",
                "work-log" => "✎",
                _ => "•"
            };
            var label = $"{icon}  {ToHeaderLabel(entry.Label)}";
            return entry.ScreenId == shell.ActiveScreenId
                ? $"[black on {shell.Theme.Colors.Accent}] {Markup.Escape(label)} [/] "
                : $"[{shell.Theme.Colors.Accent}]{entry.Shortcut.PadLeft(2, '0')}[/]  [{shell.Theme.Colors.Foreground}]{Markup.Escape(label)}[/]";
        });

        var navBody = new Rows(
            new Markup(string.Join("\n\n", lines)),
            new Text(string.Empty),
            new Markup($"[{shell.Theme.Colors.Muted}]TAB[/] cycle"),
            new Markup($"[{shell.Theme.Colors.Muted}]B[/] back  [{shell.Theme.Colors.Muted}]W[/] modal  [{shell.Theme.Colors.Muted}]Q[/] quit"));

        var layout = new Layout("nav-root");
        layout.SplitRows(
            new Layout("nav-main").Size(14),
            new Layout("nav-space"),
            new Layout("nav-health").Size(9));

        layout["nav-main"].Update(CreateFrame(shell, navBody, "Navigation"));
        layout["nav-health"].Update(CreateFrame(shell, BuildSystemHealth(shell), "System Health"));
        layout["nav-space"].Update(new Text(string.Empty));
        return layout;
    }

    private static IRenderable BuildContent(AppShellModel shell)
    {
        var layout = new Layout("content-root");
        layout.SplitRows(
            new Layout("hero").Size(13),
            new Layout("manager"));

        layout["hero"].Update(BuildHeroPanel(shell));
        layout["manager"].Update(BuildManagerPanel(shell));
        return layout;
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
            Title = CreateTableTitle(shell, "ATTENTION QUEUE", shell.Theme.Colors.Warning),
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
        var graph = BuildBranchBarField(timeline.Samples, timeline.Min, timeline.Max, 70, 7);
        var meta = new Table
        {
            Border = TableBorder.None,
            Expand = true
        };

        meta.HideHeaders();
        meta.AddColumn(string.Empty);
        meta.AddColumn(string.Empty);
        meta.AddRow(
            new Markup($"[{shell.Theme.Colors.Muted}]LIVE VIEW:[/] [{shell.Theme.Colors.Accent}]{Math.Max(timeline.Samples.Count / 5, 1)} ACTIVE BRANCHES[/]"),
            Align.Right(new Markup($"[{shell.Theme.Colors.Muted}]CURRENT T INDEX:[/] [{shell.Theme.Colors.Accent}]{timeline.Samples.LastOrDefault():000}[/]")));
        meta.AddRow(
            new Markup($"[{shell.Theme.Colors.Accent}]{Markup.Escape(graph)}[/]"),
            new Text(string.Empty));
        meta.AddRow(
            new Markup($"[{shell.Theme.Colors.Muted}]T_MINUS_24H[/]"),
            Align.Right(new Markup($"[{shell.Theme.Colors.Muted}]CURRENT INDEX:[/] [{shell.Theme.Colors.Accent}]{timeline.Samples.LastOrDefault():000}[/]")));

        return CreateFrame(shell, meta, "Deployment Branch History");
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
        return BuildTicketRail(shell);
    }

    private static IRenderable BuildFooter(AppShellModel shell)
    {
        var layout = new Layout("footer-root");
        layout.SplitRows(
            new Layout("log").Size(3),
            new Layout("menu").Size(2));

        var feedMessage = shell.ActiveNotification is not null
            ? $"{shell.ActiveNotification.CreatedAt:HH:mm:ss}  AUTH_SUCCESS: {shell.ActiveNotification.Message}"
            : shell.RecentAlerts.FirstOrDefault() is { } alert
                ? $"{alert.Timestamp:HH:mm:ss}  WARN: {alert.Message}"
                : "NOMINAL FEED: no active anomalies";

        var logStrip = new Table
        {
            Border = TableBorder.None,
            Expand = true
        };

        logStrip.HideHeaders();
        logStrip.AddColumn(string.Empty);
        logStrip.AddRow(new Markup($"[{shell.Theme.Colors.Foreground}]{Markup.Escape(feedMessage)}[/]"));
        layout["log"].Update(CreateFrame(shell, logStrip, "System Log Feed", titleColor: shell.Theme.Colors.Warning));

        var menu = new Table
        {
            Border = TableBorder.None,
            Expand = true
        };

        menu.HideHeaders();
        menu.AddColumn(string.Empty);
        menu.AddColumn(string.Empty);
        menu.AddRow(
            new Markup($"[{shell.Theme.Colors.Accent}]FILE[/]   [{shell.Theme.Colors.Accent}]EDIT[/]   [{shell.Theme.Colors.Accent}]VIEW[/]   [{shell.Theme.Colors.Accent}]MODE[/]   [{shell.Theme.Colors.Accent}]HELP[/]"),
            Align.Right(new Markup($"[{shell.Theme.Colors.Muted}]⌁ ENCRYPTED_LINE_8[/]   [{shell.Theme.Colors.Muted}]▤ DATABASE_CONNECTED[/]")));
        layout["menu"].Update(menu);

        return layout;
    }

    private static IRenderable BuildSystemHealth(AppShellModel shell)
    {
        var alerts = Math.Clamp(GetStatusInt(shell, "Alerts", 0), 0, 100);
        var reviews = Math.Clamp(GetStatusInt(shell, "Reviews", 0) * 20, 0, 100);
        var ready = Math.Clamp(GetStatusInt(shell, "Ready", 0) * 20, 0, 100);

        var lines = new[]
        {
            $"[{shell.Theme.Colors.Muted}]ALERT LOAD[/]\n[{shell.Theme.Colors.Accent}]{BuildMeter(alerts, 16)}[/] [{shell.Theme.Colors.Highlight}]{alerts,3}%[/]",
            $"[{shell.Theme.Colors.Muted}]REVIEWS[/]\n[{shell.Theme.Colors.Accent}]{BuildMeter(reviews, 16)}[/] [{shell.Theme.Colors.Highlight}]{reviews,3}%[/]",
            $"[{shell.Theme.Colors.Muted}]READY TO SHIP[/]\n[{shell.Theme.Colors.Accent}]{BuildMeter(ready, 16)}[/] [{ColorForSeverity(shell, ready > 0 ? SeverityLevel.Info : SeverityLevel.Warning)}]{ready,3}%[/]"
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
                "Attention Queue",
                "00 Pending");
        }

        var tickets = shell.RecentAlerts.Take(4).Select(alert =>
        {
            var color = ColorForSeverity(shell, alert.Severity);
            return (IRenderable)new Markup(
                $"[{shell.Theme.Colors.Accent}]#{alert.Timestamp:HHmmss}[/]                                            [{color}]{Markup.Escape(alert.Severity.ToString().ToUpperInvariant())}[/]\n" +
                $"[{shell.Theme.Colors.Foreground}]{Markup.Escape(ToSignalLabel(alert.Message))}[/]\n" +
                $"[{shell.Theme.Colors.Muted}]REPORTER:[/] [{shell.Theme.Colors.Foreground}]{Markup.Escape(ToHeaderLabel(alert.Source))}[/]          [{shell.Theme.Colors.Muted}]{FormatAge(alert.Timestamp)}[/]");
        });

        return CreateFrame(
            shell,
            new Rows(InterleaveWithRules(tickets.ToArray(), shell.Theme.Colors.Border)),
            "Attention Queue",
            $"{shell.RecentAlerts.Count:00} Active",
            titleColor: shell.Theme.Colors.Warning);
    }

    private static IRenderable BuildHeroPanel(AppShellModel shell)
    {
        if (shell.ShowWarning && !string.IsNullOrWhiteSpace(shell.WarningMessage))
        {
            return CreateFrame(
                shell,
                new Markup($"[{shell.Theme.Colors.Warning}]TEMPORAL WARNING:[/] {Markup.Escape(shell.WarningMessage)}"),
                "Override",
                titleColor: shell.Theme.Colors.Warning);
        }

        if (shell.ActiveScreen.Timeline is not null)
        {
            return BuildTimeline(shell, shell.ActiveScreen.Timeline);
        }

        var summary = new Table
        {
            Border = TableBorder.None,
            Expand = true
        };

        summary.HideHeaders();
        summary.AddColumn(string.Empty);
        summary.AddColumn(string.Empty);
        summary.AddRow(
            new Markup($"[{shell.Theme.Colors.Highlight}]{Markup.Escape(ToSignalLabel(shell.ActiveScreen.Title))}[/]"),
            Align.Right(new Markup($"[{shell.Theme.Colors.Muted}]ACTIVE SCREEN:[/] [{shell.Theme.Colors.Accent}]{Markup.Escape(ToSignalLabel(shell.ActiveScreenId.Value))}[/]")));
        summary.AddRow(
            new Markup($"[{shell.Theme.Colors.Foreground}]{Markup.Escape(shell.ActiveScreen.Subtitle)}[/]"),
            Align.Right(new Markup($"[{shell.Theme.Colors.Muted}]MODULES ONLINE:[/] [{shell.Theme.Colors.Accent}]{shell.Navigation.Count:00}[/]")));

        return CreateFrame(shell, summary, "Workspace Overview");
    }

    private static IRenderable BuildManagerPanel(AppShellModel shell)
    {
        if (shell.ActiveScreen.Terminal is not null)
        {
            return BuildTerminal(shell, shell.ActiveScreen.Terminal);
        }

        var sections = new List<IRenderable>();

        if (shell.ActiveScreen.Table is not null && shell.ActiveScreen.Panels.Count > 0)
        {
            var grid = new Grid();
            grid.AddColumn();
            grid.AddColumn();
            grid.AddRow(
                BuildTable(shell, shell.ActiveScreen.Table),
                BuildPanelDeck(shell, shell.ActiveScreen.Panels));
            sections.Add(grid);
        }
        else
        {
            if (shell.ActiveScreen.Table is not null)
            {
                sections.Add(BuildTable(shell, shell.ActiveScreen.Table));
            }

            if (shell.ActiveScreen.Panels.Count > 0)
            {
                sections.Add(BuildPanelDeck(shell, shell.ActiveScreen.Panels));
            }
        }

        if (shell.ActiveScreen.Alerts.Count > 0)
        {
            sections.Add(BuildAlerts(shell, shell.ActiveScreen.Alerts));
        }

        sections.Add(CreateFrame(shell, BuildOperatorLog(shell), "Activity"));

        if (sections.Count == 1)
        {
            return sections[0];
        }

        return new Rows(sections.ToArray());
    }

    private static IRenderable BuildProcessManagerTable(AppShellModel shell)
    {
        var alerts = Math.Clamp(GetStatusInt(shell, "Alerts", 0), 0, 99);
        var pulse = Math.Clamp(GetStatusInt(shell, "Pulse", 50), 0, 100);
        var critical = GetStatusInt(shell, "Critical", 0);
        var rows = new[]
        {
            new ProcessRow("MINUTEMAN_01", $"PARSING_{ToSignalLabel(shell.ActiveScreen.Title)}_LOGS", Math.Clamp(35 + alerts * 4, 0, 99), critical > 0 ? "WORKING" : "IDLE", shell.Theme.Colors.Highlight),
            new ProcessRow("HE_WHO_REMAINS_BOT", "MERGING_SYNTHETIC_BRANCHES", Math.Clamp(20 + pulse / 2, 0, 99), "WORKING", shell.Theme.Colors.Highlight),
            new ProcessRow("MISS_MINUTES_AI", $"OPTIMIZING_{ToSignalLabel(shell.ActiveScreenId.Value)}_AURA", 100, "IDLE", shell.Theme.Colors.Muted),
            new ProcessRow("LOKI_AGENT_S2", "DEBUGGING_GLITCH_IN_THE_SYSTEM", critical > 0 ? 0 : 64, critical > 0 ? "FAULTED" : "WORKING", critical > 0 ? shell.Theme.Colors.Critical : shell.Theme.Colors.Warning)
        };

        var table = new Table
        {
            Border = TableBorder.None,
            Expand = true
        };

        table.HideHeaders();
        table.AddColumn(string.Empty);
        table.AddColumn(string.Empty);
        table.AddColumn(string.Empty);
        table.AddColumn(string.Empty);
        table.AddRow(
            $"[{shell.Theme.Colors.Muted}]AGENT ID[/]",
            $"[{shell.Theme.Colors.Muted}]TASK DESCRIPTION[/]",
            $"[{shell.Theme.Colors.Muted}]PROGRESS[/]",
            $"[{shell.Theme.Colors.Muted}]STATUS[/]");

        foreach (var row in rows)
        {
            var progress = row.Progress == 0
                ? $"[{shell.Theme.Colors.Muted}]ERROR[/]"
                : $"[{shell.Theme.Colors.Accent}]{BuildProgressBar(row.Progress, 12)}[/] [{shell.Theme.Colors.Highlight}]{row.Progress}%[/]";

            table.AddRow(
                $"[{shell.Theme.Colors.Foreground}]{row.AgentId}[/]",
                $"[{shell.Theme.Colors.Foreground}]{Markup.Escape(row.Task)}[/]",
                progress,
                $"[{row.StatusColor}]{row.Status}[/]");
        }

        return table;
    }

    private static IRenderable BuildPanelDeck(AppShellModel shell, IReadOnlyList<PanelModel> panels)
    {
        var renderedPanels = panels
            .Take(4)
            .Select(panel => (IRenderable)BuildPanel(shell, panel))
            .ToArray();

        if (renderedPanels.Length == 0)
        {
            return CreateFrame(
                shell,
                new Markup($"[{shell.Theme.Colors.Muted}]NO_MODULE_PANELS_AVAILABLE[/]"),
                "Panels");
        }

        return new Rows(renderedPanels);
    }

    private static IRenderable BuildOperatorLog(AppShellModel shell)
    {
        var lines = new List<string>();

        if (!string.IsNullOrWhiteSpace(shell.ActiveScreen.Hint))
        {
            lines.Add($"[{shell.Theme.Colors.Muted}]OP NOTE:[/] [{shell.Theme.Colors.Foreground}]{Markup.Escape(shell.ActiveScreen.Hint)}[/]");
        }

        lines.AddRange(shell.ActiveScreen.Panels
            .SelectMany(panel => panel.Lines)
            .Take(2)
            .Select(line => $"[{shell.Theme.Colors.Muted}]TRACE:[/] [{shell.Theme.Colors.Foreground}]{Markup.Escape(line)}[/]"));

        lines.AddRange(shell.ActiveScreen.LogLines
            .Take(2)
            .Select(line => $"[{shell.Theme.Colors.Muted}]LOG:[/] [{shell.Theme.Colors.Foreground}]{Markup.Escape(line)}[/]"));

        if (lines.Count == 0)
        {
            lines.Add($"[{shell.Theme.Colors.Muted}]OP NOTE:[/] [{shell.Theme.Colors.Foreground}]SYNTHETIC CONTROL LOOP IS NOMINAL.[/]");
        }

        return new Markup(string.Join("\n", lines));
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

    private static string BuildProgressBar(int value, int width)
    {
        var clamped = Math.Clamp(value, 0, 100);
        var filled = (int)Math.Round(clamped / 100d * width);
        return new string('━', filled) + new string('─', Math.Max(width - filled, 0));
    }

    private static string BuildLinePlot(IReadOnlyList<int> samples, int min, int max, int width, int height)
    {
        if (samples.Count == 0)
        {
            return "·";
        }

        var canvas = new char[height, width];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                canvas[y, x] = ' ';
            }
        }

        var baseline = height / 2;
        for (var x = 0; x < width; x++)
        {
            canvas[baseline, x] = '─';
        }

        var points = new (int X, int Y)[width];
        for (var x = 0; x < width; x++)
        {
            var index = (int)Math.Round(x * (samples.Count - 1) / (double)Math.Max(width - 1, 1));
            var normalized = Math.Clamp(samples[index], min, max);
            var ratio = max == min ? 0.5 : (normalized - min) / (double)(max - min);
            var y = height - 1 - (int)Math.Round(ratio * (height - 1));
            points[x] = (x, y);
        }

        for (var i = 0; i < points.Length - 1; i++)
        {
            var current = points[i];
            var next = points[i + 1];
            var dx = next.X - current.X;
            var dy = next.Y - current.Y;
            var steps = Math.Max(Math.Abs(dx), Math.Abs(dy));

            for (var step = 0; step <= steps; step++)
            {
                var x = current.X + (int)Math.Round(step * dx / (double)Math.Max(steps, 1));
                var y = current.Y + (int)Math.Round(step * dy / (double)Math.Max(steps, 1));
                canvas[y, x] = dy switch
                {
                    > 0 => '╲',
                    < 0 => '╱',
                    _ => '─'
                };
            }

            canvas[current.Y, current.X] = '●';
        }

        canvas[points[^1].Y, points[^1].X] = '●';

        var lines = new string[height];
        for (var y = 0; y < height; y++)
        {
            var row = new char[width];
            for (var x = 0; x < width; x++)
            {
                row[x] = canvas[y, x];
            }

            lines[y] = new string(row).TrimEnd();
        }

        return string.Join("\n", lines);
    }

    private static string BuildBranchBarField(IReadOnlyList<int> samples, int min, int max, int width, int height)
    {
        if (samples.Count == 0)
        {
            return "·";
        }

        var canvas = new char[height, width];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                canvas[y, x] = ' ';
            }
        }

        var baseline = height / 2;
        for (var x = 0; x < width; x++)
        {
            canvas[baseline, x] = '─';
        }

        var groupCount = Math.Min(5, Math.Max(3, samples.Count / 14));
        var segmentWidth = Math.Max(10, width / (groupCount + 1));

        for (var group = 0; group < groupCount; group++)
        {
            var sourceIndex = (int)Math.Round(group * (samples.Count - 1) / (double)Math.Max(groupCount - 1, 1));
            var normalized = Math.Clamp(samples[sourceIndex], min, max);
            var ratio = max == min ? 0.5 : (normalized - min) / (double)(max - min);
            var branchHeight = Math.Max(1, (int)Math.Round(ratio * (height / 2d)));
            var startX = 6 + group * segmentWidth;
            var upStemX = Math.Min(startX + 2, width - 3);
            var downStemX = Math.Min(startX + (segmentWidth / 2), width - 3);
            var upTopY = Math.Max(0, baseline - branchHeight);
            var downBottomY = Math.Min(height - 1, baseline + branchHeight);

            for (var x = startX; x < Math.Min(startX + segmentWidth - 1, width); x++)
            {
                canvas[baseline, x] = '─';
            }

            for (var y = upTopY + 1; y < baseline; y++)
            {
                canvas[y, upStemX] = '│';
            }

            for (var y = baseline + 1; y < downBottomY; y++)
            {
                canvas[y, downStemX] = '│';
            }

            if (upTopY >= 0)
            {
                canvas[upTopY, upStemX] = '●';
            }

            if (downBottomY < height)
            {
                canvas[downBottomY, downStemX] = '●';
            }
        }

        var lines = new string[height];
        for (var y = 0; y < height; y++)
        {
            var row = new char[width];
            for (var x = 0; x < width; x++)
            {
                row[x] = canvas[y, x];
            }

            lines[y] = new string(row).TrimEnd();
        }

        return string.Join("\n", lines);
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

    private static IRenderable[] InterleaveWithRules(IRenderable[] items, string color)
    {
        var result = new List<IRenderable>();
        for (var i = 0; i < items.Length; i++)
        {
            result.Add(items[i]);
            if (i < items.Length - 1)
            {
                result.Add(new Rule().RuleStyle(CreateStyle(color)));
            }
        }

        return result.ToArray();
    }

    private static string FormatAge(DateTimeOffset timestamp)
    {
        var delta = DateTimeOffset.Now - timestamp;
        if (delta.TotalMinutes < 1)
        {
            return "NOW";
        }

        if (delta.TotalHours < 1)
        {
            return $"{(int)delta.TotalMinutes}M AGO";
        }

        return $"{(int)delta.TotalHours}H AGO";
    }

    private sealed record ProcessRow(string AgentId, string Task, int Progress, string Status, string StatusColor);
}
