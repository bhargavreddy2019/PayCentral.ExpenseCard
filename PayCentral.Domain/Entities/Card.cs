using PayCentral.Domain.Common;
using PayCentral.Domain.Enums;
using System.Transactions;

namespace PayCentral.Domain.Entities;

public class Card : BaseEntity
{
    public string CardNumber { get; set; } = string.Empty;
    public string MaskedCardNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public CardStatus Status { get; set; } = CardStatus.Pending;
    public DateTime ExpiryDate { get; set; }
    public string? BlockReason { get; set; }
    public DateTime? BlockedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Wallet Wallet { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<CardStatusHistory> StatusHistory { get; set; } = new List<CardStatusHistory>();
    public ICollection<FraudAlert> FraudAlerts { get; set; } = new List<FraudAlert>();

    // Domain method — card number generation
    public static string GenerateCardNumber()
    {
        var random = new Random();
        return $"4{string.Join("", Enumerable.Range(0, 15).Select(_ => random.Next(0, 10)))}";
    }

    public static string MaskCardNumber(string cardNumber)
    {
        if (cardNumber.Length < 4) return cardNumber;
        return $"**** **** **** {cardNumber[^4..]}";
    }

    // Domain rule — closed cards are terminal
    public bool CanTransitionTo(CardStatus newStatus)
    {
        if (Status == CardStatus.Closed) return false;
        if (Status == CardStatus.Pending && newStatus == CardStatus.Blocked) return false;
        return true;
    }
}