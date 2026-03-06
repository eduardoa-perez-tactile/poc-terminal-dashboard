namespace Tva.Application;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
