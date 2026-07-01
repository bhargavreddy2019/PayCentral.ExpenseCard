using PayCentral.Domain.Entities;

namespace PayCentral.Application.Common.Interfaces;

public interface IFraudService
{
    Task EvaluateTransactionAsync(Transaction transaction, Card card);
}