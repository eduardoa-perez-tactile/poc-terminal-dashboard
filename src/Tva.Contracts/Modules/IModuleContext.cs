using Tva.Core;

namespace Tva.Contracts;

public interface IModuleContext
{
    void RegisterScreen(ScreenId id, IScreenProvider provider);
    void RegisterNavigation(NavigationEntry entry);
    void RegisterDashboardPanel(Func<IModuleState, PanelModel> panelFactory);
    void RegisterStatusItem(Func<IModuleState, StatusItem> statusFactory);
}
