namespace PayCentral.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICardRepository Cards { get; }
    ITransactionRepository Transactions { get; }
    IFraudAlertRepository FraudAlerts { get; }
    Task<int> CommitAsync();
}