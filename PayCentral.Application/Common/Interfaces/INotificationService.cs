using PayCentral.Domain.Enums;

namespace PayCentral.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        NotificationChannel channel = NotificationChannel.Push);
}