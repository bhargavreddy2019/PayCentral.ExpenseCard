using PayCentral.Domain.Entities;

namespace PayCentral.Application.Fraud;

public interface IFraudRule
{
    string AlertType { get; }
    Task<FraudRuleResult> EvaluateAsync(Transaction transaction, Card card);
}

public record FraudRuleResult(
    bool IsViolated,
    string Reason,
    Domain.Enums.FraudSeverity Severity
);