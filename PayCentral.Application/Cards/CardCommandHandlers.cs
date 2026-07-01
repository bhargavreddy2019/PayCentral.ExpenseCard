using MediatR;
using Microsoft.EntityFrameworkCore;
using PayCentral.Application.Common.Interfaces;
using PayCentral.Application.Common.Models;
using PayCentral.Domain.Entities;
using PayCentral.Domain.Enums;

namespace PayCentral.Application.Cards;

public class CreateCardCommandHandler : IRequestHandler<CreateCardCommand, ApiResponse<CardDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateCardCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<CardDto>> Handle(
        CreateCardCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new KeyNotFoundException($"User {request.UserId} not found");

        var cardNumber = Card.GenerateCardNumber();

        var card = new Card
        {
            CardNumber = cardNumber,
            MaskedCardNumber = Card.MaskCardNumber(cardNumber),
            UserId = request.UserId,
            Status = CardStatus.Pending,
            ExpiryDate = DateTime.UtcNow.AddYears(3),
            CreatedBy = _currentUser.Email ?? "System",
            Wallet = new Wallet
            {
                Balance = 0,
                AvailableBalance = 0,
                Currency = "ZAR",
                CreatedBy = _currentUser.Email ?? "System"
            }
        };

        await _context.Cards.AddAsync(card, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<CardDto>.Ok(new CardDto(
            card.Id,
            card.MaskedCardNumber,
            user.FullName,
            user.Email,
            card.Status,
            0, 0, "ZAR",
            card.ExpiryDate,
            null, null, null,
            card.CreatedAt),
            "Card created successfully");
    }
}

public class ActivateCardCommandHandler : IRequestHandler<ActivateCardCommand, ApiResponse<CardDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ActivateCardCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<CardDto>> Handle(
        ActivateCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _context.Cards
            .Include(c => c.User)
            .Include(c => c.Wallet)
            .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken);

        if (card == null)
            throw new KeyNotFoundException($"Card {request.CardId} not found");

        if (!card.CanTransitionTo(CardStatus.Active))
            throw new ArgumentException($"Card cannot be activated from {card.Status} status");

        var history = new CardStatusHistory
        {
            CardId = card.Id,
            FromStatus = card.Status,
            ToStatus = CardStatus.Active,
            Reason = "Card activated",
            ChangedBy = _currentUser.Email ?? "System"
        };

        card.Status = CardStatus.Active;
        card.ActivatedAt = DateTime.UtcNow;
        card.UpdatedAt = DateTime.UtcNow;
        card.UpdatedBy = _currentUser.Email;

        await _context.CardStatusHistories.AddAsync(history, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<CardDto>.Ok(new CardDto(
            card.Id, card.MaskedCardNumber,
            card.User.FullName, card.User.Email,
            card.Status, card.Wallet.Balance,
            card.Wallet.AvailableBalance, card.Wallet.Currency,
            card.ExpiryDate, card.ActivatedAt,
            card.BlockedAt, card.BlockReason,
            card.CreatedAt), "Card activated successfully");
    }
}

public class BlockCardCommandHandler : IRequestHandler<BlockCardCommand, ApiResponse<CardDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public BlockCardCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<CardDto>> Handle(
        BlockCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _context.Cards
            .Include(c => c.User)
            .Include(c => c.Wallet)
            .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken);

        if (card == null)
            throw new KeyNotFoundException($"Card {request.CardId} not found");

        if (!card.CanTransitionTo(CardStatus.Blocked))
            throw new ArgumentException($"Card cannot be blocked from {card.Status} status");

        var history = new CardStatusHistory
        {
            CardId = card.Id,
            FromStatus = card.Status,
            ToStatus = CardStatus.Blocked,
            Reason = request.Reason,
            ChangedBy = _currentUser.Email ?? "System"
        };

        card.Status = CardStatus.Blocked;
        card.BlockedAt = DateTime.UtcNow;
        card.BlockReason = request.Reason;
        card.UpdatedAt = DateTime.UtcNow;
        card.UpdatedBy = _currentUser.Email;

        await _context.CardStatusHistories.AddAsync(history, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<CardDto>.Ok(new CardDto(
            card.Id, card.MaskedCardNumber,
            card.User.FullName, card.User.Email,
            card.Status, card.Wallet.Balance,
            card.Wallet.AvailableBalance, card.Wallet.Currency,
            card.ExpiryDate, card.ActivatedAt,
            card.BlockedAt, card.BlockReason,
            card.CreatedAt), "Card blocked successfully");
    }
}

public class UnblockCardCommandHandler : IRequestHandler<UnblockCardCommand, ApiResponse<CardDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UnblockCardCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<CardDto>> Handle(
        UnblockCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _context.Cards
            .Include(c => c.User)
            .Include(c => c.Wallet)
            .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken);

        if (card == null)
            throw new KeyNotFoundException($"Card {request.CardId} not found");

        if (card.Status != CardStatus.Blocked)
            throw new ArgumentException("Only blocked cards can be unblocked");

        var history = new CardStatusHistory
        {
            CardId = card.Id,
            FromStatus = card.Status,
            ToStatus = CardStatus.Active,
            Reason = "Card unblocked",
            ChangedBy = _currentUser.Email ?? "System"
        };

        card.Status = CardStatus.Active;
        card.BlockedAt = null;
        card.BlockReason = null;
        card.UpdatedAt = DateTime.UtcNow;
        card.UpdatedBy = _currentUser.Email;

        await _context.CardStatusHistories.AddAsync(history, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<CardDto>.Ok(new CardDto(
            card.Id, card.MaskedCardNumber,
            card.User.FullName, card.User.Email,
            card.Status, card.Wallet.Balance,
            card.Wallet.AvailableBalance, card.Wallet.Currency,
            card.ExpiryDate, card.ActivatedAt,
            card.BlockedAt, card.BlockReason,
            card.CreatedAt), "Card unblocked successfully");
    }
}

public class SuspendCardCommandHandler : IRequestHandler<SuspendCardCommand, ApiResponse<CardDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SuspendCardCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<CardDto>> Handle(
        SuspendCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _context.Cards
            .Include(c => c.User)
            .Include(c => c.Wallet)
            .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken);

        if (card == null)
            throw new KeyNotFoundException($"Card {request.CardId} not found");

        if (!card.CanTransitionTo(CardStatus.Suspended))
            throw new ArgumentException($"Card cannot be suspended from {card.Status} status");

        var history = new CardStatusHistory
        {
            CardId = card.Id,
            FromStatus = card.Status,
            ToStatus = CardStatus.Suspended,
            Reason = request.Reason,
            ChangedBy = _currentUser.Email ?? "System"
        };

        card.Status = CardStatus.Suspended;
        card.UpdatedAt = DateTime.UtcNow;
        card.UpdatedBy = _currentUser.Email;

        await _context.CardStatusHistories.AddAsync(history, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<CardDto>.Ok(new CardDto(
            card.Id, card.MaskedCardNumber,
            card.User.FullName, card.User.Email,
            card.Status, card.Wallet.Balance,
            card.Wallet.AvailableBalance, card.Wallet.Currency,
            card.ExpiryDate, card.ActivatedAt,
            card.BlockedAt, card.BlockReason,
            card.CreatedAt), "Card suspended successfully");
    }
}

public class CloseCardCommandHandler : IRequestHandler<CloseCardCommand, ApiResponse<CardDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CloseCardCommandHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<CardDto>> Handle(
        CloseCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _context.Cards
            .Include(c => c.User)
            .Include(c => c.Wallet)
            .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken);

        if (card == null)
            throw new KeyNotFoundException($"Card {request.CardId} not found");

        if (!card.CanTransitionTo(CardStatus.Closed))
            throw new ArgumentException($"Card cannot be closed from {card.Status} status");

        var history = new CardStatusHistory
        {
            CardId = card.Id,
            FromStatus = card.Status,
            ToStatus = CardStatus.Closed,
            Reason = request.Reason,
            ChangedBy = _currentUser.Email ?? "System"
        };

        card.Status = CardStatus.Closed;
        card.ClosedAt = DateTime.UtcNow;
        card.UpdatedAt = DateTime.UtcNow;
        card.UpdatedBy = _currentUser.Email;

        await _context.CardStatusHistories.AddAsync(history, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<CardDto>.Ok(new CardDto(
            card.Id, card.MaskedCardNumber,
            card.User.FullName, card.User.Email,
            card.Status, card.Wallet.Balance,
            card.Wallet.AvailableBalance, card.Wallet.Currency,
            card.ExpiryDate, card.ActivatedAt,
            card.BlockedAt, card.BlockReason,
            card.CreatedAt), "Card closed successfully");
    }
}