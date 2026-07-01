using PayCentral.Domain.Common;
using PayCentral.Domain.Enums;

namespace PayCentral.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public bool IsSent { get; set; } = false;
    public DateTime? SentAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}