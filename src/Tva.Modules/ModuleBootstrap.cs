using Tva.Contracts;

namespace Tva.Modules;

public static class ModuleBootstrap
{
    public static IReadOnlyList<IModule> CreateModules()
    {
        return
        [
            new DashboardModule(),
            new WorkQueueModule(),
            new CodingSessionModule(),
            new TerminalModule(),
            new ChangeDeliveryModule(),
            new ReviewQueueModule(),
            new CommunicationsModule(),
            new WorkLogModule()
        ];
    }
}
