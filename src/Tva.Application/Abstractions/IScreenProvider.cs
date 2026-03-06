using Tva.Core;

namespace Tva.Application;

public interface IScreenProvider
{
    ScreenId ScreenId { get; }
    string ModuleId { get; }
    ScreenViewModel Build(AppSessionState state);
}
