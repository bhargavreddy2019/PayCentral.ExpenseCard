using PayCentral.Domain.Enums;

namespace PayCentral.Application.Fraud;

public record FraudAlertDto(
    Guid Id,
    string CardNumber,
    string CardholderName,
    string AlertType,
    string Reason,
    FraudSeverity Severity,
    bool IsResolved,
    DateTime? ResolvedAt,
    string? ResolvedBy,
    DateTime CreatedAt
);