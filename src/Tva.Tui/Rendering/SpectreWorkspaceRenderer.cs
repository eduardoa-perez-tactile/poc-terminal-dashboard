using Spectre.Console;
using Spectre.Console.Rendering;
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
                new Layout("footer").Size(4));

        layout["body"].SplitColumns(
            new Layout("nav").Size(28),
            new Layout("content"));

        layout["header"].Update(BuildHeader(shell));
        layout["nav"].Update(BuildNavigation(shell));
        layout["content"].Update(BuildContent(shell));
        layout["footer"].Update(BuildFooter(shell));

        return layout;
    }

    private static IRenderable BuildHeader(AppShellModel shell)
    {
        var header =
            $"[{shell.Theme.Colors.Header}]{Markup.Escape(shell.Theme.Frame.HeaderPrefix)}[/] " +
            $"[{shell.Theme.Colors.Accent}]TVA[/] " +
            $"[{shell.Theme.Colors.Foreground}]{Markup.Escape(shell.AppTitle)}[/] " +
            $"[{shell.Theme.Colors.Muted}]| {Markup.Escape(shell.HeaderSubtitle)}[/] " +
            $"[{shell.Theme.Colors.Highlight}]| Active: {Markup.Escape(shell.ActiveScreenId.Value)}[/]";

        var panel = new Panel(new Markup(header))
        {
            Border = BoxBorder.Square,
            Padding = new Padding(shell.Theme.Frame.PanelPadding, 0, shell.Theme.Frame.PanelPadding, 0)
        };

        return panel;
    }

    private static IRenderable BuildNavigation(AppShellModel shell)
    {
        var lines = shell.Navigation.Select(entry =>
        {
            var marker = entry.ScreenId == shell.ActiveScreenId ? ">" : " ";
            return $"[{shell.Theme.Colors.Accent}]{marker} {Markup.Escape(entry.Shortcut)}[/] {Markup.Escape(entry.Label)}";
        });

        var help =
            $"\n[{shell.Theme.Colors.Muted}]Tab/Shift+Tab cycle[/]\n" +
            $"[{shell.Theme.Colors.Muted}]B back | W warning | Q quit[/]";

        var body = new Markup(string.Join("\n", lines) + help);
        var panel = new Panel(body)
        {
            Header = new PanelHeader($"[{shell.Theme.Colors.Highlight}]Navigation[/]"),
            Border = BoxBorder.Square
        };

        return panel;
    }

    private static IRenderable BuildContent(AppShellModel shell)
    {
        var parts = new List<IRenderable>();

        if (shell.ShowWarning && !string.IsNullOrWhiteSpace(shell.WarningMessage))
        {
            var warning = new Panel(new Markup($"[{shell.Theme.Colors.Warning}]WARNING:[/] {Markup.Escape(shell.WarningMessage)}"))
            {
                Border = BoxBorder.Double,
                Header = new PanelHeader($"[{shell.Theme.Colors.Warning}]Modal[/]")
            };
            parts.Add(warning);
        }

        parts.Add(new Markup($"[{shell.Theme.Colors.Highlight}]{Markup.Escape(shell.ActiveScreen.Title)}[/] [{shell.Theme.Colors.Muted}]:: {Markup.Escape(shell.ActiveScreen.Subtitle)}[/]"));

        if (shell.ActiveScreen.Panels.Count > 0)
        {
            var panelRenderables = shell.ActiveScreen.Panels.Select(panelModel => BuildPanel(shell, panelModel)).ToList();
            parts.Add(new Columns(panelRenderables) { Expand = true });
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
            var logLines = shell.ActiveScreen.LogLines.Take(ThemeConventions.MaxEventLines).Select(Markup.Escape);
            var logPanel = new Panel(new Markup(string.Join("\n", logLines)))
            {
                Header = new PanelHeader($"[{shell.Theme.Colors.Highlight}]Logs[/]"),
                Border = BoxBorder.Square
            };
            parts.Add(logPanel);
        }

        if (shell.ActiveScreen.Timeline is not null)
        {
            parts.Add(BuildTimeline(shell, shell.ActiveScreen.Timeline));
        }

        if (shell.ActiveScreen.Terminal is not null)
        {
            parts.Add(BuildTerminal(shell, shell.ActiveScreen.Terminal));
        }

        if (!string.IsNullOrWhiteSpace(shell.ActiveScreen.Hint))
        {
            parts.Add(new Markup($"[{shell.Theme.Colors.Muted}]Hint: {Markup.Escape(shell.ActiveScreen.Hint)}[/]"));
        }

        return new Panel(new Rows(parts.ToArray()))
        {
            Border = BoxBorder.Square
        };
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

        var text = string.Join("\n", panelModel.Lines.Select(Markup.Escape));
        var panel = new Panel(new Markup($"[{color}]{text}[/]"))
        {
            Header = new PanelHeader($"[{color}]{Markup.Escape(panelModel.Title)}[/]"),
            Border = BoxBorder.Square
        };
        return panel;
    }

    private static IRenderable BuildTable(AppShellModel shell, TableModel tableModel)
    {
        var table = new Table
        {
            Border = TableBorder.Square,
            Title = new TableTitle($"[{shell.Theme.Colors.Highlight}]{Markup.Escape(tableModel.Title)}[/]")
        };

        foreach (var column in tableModel.Columns)
        {
            table.AddColumn(new TableColumn($"[{shell.Theme.Colors.Accent}]{Markup.Escape(column)}[/]"));
        }

        foreach (var row in tableModel.Rows)
        {
            table.AddRow(row.Select(cell => Markup.Escape(cell)).ToArray());
        }

        return table;
    }

    private static IRenderable BuildAlerts(AppShellModel shell, IReadOnlyList<AlertModel> alerts)
    {
        var table = new Table
        {
            Border = TableBorder.Square,
            Title = new TableTitle($"[{shell.Theme.Colors.Warning}]Alert Highlights[/]")
        };

        table.AddColumn(new TableColumn("Time"));
        table.AddColumn(new TableColumn("Source"));
        table.AddColumn(new TableColumn("Message"));

        foreach (var alert in alerts.Take(8))
        {
            var color = alert.Severity switch
            {
                SeverityLevel.Warning => shell.Theme.Colors.Warning,
                SeverityLevel.Critical => shell.Theme.Colors.Critical,
                _ => shell.Theme.Colors.Foreground
            };

            table.AddRow(
                alert.Timestamp.ToString("HH:mm:ss"),
                Markup.Escape(alert.Source),
                $"[{color}]{Markup.Escape(alert.Message)}[/]");
        }

        return table;
    }

    private static IRenderable BuildTimeline(AppShellModel shell, TimelineModel timeline)
    {
        const string ramp = " .:-=+*#%@";
        var bars = timeline.Samples.Select(sample =>
        {
            var normalized = Math.Clamp(sample, timeline.Min, timeline.Max);
            var ratio = timeline.Max == timeline.Min ? 0.0 : (normalized - timeline.Min) / (double)(timeline.Max - timeline.Min);
            var index = (int)Math.Round(ratio * (ramp.Length - 1));
            return ramp[index];
        });

        var content = new Markup(
            $"[{shell.Theme.Colors.Accent}]{Markup.Escape(new string(bars.ToArray()))}[/]\n" +
            $"[{shell.Theme.Colors.Muted}]min:{timeline.Min} max:{timeline.Max} samples:{timeline.Samples.Count}[/]");

        var panel = new Panel(content)
        {
            Header = new PanelHeader($"[{shell.Theme.Colors.Highlight}]{Markup.Escape(timeline.Title)}[/]"),
            Border = BoxBorder.Square
        };
        return panel;
    }

    private static IRenderable BuildTerminal(AppShellModel shell, TerminalViewModel terminal)
    {
        var outputTable = new Table { Border = TableBorder.Square };
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
        var panel = new Panel(content)
        {
            Header = new PanelHeader($"[{shell.Theme.Colors.Highlight}]Terminal[/]"),
            Border = BoxBorder.Square
        };
        return panel;
    }

    private static IRenderable BuildFooter(AppShellModel shell)
    {
        var statusText = string.Join(
            $" [{shell.Theme.Colors.Muted}]{ThemeConventions.Separator}[/] ",
            shell.StatusItems.Select(item =>
            {
                var color = item.Severity switch
                {
                    SeverityLevel.Warning => shell.Theme.Colors.Warning,
                    SeverityLevel.Critical => shell.Theme.Colors.Critical,
                    _ => shell.Theme.Colors.Foreground
                };

                return $"[{shell.Theme.Colors.Muted}]{Markup.Escape(item.Key)}:[/] [{color}]{Markup.Escape(item.Value)}[/]";
            }));

        var lines = new List<IRenderable>
        {
            new Markup(statusText)
        };

        if (shell.ActiveNotification is not null)
        {
            var color = shell.ActiveNotification.Severity switch
            {
                SeverityLevel.Warning => shell.Theme.Colors.Warning,
                SeverityLevel.Critical => shell.Theme.Colors.Critical,
                _ => shell.Theme.Colors.Muted
            };

            lines.Add(new Markup($"[{color}]Notice: {Markup.Escape(shell.ActiveNotification.Message)}[/]"));
        }

        var panel = new Panel(new Rows(lines.ToArray()))
        {
            Border = BoxBorder.Square
        };
        return panel;
    }
}
