using PayCentral.Domain.Entities;
using PayCentral.Domain.Enums;

namespace PayCentral.Domain.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<Transaction?> GetByReferenceAsync(string referenceNumber);
    Task<IEnumerable<Transaction>> GetByCardIdAsync(Guid cardId);
    Task<IEnumerable<Transaction>> GetByCardIdAndDateRangeAsync(Guid cardId, DateTime from, DateTime to);
    Task<bool> ExistsAsync(string idempotencyKey);
    Task<IEnumerable<Transaction>> GetRecentByCardIdAsync(Guid cardId, int minutes);
    Task<int> GetFailedCountAsync(Guid cardId);
}