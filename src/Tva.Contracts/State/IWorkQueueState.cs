using Tva.Core;

namespace Tva.Contracts;

public interface IWorkQueueState
{
    WorkQueueModel WorkQueue { get; }
}
