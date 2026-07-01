using PayCentral.Domain.Enums;

namespace PayCentral.Application.Transactions;

public record TransactionDto(
    Guid Id,
    string ReferenceNumber,
    string MaskedCardNumber,
    string CardholderName,
    TransactionType Type,
    TransactionStatus Status,
    decimal Amount,
    decimal BalanceAfter,
    string Currency,
    string? MerchantName,
    string? MerchantCategory,
    bool IsInternational,
    string? Description,
    string? FailureReason,
    DateTime CreatedAt
);

public record WalletDto(
    Guid Id,
    Guid CardId,
    string MaskedCardNumber,
    decimal Balance,
    decimal AvailableBalance,
    string Currency
);