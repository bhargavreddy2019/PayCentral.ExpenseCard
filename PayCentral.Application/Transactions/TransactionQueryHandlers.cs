using MediatR;
using Microsoft.EntityFrameworkCore;
using PayCentral.Application.Common.Interfaces;
using PayCentral.Application.Common.Models;

namespace PayCentral.Application.Transactions;

public class GetBalanceQueryHandler : IRequestHandler<GetBalanceQuery, ApiResponse<WalletDto>>
{
    private readonly IAppDbContext _context;

    public GetBalanceQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<WalletDto>> Handle(
        GetBalanceQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _context.Wallets
            .Include(w => w.Card)
            .FirstOrDefaultAsync(w => w.CardId == request.CardId, cancellationToken);

        if (wallet == null)
            throw new KeyNotFoundException($"Wallet for card {request.CardId} not found");

        return ApiResponse<WalletDto>.Ok(new WalletDto(
            wallet.Id,
            wallet.CardId,
            wallet.Card.MaskedCardNumber,
            wallet.Balance,
            wallet.AvailableBalance,
            wallet.Currency));
    }
}

public class GetTransactionsQueryHandler
    : IRequestHandler<GetTransactionsQuery, ApiResponse<List<TransactionDto>>>
{
    private readonly IAppDbContext _context;

    public GetTransactionsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<TransactionDto>>> Handle(
        GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Transactions
            .Include(t => t.Card)
            .ThenInclude(c => c.User)
            .Include(t => t.Merchant)
            .AsQueryable();

        if (request.CardId.HasValue)
            query = query.Where(t => t.CardId == request.CardId);

        if (!string.IsNullOrEmpty(request.ReferenceNumber))
            query = query.Where(t =>
                t.ReferenceNumber.Contains(request.ReferenceNumber));

        if (!string.IsNullOrEmpty(request.MerchantName))
            query = query.Where(t =>
                t.Merchant != null &&
                t.Merchant.Name.Contains(request.MerchantName));

        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<Domain.Enums.TransactionStatus>(request.Status, out var status))
            query = query.Where(t => t.Status == status);

        if (request.FromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= request.FromDate);

        if (request.ToDate.HasValue)
            query = query.Where(t => t.CreatedAt <= request.ToDate);

        var transactions = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TransactionDto(
                t.Id,
                t.ReferenceNumber,
                t.Card.MaskedCardNumber,
                t.Card.User.FirstName + " " + t.Card.User.LastName,
                t.Type,
                t.Status,
                t.Amount,
                t.BalanceAfter,
                t.Currency,
                t.Merchant != null ? t.Merchant.Name : null,
                t.Merchant != null ? t.Merchant.Category : null,
                t.IsInternational,
                t.Description,
                t.FailureReason,
                t.CreatedAt))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<TransactionDto>>.Ok(transactions);
    }
}