namespace PayCentral.Domain.Enums;

public enum TransactionType
{
    Purchase = 1,
    Reversal = 2,
    Fee = 3,
    BalanceEnquiry = 4,
    Refund = 5,
    LoadFunds = 6,
    DebitFunds = 7
}