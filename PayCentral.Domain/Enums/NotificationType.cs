namespace PayCentral.Domain.Enums;

public enum NotificationType
{
    CardCreated = 1,
    CardBlocked = 2,
    CardUnblocked = 3,
    FundsLoaded = 4,
    PurchaseCompleted = 5,
    RefundProcessed = 6,
    LowBalance = 7,
    FraudAlert = 8
}