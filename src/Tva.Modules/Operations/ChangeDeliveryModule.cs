using Tva.Contracts;
using Tva.Core;
using Tva.Core.Ids;

namespace Tva.Modules;

public sealed class ChangeDeliveryModule : IModule
{
    public string Id => "change-delivery";

    public string DisplayName => "Change Delivery";

    public void Register(IModuleContext context)
    {
        context.RegisterNavigation(new NavigationEntry(Id, ScreenCatalog.ChangeDelivery, "Delivery", "5", 50));
        context.RegisterScreen(ScreenCatalog.ChangeDelivery, new ChangeDeliveryScreenProvider());

        context.RegisterDashboardPanel(
            "change-delivery",
            DashboardRegion.MainLeft,
            30,
            state =>
                new PanelModel(
                    "Ready To Ship",
                    [
                        $"Git ready: {state.ChangeDelivery.GitChanges.Count(change => change.Readiness == DeliveryReadiness.ReadyToOpen)}",
                        $"SVN ready: {state.ChangeDelivery.SvnChanges.Count(change => change.Readiness == DeliveryReadiness.ReadyToOpen)}",
                        $"Blockers: {state.ChangeDelivery.Blockers.Count}"
                    ],
                    state.ChangeDelivery.Blockers.Count > 0 ? PanelTone.Warning : PanelTone.Normal));

        context.RegisterStatusItem(state =>
            new StatusItem(
                "Ready",
                state.ChangeDelivery.GitChanges.Count(change => change.Readiness == DeliveryReadiness.ReadyToOpen).ToString(),
                state.ChangeDelivery.Blockers.Count > 0 ? SeverityLevel.Warning : SeverityLevel.Info));
    }

    private sealed class ChangeDeliveryScreenProvider : IScreenProvider
    {
        public ScreenViewModel Create(IModuleState state)
        {
            var rows = state.ChangeDelivery.GitChanges
                .Concat(state.ChangeDelivery.SvnChanges)
                .Select(change => (IReadOnlyList<string>)
                [
                    change.Id,
                    change.System.ToString(),
                    change.Repository,
                    change.BranchOrPath,
                    change.Readiness.ToString()
                ])
                .ToList();

            return new ScreenViewModel(
                ScreenCatalog.ChangeDelivery,
                "Change Delivery",
                "Outbound Git and SVN review preparation",
                [
                    new PanelModel(
                        "Git Outbound",
                        state.ChangeDelivery.GitChanges.Select(change => $"{change.Id} {change.Title}").DefaultIfEmpty("No Git changes.").ToList()),
                    new PanelModel(
                        "SVN Outbound",
                        state.ChangeDelivery.SvnChanges.Select(change => $"{change.Id} {change.Title}").DefaultIfEmpty("No SVN changes.").ToList()),
                    new PanelModel(
                        "Delivery Blockers",
                        state.ChangeDelivery.Blockers.Select(blocker => blocker.Reason).DefaultIfEmpty("No blockers.").ToList(),
                        state.ChangeDelivery.Blockers.Count > 0 ? PanelTone.Warning : PanelTone.Normal)
                ],
                new TableModel("Outbound Changes", ["Id", "SCM", "Repo", "Branch/Path", "Readiness"], rows),
                state.Alerts.Where(alert => alert.Source is "Delivery").Take(4).ToList(),
                [],
                null,
                null,
                "Track Git and SVN delivery work in one place; keep the execution itself in the terminal.");
        }
    }
}
