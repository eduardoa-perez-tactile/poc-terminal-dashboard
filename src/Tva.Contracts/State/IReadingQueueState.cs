using Tva.Core;

namespace Tva.Contracts;

public interface IReadingQueueState
{
    ReadingQueueModel ReadingQueue { get; }
}
