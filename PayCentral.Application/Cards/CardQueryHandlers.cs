using MediatR;
using Microsoft.EntityFrameworkCore;
using PayCentral.Application.Common.Interfaces;
using PayCentral.Application.Common.Models;

namespace PayCentral.Application.Cards;

public class GetCardsQueryHandler : IRequestHandler<GetCardsQuery, ApiResponse<List<CardDto>>>
{
    private readonly IAppDbContext _context;

    public GetCardsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<CardDto>>> Handle(
        GetCardsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Cards
            .Include(c => c.User)
            .Include(c => c.Wallet)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(c =>
                c.CardNumber.Contains(term) ||
                c.User.FirstName.ToLower().Contains(term) ||
                c.User.LastName.ToLower().Contains(term) ||
                c.User.Email.ToLower().Contains(term));
        }

        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<Domain.Enums.CardStatus>(request.Status, out var status))
        {
            query = query.Where(c => c.Status == status);
        }

        var cards = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CardDto(
                c.Id,
                c.MaskedCardNumber,
                c.User.FirstName + " " + c.User.LastName,
                c.User.Email,
                c.Status,
                c.Wallet.Balance,
                c.Wallet.AvailableBalance,
                c.Wallet.Currency,
                c.ExpiryDate,
                c.ActivatedAt,
                c.BlockedAt,
                c.BlockReason,
                c.CreatedAt))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<CardDto>>.Ok(cards);
    }
}

public class GetCardByIdQueryHandler : IRequestHandler<GetCardByIdQuery, ApiResponse<CardDetailDto>>
{
    private readonly IAppDbContext _context;

    public GetCardByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<CardDetailDto>> Handle(
        GetCardByIdQuery request, CancellationToken cancellationToken)
    {
        var card = await _context.Cards
            .Include(c => c.User)
            .Include(c => c.Wallet)
            .Include(c => c.StatusHistory)
            .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken);

        if (card == null)
            throw new KeyNotFoundException($"Card {request.CardId} not found");

        var dto = new CardDetailDto(
            card.Id,
            card.MaskedCardNumber,
            card.User.FullName,
            card.User.Email,
            card.User.PhoneNumber ?? string.Empty,
            card.Status,
            card.Wallet.Balance,
            card.Wallet.AvailableBalance,
            card.Wallet.Currency,
            card.ExpiryDate,
            card.ActivatedAt,
            card.BlockedAt,
            card.BlockReason,
            card.CreatedAt,
            card.StatusHistory
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new CardStatusHistoryDto(
                    h.FromStatus,
                    h.ToStatus,
                    h.Reason,
                    h.ChangedBy,
                    h.CreatedAt))
                .ToList());

        return ApiResponse<CardDetailDto>.Ok(dto);
    }
}

public class GetCardsByUserQueryHandler
    : IRequestHandler<GetCardsByUserQuery, ApiResponse<List<CardDto>>>
{
    private readonly IAppDbContext _context;

    public GetCardsByUserQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<CardDto>>> Handle(
        GetCardsByUserQuery request, CancellationToken cancellationToken)
    {
        var cards = await _context.Cards
            .Include(c => c.User)
            .Include(c => c.Wallet)
            .Where(c => c.UserId == request.UserId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CardDto(
                c.Id,
                c.MaskedCardNumber,
                c.User.FirstName + " " + c.User.LastName,
                c.User.Email,
                c.Status,
                c.Wallet.Balance,
                c.Wallet.AvailableBalance,
                c.Wallet.Currency,
                c.ExpiryDate,
                c.ActivatedAt,
                c.BlockedAt,
                c.BlockReason,
                c.CreatedAt))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<CardDto>>.Ok(cards);
    }
}