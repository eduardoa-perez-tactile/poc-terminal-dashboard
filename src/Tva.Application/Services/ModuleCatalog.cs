using Tva.Contracts;
using Tva.Core;

namespace Tva.Application;

public sealed class ModuleCatalog : IModuleCatalog
{
    private readonly Dictionary<ScreenId, IScreenProvider> _screens = new();
    private readonly IReadOnlyList<NavigationEntry> _navigation;
    private readonly IReadOnlyList<Func<IModuleState, PanelModel>> _dashboardPanels;
    private readonly IReadOnlyList<Func<IModuleState, StatusItem>> _statusItems;

    public ModuleCatalog(IEnumerable<IModule> modules)
    {
        var moduleList = modules.ToList();
        var navigation = new List<NavigationEntry>();
        var dashboardPanels = new List<Func<IModuleState, PanelModel>>();
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
            "Press [1-5] to switch screens.");
    }

    public IReadOnlyList<PanelModel> BuildDashboardPanels(IModuleState state) => _dashboardPanels
        .Select(factory => factory(state))
        .ToList();

    public IReadOnlyList<StatusItem> BuildStatusItems(IModuleState state) => _statusItems
        .Select(factory => factory(state))
        .ToList();

    private sealed class ModuleRegistrationContext : IModuleContext
    {
        private readonly Dictionary<ScreenId, IScreenProvider> _screens;
        private readonly List<NavigationEntry> _navigation;
        private readonly List<Func<IModuleState, PanelModel>> _dashboardPanels;
        private readonly List<Func<IModuleState, StatusItem>> _statusItems;

        public ModuleRegistrationContext(
            Dictionary<ScreenId, IScreenProvider> screens,
            List<NavigationEntry> navigation,
            List<Func<IModuleState, PanelModel>> dashboardPanels,
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

        public void RegisterDashboardPanel(Func<IModuleState, PanelModel> panelFactory)
        {
            _dashboardPanels.Add(panelFactory);
        }

        public void RegisterStatusItem(Func<IModuleState, StatusItem> statusFactory)
        {
            _statusItems.Add(statusFactory);
        }
    }
}
