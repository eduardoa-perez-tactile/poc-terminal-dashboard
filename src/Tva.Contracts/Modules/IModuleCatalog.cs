using Tva.Core;

namespace Tva.Contracts;

public interface IModuleCatalog
{
    IReadOnlyList<IModule> Modules { get; }
    IReadOnlyList<NavigationEntry> Navigation { get; }

    ScreenViewModel BuildScreen(ScreenId screenId, IModuleState state);
    IReadOnlyList<DashboardPanelModel> BuildDashboardPanels(IModuleState state);
    IReadOnlyList<StatusItem> BuildStatusItems(IModuleState state);
}
