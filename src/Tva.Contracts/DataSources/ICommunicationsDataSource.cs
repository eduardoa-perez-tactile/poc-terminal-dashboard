using Tva.Core;

namespace Tva.Contracts;

public interface ICommunicationsDataSource
{
    CommunicationsModel GetSnapshot(DateTimeOffset now);
}
