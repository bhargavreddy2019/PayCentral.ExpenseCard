using Microsoft.Extensions.Logging;
using PayCentral.Application.Common.Interfaces;
using PayCentral.Domain.Entities;
using PayCentral.Domain.Enums;

namespace PayCentral.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IAppDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IAppDbContext context,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SendAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        NotificationChannel channel = NotificationChannel.Push)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Channel = channel,
            Title = title,
            Message = message,
            IsSent = true,
            SentAt = DateTime.UtcNow,
            CreatedBy = "System",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync(default);

        // Mock delivery — in production replace with 
        // real Email/SMS/Push provider
        _logger.LogInformation(
            "Notification sent [{Channel}] to User {UserId}: {Title}",
            channel, userId, title);
    }
}