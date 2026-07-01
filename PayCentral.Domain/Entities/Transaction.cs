using PayCentral.Domain.Common;
using PayCentral.Domain.Enums;

namespace PayCentral.Domain.Entities;

public class Transaction : BaseEntity
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public Guid CardId { get; set; }
    public Guid? MerchantId { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Currency { get; set; } = "ZAR";
    public string? Description { get; set; }
    public string? FailureReason { get; set; }
    public bool IsInternational { get; set; } = false;
    public string? IdempotencyKey { get; set; }

    // Navigation
    public Card Card { get; set; } = null!;
    public Merchant? Merchant { get; set; }

    // Domain method
    public static string GenerateReference()
    {
        return $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid().ToString()[..6].ToUpper()}";
    }
}