using Tva.Contracts;

namespace Tva.Modules;

public static class ModuleBootstrap
{
    public static IReadOnlyList<IModule> CreateModules()
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
