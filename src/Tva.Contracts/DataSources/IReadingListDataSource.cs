using Tva.Core;

namespace Tva.Contracts;

public interface IReadingListDataSource
{
    ReadingQueueModel GetSnapshot(DateTimeOffset now);
}
