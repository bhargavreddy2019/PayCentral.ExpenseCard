using FluentAssertions;
using PayCentral.Application.Fraud;
using PayCentral.Domain.Entities;
using PayCentral.Domain.Enums;
using PayCentral.Tests.Helpers;

namespace PayCentral.Tests.Fraud;

public class FraudRuleTests
{
    // ─── International Transaction Rule ───────────────────────────────

    [Fact]
    public async Task InternationalTransactionRule_ShouldFlag_WhenPurchaseIsInternational()
    {
        // Arrange
        var rule = new InternationalTransactionRule();
        var card = BuildCard();
        var transaction = BuildTransaction(
            TransactionType.Purchase, 500, isInternational: true);

        // Act
        var result = await rule.EvaluateAsync(transaction, card);

        // Assert
        result.IsViolated.Should().BeTrue();
        result.Severity.Should().Be(FraudSeverity.Medium);
        result.Reason.Should().Contain("International");
    }

    [Fact]
    public async Task InternationalTransactionRule_ShouldNotFlag_WhenPurchaseIsLocal()
    {
        // Arrange
        var rule = new InternationalTransactionRule();
        var card = BuildCard();
        var transaction = BuildTransaction(
            TransactionType.Purchase, 500, isInternational: false);

        // Act
        var result = await rule.EvaluateAsync(transaction, card);

        // Assert
        result.IsViolated.Should().BeFalse();
    }

    [Fact]
    public async Task InternationalTransactionRule_ShouldNotFlag_WhenNotAPurchase()
    {
        // Arrange
        var rule = new InternationalTransactionRule();
        var card = BuildCard();
        var transaction = BuildTransaction(
            TransactionType.LoadFunds, 500, isInternational: true);

        // Act
        var result = await rule.EvaluateAsync(transaction, card);

        // Assert
        result.IsViolated.Should().BeFalse();
    }

    // ─── Large Spend Rule ─────────────────────────────────────────────

    [Fact]
    public async Task LargeSpendRule_ShouldFlag_WhenSpendExceedsThreshold()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var card = BuildCard();
        context.Cards.Add(card);

        // Add existing transaction of R15,000 within last 10 minutes
        context.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(),
            CardId = card.Id,
            Type = TransactionType.Purchase,
            Status = TransactionStatus.Completed,
            Amount = 15000m,
            ReferenceNumber = "TXN001",
            Currency = "ZAR",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            CreatedBy = "Test"
        });
        await context.SaveChangesAsync();

        var rule = new LargeSpendRule(context);

        // New transaction of R6,000 — total R21,000 exceeds R20,000
        var transaction = BuildTransaction(TransactionType.Purchase, 6000);
        transaction.CardId = card.Id;

        // Act
        var result = await rule.EvaluateAsync(transaction, card);

        // Assert
        result.IsViolated.Should().BeTrue();
        result.Severity.Should().Be(FraudSeverity.Critical);
        result.Reason.Should().Contain("R21,000.00");
    }

    [Fact]
    public async Task LargeSpendRule_ShouldNotFlag_WhenSpendBelowThreshold()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var card = BuildCard();
        context.Cards.Add(card);

        context.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(),
            CardId = card.Id,
            Type = TransactionType.Purchase,
            Status = TransactionStatus.Completed,
            Amount = 5000m,
            ReferenceNumber = "TXN002",
            Currency = "ZAR",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            CreatedBy = "Test"
        });
        await context.SaveChangesAsync();

        var rule = new LargeSpendRule(context);
        var transaction = BuildTransaction(TransactionType.Purchase, 1000);
        transaction.CardId = card.Id;

        // Act
        var result = await rule.EvaluateAsync(transaction, card);

        // Assert
        result.IsViolated.Should().BeFalse();
    }

    [Fact]
    public async Task LargeSpendRule_ShouldNotFlag_WhenTransactionIsNotPurchase()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var rule = new LargeSpendRule(context);
        var card = BuildCard();
        var transaction = BuildTransaction(TransactionType.LoadFunds, 25000);

        // Act
        var result = await rule.EvaluateAsync(transaction, card);

        // Assert
        result.IsViolated.Should().BeFalse();
    }

    // ─── Rapid Purchase Rule ──────────────────────────────────────────

    [Fact]
    public async Task RapidPurchaseRule_ShouldFlag_WhenThreePurchasesInOneMinute()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var card = BuildCard();
        context.Cards.Add(card);

        // Add 3 purchases within last 60 seconds
        for (int i = 0; i < 3; i++)
        {
            context.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                CardId = card.Id,
                Type = TransactionType.Purchase,
                Status = TransactionStatus.Completed,
                Amount = 100m,
                ReferenceNumber = $"TXN00{i}",
                Currency = "ZAR",
                CreatedAt = DateTime.UtcNow.AddSeconds(-10 * (i + 1)),
                CreatedBy = "Test"
            });
        }
        await context.SaveChangesAsync();

        var rule = new RapidPurchaseRule(context);
        var transaction = BuildTransaction(TransactionType.Purchase, 100);
        transaction.CardId = card.Id;

        // Act
        var result = await rule.EvaluateAsync(transaction, card);

        // Assert
        result.IsViolated.Should().BeTrue();
        result.Severity.Should().Be(FraudSeverity.High);
    }

    [Fact]
    public async Task RapidPurchaseRule_ShouldNotFlag_WhenPurchasesAreFarApart()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var card = BuildCard();
        context.Cards.Add(card);

        // Add purchases outside the 60 second window
        for (int i = 0; i < 3; i++)
        {
            context.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                CardId = card.Id,
                Type = TransactionType.Purchase,
                Status = TransactionStatus.Completed,
                Amount = 100m,
                ReferenceNumber = $"TXN00{i}",
                Currency = "ZAR",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10 * (i + 1)),
                CreatedBy = "Test"
            });
        }
        await context.SaveChangesAsync();

        var rule = new RapidPurchaseRule(context);
        var transaction = BuildTransaction(TransactionType.Purchase, 100);
        transaction.CardId = card.Id;

        // Act
        var result = await rule.EvaluateAsync(transaction, card);

        // Assert
        result.IsViolated.Should().BeFalse();
    }

    // ─── Wallet Tests ─────────────────────────────────────────────────

    [Fact]
    public void Wallet_CanDebit_ShouldReturnTrue_WhenSufficientFunds()
    {
        // Arrange
        var wallet = new Wallet
        {
            Balance = 1000m,
            AvailableBalance = 1000m,
            Currency = "ZAR"
        };

        // Act
        var result = wallet.CanDebit(500m);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Wallet_CanDebit_ShouldReturnFalse_WhenInsufficientFunds()
    {
        // Arrange
        var wallet = new Wallet
        {
            Balance = 100m,
            AvailableBalance = 100m,
            Currency = "ZAR"
        };

        // Act
        var result = wallet.CanDebit(500m);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Wallet_CanDebit_ShouldReturnFalse_WhenAmountIsZero()
    {
        // Arrange
        var wallet = new Wallet
        {
            Balance = 1000m,
            AvailableBalance = 1000m,
            Currency = "ZAR"
        };

        // Act
        var result = wallet.CanDebit(0m);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Wallet_CanDebit_ShouldReturnFalse_WhenAmountIsNegative()
    {
        // Arrange
        var wallet = new Wallet
        {
            Balance = 1000m,
            AvailableBalance = 1000m,
            Currency = "ZAR"
        };

        // Act
        var result = wallet.CanDebit(-100m);

        // Assert
        result.Should().BeFalse();
    }

    // ─── Card Status Transition Tests ─────────────────────────────────

    [Fact]
    public void Card_CanTransitionTo_ShouldReturnFalse_WhenCardIsClosed()
    {
        // Arrange
        var card = BuildCard();
        card.Status = CardStatus.Closed;

        // Act & Assert
        card.CanTransitionTo(CardStatus.Active).Should().BeFalse();
        card.CanTransitionTo(CardStatus.Blocked).Should().BeFalse();
        card.CanTransitionTo(CardStatus.Suspended).Should().BeFalse();
    }

    [Fact]
    public void Card_CanTransitionTo_ShouldReturnFalse_WhenPendingToBlocked()
    {
        // Arrange
        var card = BuildCard();
        card.Status = CardStatus.Pending;

        // Act
        var result = card.CanTransitionTo(CardStatus.Blocked);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Card_CanTransitionTo_ShouldReturnTrue_WhenActiveToBlocked()
    {
        // Arrange
        var card = BuildCard();
        card.Status = CardStatus.Active;

        // Act
        var result = card.CanTransitionTo(CardStatus.Blocked);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Card_MaskCardNumber_ShouldMaskCorrectly()
    {
        // Arrange
        var cardNumber = "4111111111111111";

        // Act
        var masked = Card.MaskCardNumber(cardNumber);

        // Assert
        masked.Should().Be("**** **** **** 1111");
    }

    [Fact]
    public void Card_GenerateCardNumber_ShouldStartWithFour()
    {
        // Act
        var cardNumber = Card.GenerateCardNumber();

        // Assert
        cardNumber.Should().StartWith("4");
        cardNumber.Should().HaveLength(16);
    }

    // ─── Helpers ──────────────────────────────────────────────────────

    private static Card BuildCard() => new()
    {
        Id = Guid.NewGuid(),
        CardNumber = "4111111111111111",
        MaskedCardNumber = "**** **** **** 1111",
        Status = CardStatus.Active,
        ExpiryDate = DateTime.UtcNow.AddYears(3),
        CreatedBy = "Test",
        UserId = Guid.NewGuid(),
        Wallet = new Wallet
        {
            Balance = 10000m,
            AvailableBalance = 10000m,
            Currency = "ZAR",
            RowVersion = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } // Required for InMemory DB
        }
    };

    private static Transaction BuildTransaction(
        TransactionType type,
        decimal amount,
        bool isInternational = false) => new()
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = Transaction.GenerateReference(),
            CardId = Guid.NewGuid(),
            Type = type,
            Status = TransactionStatus.Completed,
            Amount = amount,
            Currency = "ZAR",
            IsInternational = isInternational,
            CreatedBy = "Test",
            CreatedAt = DateTime.UtcNow
        };
}