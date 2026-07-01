using PayCentral.Domain.Common;
using PayCentral.Domain.Enums;

namespace PayCentral.Domain.Entities;

public class FraudAlert : BaseEntity
{
    public Guid CardId { get; set; }
    public Guid? TransactionId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public FraudSeverity Severity { get; set; }
    public bool IsResolved { get; set; } = false;
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }

    // Navigation
    public Card Card { get; set; } = null!;
}