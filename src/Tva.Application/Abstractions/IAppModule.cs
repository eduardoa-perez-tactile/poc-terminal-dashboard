using Tva.Core;

namespace Tva.Application;

public interface IAppModule
{
    AppModuleInfo Metadata { get; }
    IReadOnlyList<NavigationEntry> NavigationEntries { get; }
    IReadOnlyList<IScreenProvider> Screens { get; }
    IReadOnlyList<PanelModel> GetDashboardPanels(AppSessionState state);
    IReadOnlyList<StatusItem> GetStatusItems(AppSessionState state);
}
