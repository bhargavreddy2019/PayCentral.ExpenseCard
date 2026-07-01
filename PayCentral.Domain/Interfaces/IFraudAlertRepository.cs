using PayCentral.Domain.Entities;

namespace PayCentral.Domain.Interfaces;

public interface IFraudAlertRepository : IRepository<FraudAlert>
{
    Task<IEnumerable<FraudAlert>> GetUnresolvedAsync();
    Task<IEnumerable<FraudAlert>> GetByCardIdAsync(Guid cardId);
}