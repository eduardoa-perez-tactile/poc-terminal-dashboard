using Tva.Application;

namespace Tva.Modules;

public static class ModuleBootstrap
{
    public static IReadOnlyList<IAppModule> CreateModules()
    {
        return
        [
            new DashboardModule(),
            new AlertsModule(),
            new EventsModule(),
            new TerminalModule(),
            new WaveformModule()
        ];
    }
}
