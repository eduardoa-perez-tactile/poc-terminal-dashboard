using Tva.Core;

namespace Tva.Contracts;

public interface ICodingActivityDataSource
{
    CodingSessionModel GetSnapshot(DateTimeOffset now);
}
