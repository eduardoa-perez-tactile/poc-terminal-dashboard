using Tva.Core;

namespace Tva.Contracts;

public interface IChangeDeliveryDataSource
{
    ChangeDeliveryModel GetSnapshot(DateTimeOffset now);
}
