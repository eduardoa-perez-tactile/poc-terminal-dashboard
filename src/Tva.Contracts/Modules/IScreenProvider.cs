using Tva.Core;

namespace Tva.Contracts;

public interface IScreenProvider
{
    ScreenViewModel Create(IModuleState state);
}
