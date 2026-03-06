using Tva.Core;

namespace Tva.Application;

public sealed class NotificationService : INotificationService
{
    private readonly ISessionState _state;

    public NotificationService(ISessionState state)
    {
        _state = state;
    }

    public AppNotification? Latest => _state.Notifications.LastOrDefault();

    public void Add(string message, SeverityLevel severity)
    {
        _state.Notifications.Add(new AppNotification(message, severity, _state.Now));
    }
}
