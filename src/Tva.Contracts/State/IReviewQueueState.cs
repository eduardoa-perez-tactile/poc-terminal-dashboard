using Tva.Core;

namespace Tva.Contracts;

public interface IReviewQueueState
{
    ReviewQueueModel ReviewQueue { get; }
}
