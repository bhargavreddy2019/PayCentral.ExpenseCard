namespace PayCentral.Application.Reports;

public record TransactionReportDto(
    string ReferenceNumber,
    string MaskedCardNumber,
    string CardholderName,
    string TransactionType,
    string Status,
    decimal Amount,
    decimal BalanceAfter,
    string Currency,
    string? MerchantName,
    string? MerchantCategory,
    bool IsInternational,
    string? Description,
    DateTime CreatedAt
);

public record FraudReportDto(
    string MaskedCardNumber,
    string CardholderName,
    string AlertType,
    string Reason,
    string Severity,
    bool IsResolved,
    DateTime? ResolvedAt,
    string? ResolvedBy,
    DateTime CreatedAt
);

public record CardReportDto(
    string MaskedCardNumber,
    string CardholderName,
    string Email,
    string Status,
    decimal Balance,
    decimal AvailableBalance,
    string Currency,
    DateTime ExpiryDate,
    DateTime? ActivatedAt,
    DateTime CreatedAt,
    int TotalTransactions
);

public record DailySummaryDto(
    DateTime Date,
    int TotalTransactions,
    decimal TotalAmount,
    int TotalPurchases,
    decimal TotalPurchaseAmount,
    int TotalRefunds,
    decimal TotalRefundAmount,
    int TotalLoads,
    decimal TotalLoadAmount,
    int FraudAlertsRaised,
    int NewCardsIssued
);