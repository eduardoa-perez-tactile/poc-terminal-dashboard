using Tva.Core;

namespace Tva.Contracts;

public interface IWorkTrackerDataSource
{
    WorkQueueModel GetSnapshot(DateTimeOffset now);
}
