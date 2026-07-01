using PayCentral.Domain.Common;
using PayCentral.Domain.Enums;

namespace PayCentral.Domain.Entities;

public class CardStatusHistory : BaseEntity
{
    public Guid CardId { get; set; }
    public CardStatus FromStatus { get; set; }
    public CardStatus ToStatus { get; set; }
    public string? Reason { get; set; }
    public string ChangedBy { get; set; } = string.Empty;

    // Navigation
    public Card Card { get; set; } = null!;
}