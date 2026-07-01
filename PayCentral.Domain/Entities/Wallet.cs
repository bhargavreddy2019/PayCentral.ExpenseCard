using PayCentral.Domain.Common;

namespace PayCentral.Domain.Entities;

public class Wallet : BaseEntity
{
    public Guid CardId { get; set; }
    public decimal Balance { get; set; } = 0;
    public decimal AvailableBalance { get; set; } = 0;
    public string Currency { get; set; } = "ZAR";
    public byte[] RowVersion { get; set; } = null!; // Optimistic concurrency

    // Navigation
    public Card Card { get; set; } = null!;

    // Domain rule — prevent negative balance
    public bool CanDebit(decimal amount)
    {
        return AvailableBalance >= amount && amount > 0;
    }
}