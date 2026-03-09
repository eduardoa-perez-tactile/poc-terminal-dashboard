using Tva.Core;

namespace Tva.Contracts;

public interface IReviewQueueDataSource
{
    ReviewQueueModel GetSnapshot(DateTimeOffset now);
}
