using PayCentral.Application.Common.Interfaces;
using PayCentral.Domain.Entities;

namespace PayCentral.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AuditService(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task LogAsync(
        string action,
        string entityName,
        string? entityId = null,
        string? oldValues = null,
        string? newValues = null,
        bool isSuccess = true)
    {
        var log = new AuditLog
        {
            UserId = _currentUser.UserId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = _currentUser.IpAddress,
            IsSuccess = isSuccess,
            CreatedBy = _currentUser.Email ?? "System",
            CreatedAt = DateTime.UtcNow
        };

        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync(default);
    }
}