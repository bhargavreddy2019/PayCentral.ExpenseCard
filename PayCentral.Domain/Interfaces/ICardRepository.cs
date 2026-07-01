using PayCentral.Domain.Entities;

namespace PayCentral.Domain.Interfaces;

public interface ICardRepository : IRepository<Card>
{
    Task<Card?> GetByCardNumberAsync(string cardNumber);
    Task<Card?> GetWithWalletAsync(Guid cardId);
    Task<IEnumerable<Card>> GetByUserIdAsync(Guid userId);
    Task<Card?> GetWithTransactionsAsync(Guid cardId);
}