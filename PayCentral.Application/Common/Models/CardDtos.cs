using PayCentral.Domain.Enums;

namespace PayCentral.Application.Cards;

public record CardDto(
    Guid Id,
    string MaskedCardNumber,
    string CardholderName,
    string Email,
    CardStatus Status,
    decimal Balance,
    decimal AvailableBalance,
    string Currency,
    DateTime ExpiryDate,
    DateTime? ActivatedAt,
    DateTime? BlockedAt,
    string? BlockReason,
    DateTime CreatedAt
);

public record CardStatusHistoryDto(
    CardStatus FromStatus,
    CardStatus ToStatus,
    string? Reason,
    string ChangedBy,
    DateTime ChangedAt
);

public record CardDetailDto(
    Guid Id,
    string MaskedCardNumber,
    string CardholderName,
    string Email,
    string PhoneNumber,
    CardStatus Status,
    decimal Balance,
    decimal AvailableBalance,
    string Currency,
    DateTime ExpiryDate,
    DateTime? ActivatedAt,
    DateTime? BlockedAt,
    string? BlockReason,
    DateTime CreatedAt,
    List<CardStatusHistoryDto> StatusHistory
);