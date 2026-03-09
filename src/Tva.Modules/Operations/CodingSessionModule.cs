using Tva.Contracts;
using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Modules;

public sealed class CodingSessionModule : IModule
{
    public string Id => "coding";

    public string DisplayName => "Coding";

    public void Register(IModuleContext context)
    {
        context.RegisterNavigation(new NavigationEntry(Id, ScreenCatalog.Coding, "Coding", "3", 30));
        context.RegisterScreen(ScreenCatalog.Coding, new CodingScreenProvider());

        context.RegisterDashboardPanel(
            "coding-session",
            DashboardRegion.HeroRight,
            20,
            state =>
                new PanelModel(
                    "Coding Focus",
                    [
                        $"Repo: {state.CodingSession.Repository.Name}",
                        $"Branch: {state.CodingSession.Repository.Branch}",
                        $"Claude: {state.CodingSession.AiSession.Status}"
                    ],
                    PanelTone.Accent));

        context.RegisterStatusItem(state =>
            new StatusItem(
                "Repo",
                state.CodingSession.Repository.Name,
                state.CodingSession.Repository.HasUncommittedChanges ? SeverityLevel.Warning : SeverityLevel.Info));
    }

    private sealed class CodingScreenProvider : IScreenProvider
    {
        public ScreenViewModel Create(IModuleState state)
        {
            var repo = state.CodingSession.Repository;
            var ai = state.CodingSession.AiSession;
            var rows = state.CodingSession.LocalChanges
                .Select(change => (IReadOnlyList<string>)
                [
                    change.Path,
                    change.ChangeKind,
                    change.IsStaged ? "yes" : "no"
                ])
                .ToList();

            return new ScreenViewModel(
                ScreenCatalog.Coding,
                "Coding Session",
                "Current implementation context and local repo state",
                [
                    new PanelModel(
                        "Active Repo",
                        [
                            $"Name: {repo.Name}",
                            $"Branch: {repo.Branch}",
                            $"Ahead/Behind: +{repo.AheadBy} / -{repo.BehindBy}"
                        ],
                        repo.HasUncommittedChanges ? PanelTone.Warning : PanelTone.Accent),
                    new PanelModel(
                        "Claude Code",
                        [
                            $"Task: {ai.ActiveTask}",
                            $"Status: {ai.Status}",
                            $"Started: {ai.StartedAt:HH:mm}"
                        ]),
                    new PanelModel(
                        "Focus",
                        [state.CodingSession.FocusNote])
                ],
                new TableModel("Local Changes", ["Path", "Kind", "Staged"], rows),
                [],
                [],
                null,
                null,
                "Use the terminal screen for command execution; this screen keeps the coding context visible.");
        }
    }
}
