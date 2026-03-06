namespace Tva.Contracts;

public interface IModule
{
    string Id { get; }
    string DisplayName { get; }

    void Register(IModuleContext context);
}
