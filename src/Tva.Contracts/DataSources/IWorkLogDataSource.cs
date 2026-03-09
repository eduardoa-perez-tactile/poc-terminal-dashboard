using Tva.Core;

namespace Tva.Contracts;

public interface IWorkLogDataSource
{
    WorkLogModel GetSnapshot(DateTimeOffset now);
}
