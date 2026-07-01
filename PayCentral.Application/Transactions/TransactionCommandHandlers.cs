using MediatR;
using Microsoft.EntityFrameworkCore;
using PayCentral.Application.Common.Interfaces;
using PayCentral.Application.Common.Models;
using PayCentral.Domain.Entities;
using PayCentral.Domain.Enums;

namespace PayCentral.Application.Transactions;

public class LoadFundsCommandHandler : IRequestHandler<LoadFundsCommand, ApiResponse<TransactionDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public LoadFundsCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<TransactionDto>> Handle(
        LoadFundsCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existing = await _context.Transactions
                .FirstOrDefaultAsync(t => t.IdempotencyKey == request.IdempotencyKey,
                    cancellationToken);
            if (existing != null)
                throw new ArgumentException("Duplicate request detected");
        }

        var card = await _context.Cards
            .Include(c => c.User)
            .Include(c => c.Wallet)
            .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken);

        if (card == null)
            throw new KeyNotFoundException($"Card {request.CardId} not found");

        if (card.Status == CardStatus.Blocked)
            throw new ArgumentException("Cannot load funds to a blocked card");

        if (card.Status == CardStatus.Closed)
            throw new ArgumentException("Cannot load funds to a closed card");

        card.Wallet.Balance += request.Amount;
        card.Wallet.AvailableBalance += request.Amount;
        card.Wallet.UpdatedAt = DateTime.UtcNow;

        var transaction = new Transaction
        {
            ReferenceNumber = Transaction.GenerateReference(),
            CardId = card.Id,
            Type = TransactionType.LoadFunds,
            Status = TransactionStatus.Completed,
            Amount = request.Amount,
            BalanceAfter = card.Wallet.Balance,
            Currency = card.Wallet.Currency,
            Description = request.Description ?? "Fund load",
            IdempotencyKey = request.IdempotencyKey,
            IsInternational = false,
            CreatedBy = _currentUser.Email ?? "System"
        };

        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<TransactionDto>.Ok(MapToDto(transaction, card),
            "Funds loaded successfully");
    }

    private static TransactionDto MapToDto(Transaction t, Card card) => new(
        t.Id, t.ReferenceNumber, card.MaskedCardNumber,
        card.User.FullName, t.Type, t.Status, t.Amount,
        t.BalanceAfter, t.Currency, null, null,
        t.IsInternational, t.Description, t.FailureReason, t.CreatedAt);
}

public class PurchaseCommandHandler : IRequestHandler<PurchaseCommand, ApiResponse<TransactionDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFraudService _fraudService;

    public PurchaseCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUser,
        IFraudService fraudService)
    {
        _context = context;
        _currentUser = currentUser;
        _fraudService = fraudService;
    }

    public async Task<ApiResponse<TransactionDto>> Handle(
        PurchaseCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existing = await _context.Transactions
                .FirstOrDefaultAsync(t => t.IdempotencyKey == request.IdempotencyKey,
                    cancellationToken);
            if (existing != null)
                throw new ArgumentException("Duplicate request detected");
        }

        var card = await _context.Cards
            .Include(c => c.User)
            .Include(c => c.Wallet)
            .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken);

        if (card == null)
            throw new KeyNotFoundException($"Card {request.CardId} not found");

        if (card.Status == CardStatus.Blocked)
            throw new ArgumentException("Transaction declined — card is blocked");

        if (card.Status == CardStatus.Suspended)
            throw new ArgumentException("Transaction declined — card is suspended");

        if (card.Status == CardStatus.Closed)
            throw new ArgumentException("Transaction declined — card is closed");

        if (!card.Wallet.CanDebit(request.Amount))
            throw new ArgumentException(
                $"Insufficient funds. Available balance: {card.Wallet.AvailableBalance:C}");

        Merchant? merchant = null;
        if (request.MerchantId.HasValue)
        {
            merchant = await _context.Merchants
                .FirstOrDefaultAsync(m => m.Id == request.MerchantId, cancellationToken);
        }

        card.Wallet.Balance -= request.Amount;
        card.Wallet.AvailableBalance -= request.Amount;
        card.Wallet.UpdatedAt = DateTime.UtcNow;

        var transaction = new Transaction
        {
            ReferenceNumber = Transaction.GenerateReference(),
            CardId = card.Id,
            MerchantId = request.MerchantId,
            Type = TransactionType.Purchase,
            Status = TransactionStatus.Completed,
            Amount = request.Amount,
            BalanceAfter = card.Wallet.Balance,
            Currency = card.Wallet.Currency,
            Description = request.Description ?? "Purchase",
            IsInternational = request.IsInternational,
            IdempotencyKey = request.IdempotencyKey,
            CreatedBy = _currentUser.Email ?? "System"
        };

        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Run fraud detection
        await _fraudService.EvaluateTransactionAsync(transaction, card);

        return ApiResponse<TransactionDto>.Ok(new TransactionDto(
            transaction.Id, transaction.ReferenceNumber,
            card.MaskedCardNumber, card.User.FullName,
            transaction.Type, transaction.Status,
            transaction.Amount, transaction.BalanceAfter,
            transaction.Currency,
            merchant?.Name, merchant?.Category,
            transaction.IsInternational, transaction.Description,
            transaction.FailureReason, transaction.CreatedAt),
            "Purchase completed successfully");
    }
}

public class RefundCommandHandler : IRequestHandler<RefundCommand, ApiResponse<TransactionDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RefundCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<TransactionDto>> Handle(
        RefundCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existing = await _context.Transactions
                .FirstOrDefaultAsync(t => t.IdempotencyKey == request.IdempotencyKey,
                    cancellationToken);
            if (existing != null)
                throw new ArgumentException("Duplicate request detected");
        }

        var originalTransaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.ReferenceNumber == request.OriginalReferenceNumber,
                cancellationToken);

        if (originalTransaction == null)
            throw new KeyNotFoundException(
                $"Transaction {request.OriginalReferenceNumber} not found");

        if (originalTransaction.Status != TransactionStatus.Completed)
            throw new ArgumentException("Only completed transactions can be refunded");

        var card = await _context.Cards
            .Include(c => c.User)
            .Include(c => c.Wallet)
            .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken);

        if (card == null)
            throw new KeyNotFoundException($"Card {request.CardId} not found");

        card.Wallet.Balance += request.Amount;
        card.Wallet.AvailableBalance += request.Amount;
        card.Wallet.UpdatedAt = DateTime.UtcNow;

        var transaction = new Transaction
        {
            ReferenceNumber = Transaction.GenerateReference(),
            CardId = card.Id,
            Type = TransactionType.Refund,
            Status = TransactionStatus.Completed,
            Amount = request.Amount,
            BalanceAfter = card.Wallet.Balance,
            Currency = card.Wallet.Currency,
            Description = request.Description ??
                $"Refund for {request.OriginalReferenceNumber}",
            IdempotencyKey = request.IdempotencyKey,
            CreatedBy = _currentUser.Email ?? "System"
        };

        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<TransactionDto>.Ok(new TransactionDto(
            transaction.Id, transaction.ReferenceNumber,
            card.MaskedCardNumber, card.User.FullName,
            transaction.Type, transaction.Status,
            transaction.Amount, transaction.BalanceAfter,
            transaction.Currency, null, null,
            false, transaction.Description,
            null, transaction.CreatedAt),
            "Refund processed successfully");
    }
}

public class ReversalCommandHandler : IRequestHandler<ReversalCommand, ApiResponse<TransactionDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ReversalCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<TransactionDto>> Handle(
        ReversalCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existing = await _context.Transactions
                .FirstOrDefaultAsync(t => t.IdempotencyKey == request.IdempotencyKey,
                    cancellationToken);
            if (existing != null)
                throw new ArgumentException("Duplicate request detected");
        }

        var originalTransaction = await _context.Transactions
            .Include(t => t.Card)
            .ThenInclude(c => c.User)
            .Include(t => t.Card)
            .ThenInclude(c => c.Wallet)
            .FirstOrDefaultAsync(t =>
                t.ReferenceNumber == request.OriginalReferenceNumber,
                cancellationToken);

        if (originalTransaction == null)
            throw new KeyNotFoundException(
                $"Transaction {request.OriginalReferenceNumber} not found");

        if (originalTransaction.Status != TransactionStatus.Completed)
            throw new ArgumentException("Only completed transactions can be reversed");

        if (originalTransaction.Type != TransactionType.Purchase)
            throw new ArgumentException("Only purchase transactions can be reversed");

        var card = originalTransaction.Card;

        card.Wallet.Balance += originalTransaction.Amount;
        card.Wallet.AvailableBalance += originalTransaction.Amount;
        card.Wallet.UpdatedAt = DateTime.UtcNow;

        originalTransaction.Status = TransactionStatus.Reversed;

        var reversalTransaction = new Transaction
        {
            ReferenceNumber = Transaction.GenerateReference(),
            CardId = card.Id,
            Type = TransactionType.Reversal,
            Status = TransactionStatus.Completed,
            Amount = originalTransaction.Amount,
            BalanceAfter = card.Wallet.Balance,
            Currency = card.Wallet.Currency,
            Description = $"Reversal of {request.OriginalReferenceNumber}",
            IdempotencyKey = request.IdempotencyKey,
            CreatedBy = _currentUser.Email ?? "System"
        };

        await _context.Transactions.AddAsync(reversalTransaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<TransactionDto>.Ok(new TransactionDto(
            reversalTransaction.Id, reversalTransaction.ReferenceNumber,
            card.MaskedCardNumber, card.User.FullName,
            reversalTransaction.Type, reversalTransaction.Status,
            reversalTransaction.Amount, reversalTransaction.BalanceAfter,
            reversalTransaction.Currency, null, null,
            false, reversalTransaction.Description,
            null, reversalTransaction.CreatedAt),
            "Transaction reversed successfully");
    }
}