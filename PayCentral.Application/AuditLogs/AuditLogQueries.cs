using MediatR;
using Microsoft.EntityFrameworkCore;
using PayCentral.Application.Common.Interfaces;
using PayCentral.Application.Common.Models;

namespace PayCentral.Application.AuditLogs;

public record AuditLogDto(
    Guid Id,
    string? UserEmail,
    string Action,
    string EntityName,
    string? EntityId,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    bool IsSuccess,
    DateTime CreatedAt
);

public record GetAuditLogsQuery(
    string? EntityName,
    string? Action,
    DateTime? FromDate,
    DateTime? ToDate,
    int Page = 1,
    int PageSize = 20
) : IRequest<ApiResponse<List<AuditLogDto>>>;

public class GetAuditLogsQueryHandler
    : IRequestHandler<GetAuditLogsQuery, ApiResponse<List<AuditLogDto>>>
{
    private readonly IAppDbContext _context;

    public GetAuditLogsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<AuditLogDto>>> Handle(
        GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs
            .Include(a => a.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.EntityName))
            query = query.Where(a => a.EntityName == request.EntityName);

        if (!string.IsNullOrEmpty(request.Action))
            query = query.Where(a => a.Action.Contains(request.Action));

        if (request.FromDate.HasValue)
            query = query.Where(a => a.CreatedAt >= request.FromDate);

        if (request.ToDate.HasValue)
            query = query.Where(a => a.CreatedAt <= request.ToDate);

        var logs = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AuditLogDto(
                a.Id,
                a.User != null ? a.User.Email : null,
                a.Action,
                a.EntityName,
                a.EntityId,
                a.OldValues,
                a.NewValues,
                a.IpAddress,
                a.IsSuccess,
                a.CreatedAt))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<AuditLogDto>>.Ok(logs);
    }
}