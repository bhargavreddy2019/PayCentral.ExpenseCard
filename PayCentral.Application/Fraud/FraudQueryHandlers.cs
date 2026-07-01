using MediatR;
using Microsoft.EntityFrameworkCore;
using PayCentral.Application.Common.Interfaces;
using PayCentral.Application.Common.Models;

namespace PayCentral.Application.Fraud;

public record GetFraudAlertsQuery(
    bool? IsResolved,
    int Page = 1,
    int PageSize = 10
) : IRequest<ApiResponse<List<FraudAlertDto>>>;

public record ResolveFraudAlertCommand(Guid AlertId)
    : IRequest<ApiResponse<FraudAlertDto>>;

public class GetFraudAlertsQueryHandler
    : IRequestHandler<GetFraudAlertsQuery, ApiResponse<List<FraudAlertDto>>>
{
    private readonly IAppDbContext _context;

    public GetFraudAlertsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<FraudAlertDto>>> Handle(
        GetFraudAlertsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FraudAlerts
            .Include(f => f.Card)
            .ThenInclude(c => c.User)
            .AsQueryable();

        if (request.IsResolved.HasValue)
            query = query.Where(f => f.IsResolved == request.IsResolved);

        var alerts = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(f => new FraudAlertDto(
                f.Id,
                f.Card.MaskedCardNumber,
                f.Card.User.FirstName + " " + f.Card.User.LastName,
                f.AlertType,
                f.Reason,
                f.Severity,
                f.IsResolved,
                f.ResolvedAt,
                f.ResolvedBy,
                f.CreatedAt))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<FraudAlertDto>>.Ok(alerts);
    }
}

public class ResolveFraudAlertCommandHandler
    : IRequestHandler<ResolveFraudAlertCommand, ApiResponse<FraudAlertDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ResolveFraudAlertCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<FraudAlertDto>> Handle(
        ResolveFraudAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _context.FraudAlerts
            .Include(f => f.Card)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(f => f.Id == request.AlertId, cancellationToken);

        if (alert == null)
            throw new KeyNotFoundException($"Fraud alert {request.AlertId} not found");

        alert.IsResolved = true;
        alert.ResolvedAt = DateTime.UtcNow;
        alert.ResolvedBy = _currentUser.Email;
        alert.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<FraudAlertDto>.Ok(new FraudAlertDto(
            alert.Id,
            alert.Card.MaskedCardNumber,
            alert.Card.User.FullName,
            alert.AlertType,
            alert.Reason,
            alert.Severity,
            alert.IsResolved,
            alert.ResolvedAt,
            alert.ResolvedBy,
            alert.CreatedAt), "Fraud alert resolved");
    }
}