using PayCentral.Domain.Common;

namespace PayCentral.Domain.Entities;

public class Merchant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CountryCode { get; set; } = "ZA";
    public string? City { get; set; }

    // Navigation
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}