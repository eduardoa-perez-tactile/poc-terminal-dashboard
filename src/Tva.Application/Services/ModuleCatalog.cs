using Tva.Contracts;
using Tva.Core;

namespace Tva.Application;

public sealed class ModuleCatalog : IModuleCatalog
{
    private readonly Dictionary<ScreenId, IScreenProvider> _screens = new();
    private readonly IReadOnlyList<NavigationEntry> _navigation;
    private readonly IReadOnlyList<DashboardRegistration> _dashboardPanels;
    private readonly IReadOnlyList<Func<IModuleState, StatusItem>> _statusItems;

    public ModuleCatalog(IEnumerable<IModule> modules)
    {
        var moduleList = modules.ToList();
        var navigation = new List<NavigationEntry>();
        var dashboardPanels = new List<DashboardRegistration>();
        var statusItems = new List<Func<IModuleState, StatusItem>>();
        var context = new ModuleRegistrationContext(_screens, navigation, dashboardPanels, statusItems);

        foreach (var module in moduleList)
        {
            module.Register(context);
        }

        Modules = moduleList;
        _navigation = navigation.OrderBy(static entry => entry.Order).ToList();
        _dashboardPanels = dashboardPanels;
        _statusItems = statusItems;
    }

    public IReadOnlyList<IModule> Modules { get; }

    public IReadOnlyList<NavigationEntry> Navigation => _navigation;

    public ScreenViewModel BuildScreen(ScreenId screenId, IModuleState state)
    {
        if (_screens.TryGetValue(screenId, out var provider))
        {
            return provider.Create(state);
        }

        return new ScreenViewModel(
            screenId,
            "Missing Screen",
            "The requested screen is not registered.",
            [new PanelModel("Missing", [$"Screen '{screenId.Value}' is not registered."], PanelTone.Warning)],
            null,
            [],
            [],
            null,
            null,
            "Press [1-8] to switch screens.");
    }

    public IReadOnlyList<DashboardPanelModel> BuildDashboardPanels(IModuleState state) => _dashboardPanels
        .OrderBy(static registration => registration.Region)
        .ThenBy(static registration => registration.Order)
        .Select(registration => new DashboardPanelModel(
            registration.Key,
            registration.Region,
            registration.Order,
            registration.Factory(state)))
        .ToList();

    public IReadOnlyList<StatusItem> BuildStatusItems(IModuleState state) => _statusItems
        .Select(factory => factory(state))
        .ToList();

    private sealed class ModuleRegistrationContext : IModuleContext
    {
        private readonly Dictionary<ScreenId, IScreenProvider> _screens;
        private readonly List<NavigationEntry> _navigation;
        private readonly List<DashboardRegistration> _dashboardPanels;
        private readonly List<Func<IModuleState, StatusItem>> _statusItems;

        public ModuleRegistrationContext(
            Dictionary<ScreenId, IScreenProvider> screens,
            List<NavigationEntry> navigation,
            List<DashboardRegistration> dashboardPanels,
            List<Func<IModuleState, StatusItem>> statusItems)
        {
            _screens = screens;
            _navigation = navigation;
            _dashboardPanels = dashboardPanels;
            _statusItems = statusItems;
        }

        public void RegisterScreen(ScreenId id, IScreenProvider provider)
        {
            if (!_screens.TryAdd(id, provider))
            {
                throw new InvalidOperationException($"Duplicate screen registration: {id.Value}");
            }
        }

        public void RegisterNavigation(NavigationEntry entry)
        {
            _navigation.Add(entry);
        }

        public void RegisterDashboardPanel(string key, DashboardRegion region, int order, Func<IModuleState, PanelModel> panelFactory)
        {
            _dashboardPanels.Add(new DashboardRegistration(key, region, order, panelFactory));
        }

        public void RegisterStatusItem(Func<IModuleState, StatusItem> statusFactory)
        {
            _statusItems.Add(statusFactory);
        }
    }

    private sealed record DashboardRegistration(
        string Key,
        DashboardRegion Region,
        int Order,
        Func<IModuleState, PanelModel> Factory);
}
