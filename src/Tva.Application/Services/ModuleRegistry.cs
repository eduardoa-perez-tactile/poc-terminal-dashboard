using Tva.Core;

namespace Tva.Application;

public sealed class ModuleRegistry
{
    private readonly IReadOnlyList<IAppModule> _modules;
    private readonly Dictionary<ScreenId, IScreenProvider> _screens;

    public ModuleRegistry(IEnumerable<IAppModule> modules)
    {
        _modules = modules.ToList();
        _screens = new Dictionary<ScreenId, IScreenProvider>();

        foreach (var provider in _modules.SelectMany(static module => module.Screens))
        {
            if (!_screens.TryAdd(provider.ScreenId, provider))
            {
                throw new InvalidOperationException($"Duplicate screen registration: {provider.ScreenId.Value}");
            }
        }
    }

    public IReadOnlyList<IAppModule> Modules => _modules;

    public IReadOnlyList<NavigationEntry> Navigation => _modules
        .SelectMany(static module => module.NavigationEntries)
        .OrderBy(static entry => entry.Order)
        .ToList();

    public ScreenViewModel BuildScreen(ScreenId screenId, AppSessionState state)
    {
        if (_screens.TryGetValue(screenId, out var provider))
        {
            return provider.Build(state);
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

    public IReadOnlyList<PanelModel> BuildDashboardPanels(AppSessionState state) => _modules
        .SelectMany(module => module.GetDashboardPanels(state))
        .ToList();

    public IReadOnlyList<StatusItem> BuildStatusItems(AppSessionState state) => _modules
        .SelectMany(module => module.GetStatusItems(state))
        .ToList();
}
