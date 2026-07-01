using MediatR;
using Microsoft.EntityFrameworkCore;
using PayCentral.Application.Common.Interfaces;
using PayCentral.Application.Common.Models;
using PayCentral.Domain.Enums;

namespace PayCentral.Application.Reports;

public class GetTransactionReportQueryHandler
    : IRequestHandler<GetTransactionReportQuery, ApiResponse<List<TransactionReportDto>>>
{
    private readonly IAppDbContext _context;

    public GetTransactionReportQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<TransactionReportDto>>> Handle(
        GetTransactionReportQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Transactions
            .Include(t => t.Card).ThenInclude(c => c.User)
            .Include(t => t.Merchant)
            .AsQueryable();

        if (request.FromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= request.FromDate);

        if (request.ToDate.HasValue)
            query = query.Where(t => t.CreatedAt <= request.ToDate);

        if (request.CardId.HasValue)
            query = query.Where(t => t.CardId == request.CardId);

        var data = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TransactionReportDto(
                t.ReferenceNumber,
                t.Card.MaskedCardNumber,
                t.Card.User.FirstName + " " + t.Card.User.LastName,
                t.Type.ToString(),
                t.Status.ToString(),
                t.Amount,
                t.BalanceAfter,
                t.Currency,
                t.Merchant != null ? t.Merchant.Name : null,
                t.Merchant != null ? t.Merchant.Category : null,
                t.IsInternational,
                t.Description,
                t.CreatedAt))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<TransactionReportDto>>.Ok(data);
    }
}

public class GetFraudReportQueryHandler
    : IRequestHandler<GetFraudReportQuery, ApiResponse<List<FraudReportDto>>>
{
    private readonly IAppDbContext _context;

    public GetFraudReportQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<FraudReportDto>>> Handle(
        GetFraudReportQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FraudAlerts
            .Include(f => f.Card).ThenInclude(c => c.User)
            .AsQueryable();

        if (request.FromDate.HasValue)
            query = query.Where(f => f.CreatedAt >= request.FromDate);

        if (request.ToDate.HasValue)
            query = query.Where(f => f.CreatedAt <= request.ToDate);

        var data = await query
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FraudReportDto(
                f.Card.MaskedCardNumber,
                f.Card.User.FirstName + " " + f.Card.User.LastName,
                f.AlertType,
                f.Reason,
                f.Severity.ToString(),
                f.IsResolved,
                f.ResolvedAt,
                f.ResolvedBy,
                f.CreatedAt))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<FraudReportDto>>.Ok(data);
    }
}

public class GetCardReportQueryHandler
    : IRequestHandler<GetCardReportQuery, ApiResponse<List<CardReportDto>>>
{
    private readonly IAppDbContext _context;

    public GetCardReportQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<CardReportDto>>> Handle(
        GetCardReportQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Cards
            .Include(c => c.User)
            .Include(c => c.Wallet)
            .Include(c => c.Transactions)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<CardStatus>(request.Status, out var status))
            query = query.Where(c => c.Status == status);

        var data = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CardReportDto(
                c.MaskedCardNumber,
                c.User.FirstName + " " + c.User.LastName,
                c.User.Email,
                c.Status.ToString(),
                c.Wallet.Balance,
                c.Wallet.AvailableBalance,
                c.Wallet.Currency,
                c.ExpiryDate,
                c.ActivatedAt,
                c.CreatedAt,
                c.Transactions.Count))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<CardReportDto>>.Ok(data);
    }
}

public class GetDailySummaryQueryHandler
    : IRequestHandler<GetDailySummaryQuery, ApiResponse<DailySummaryDto>>
{
    private readonly IAppDbContext _context;

    public GetDailySummaryQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<DailySummaryDto>> Handle(
        GetDailySummaryQuery request, CancellationToken cancellationToken)
    {
        var startOfDay = request.Date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var transactions = await _context.Transactions
            .Where(t => t.CreatedAt >= startOfDay && t.CreatedAt < endOfDay)
            .ToListAsync(cancellationToken);

        var fraudAlerts = await _context.FraudAlerts
            .CountAsync(f => f.CreatedAt >= startOfDay && f.CreatedAt < endOfDay,
                cancellationToken);

        var newCards = await _context.Cards
            .CountAsync(c => c.CreatedAt >= startOfDay && c.CreatedAt < endOfDay,
                cancellationToken);

        var summary = new DailySummaryDto(
            request.Date.Date,
            transactions.Count,
            transactions.Sum(t => t.Amount),
            transactions.Count(t => t.Type == TransactionType.Purchase),
            transactions.Where(t => t.Type == TransactionType.Purchase).Sum(t => t.Amount),
            transactions.Count(t => t.Type == TransactionType.Refund),
            transactions.Where(t => t.Type == TransactionType.Refund).Sum(t => t.Amount),
            transactions.Count(t => t.Type == TransactionType.LoadFunds),
            transactions.Where(t => t.Type == TransactionType.LoadFunds).Sum(t => t.Amount),
            fraudAlerts,
            newCards);

        return ApiResponse<DailySummaryDto>.Ok(summary);
    }
}