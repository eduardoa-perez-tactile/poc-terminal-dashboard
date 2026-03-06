using Tva.Core;

namespace Tva.Application;

public interface INotificationService
{
    AppNotification? Latest { get; }
    void Add(string message, SeverityLevel severity);
}
