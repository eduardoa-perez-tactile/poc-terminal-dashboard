using Tva.Core;

namespace Tva.Contracts;

public interface IModuleCatalog
{
    IReadOnlyList<IModule> Modules { get; }
    IReadOnlyList<NavigationEntry> Navigation { get; }

    ScreenViewModel BuildScreen(ScreenId screenId, IModuleState state);
    IReadOnlyList<PanelModel> BuildDashboardPanels(IModuleState state);
    IReadOnlyList<StatusItem> BuildStatusItems(IModuleState state);
}
