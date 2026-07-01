using Microsoft.EntityFrameworkCore;
using PayCentral.Application.Common.Interfaces;
using PayCentral.Domain.Entities;
using PayCentral.Domain.Enums;

namespace PayCentral.Application.Fraud;

// Rule 1 — Large spend within 10 minutes
public class LargeSpendRule : IFraudRule
{
    private readonly IAppDbContext _context;
    private const decimal Threshold = 20000m;
    private const int WindowMinutes = 10;

    public LargeSpendRule(IAppDbContext context)
    {
        _context = context;
    }

    public string AlertType => "LARGE_SPEND";

    public async Task<FraudRuleResult> EvaluateAsync(
        Transaction transaction, Card card)
    {
        if (transaction.Type != TransactionType.Purchase)
            return new FraudRuleResult(false, string.Empty, FraudSeverity.Low);

        var windowStart = DateTime.UtcNow.AddMinutes(-WindowMinutes);

        var recentSpend = await _context.Transactions
            .Where(t => t.CardId == card.Id
                && t.Type == TransactionType.Purchase
                && t.Status == TransactionStatus.Completed
                && t.CreatedAt >= windowStart)
            .SumAsync(t => t.Amount);

        var totalSpend = recentSpend + transaction.Amount;

        if (totalSpend > Threshold)
            return new FraudRuleResult(true,
                $"R{totalSpend:N2} spent within {WindowMinutes} minutes — " +
                $"exceeds R{Threshold:N2} threshold",
                FraudSeverity.Critical);

        return new FraudRuleResult(false, string.Empty, FraudSeverity.Low);
    }
}

// Rule 2 — International transaction
public class InternationalTransactionRule : IFraudRule
{
    public string AlertType => "INTERNATIONAL_TRANSACTION";

    public Task<FraudRuleResult> EvaluateAsync(
        Transaction transaction, Card card)
    {
        if (transaction.IsInternational &&
            transaction.Type == TransactionType.Purchase)
            return Task.FromResult(new FraudRuleResult(true,
                "International transaction detected",
                FraudSeverity.Medium));

        return Task.FromResult(
            new FraudRuleResult(false, string.Empty, FraudSeverity.Low));
    }
}

// Rule 3 — Rapid purchases (3+ in 1 minute)
public class RapidPurchaseRule : IFraudRule
{
    private readonly IAppDbContext _context;
    private const int MaxPurchases = 3;
    private const int WindowSeconds = 60;

    public RapidPurchaseRule(IAppDbContext context)
    {
        _context = context;
    }

    public string AlertType => "RAPID_PURCHASES";

    public async Task<FraudRuleResult> EvaluateAsync(
        Transaction transaction, Card card)
    {
        if (transaction.Type != TransactionType.Purchase)
            return new FraudRuleResult(false, string.Empty, FraudSeverity.Low);

        var windowStart = DateTime.UtcNow.AddSeconds(-WindowSeconds);

        var recentCount = await _context.Transactions
            .CountAsync(t => t.CardId == card.Id
                && t.Type == TransactionType.Purchase
                && t.CreatedAt >= windowStart);

        if (recentCount >= MaxPurchases)
            return new FraudRuleResult(true,
                $"{recentCount + 1} purchases within {WindowSeconds} seconds",
                FraudSeverity.High);

        return new FraudRuleResult(false, string.Empty, FraudSeverity.Low);
    }
}

// Rule 4 — Multiple merchant categories in 1 minute
public class MultipleMerchantCategoriesRule : IFraudRule
{
    private readonly IAppDbContext _context;

    public MultipleMerchantCategoriesRule(IAppDbContext context)
    {
        _context = context;
    }

    public string AlertType => "MULTIPLE_MERCHANT_CATEGORIES";

    public async Task<FraudRuleResult> EvaluateAsync(
        Transaction transaction, Card card)
    {
        if (transaction.Type != TransactionType.Purchase)
            return new FraudRuleResult(false, string.Empty, FraudSeverity.Low);

        var windowStart = DateTime.UtcNow.AddMinutes(-1);

        var categories = await _context.Transactions
            .Where(t => t.CardId == card.Id
                && t.Type == TransactionType.Purchase
                && t.CreatedAt >= windowStart
                && t.MerchantId != null)
            .Include(t => t.Merchant)
            .Select(t => t.Merchant!.Category)
            .Distinct()
            .CountAsync();

        if (categories >= 3)
            return new FraudRuleResult(true,
                $"Purchases across {categories} merchant categories " +
                $"within 1 minute",
                FraudSeverity.High);

        return new FraudRuleResult(false, string.Empty, FraudSeverity.Low);
    }
}

// Rule 5 — Failed transactions
public class FailedTransactionsRule : IFraudRule
{
    private readonly IAppDbContext _context;
    private const int MaxFailed = 5;

    public FailedTransactionsRule(IAppDbContext context)
    {
        _context = context;
    }

    public string AlertType => "EXCESSIVE_FAILURES";

    public async Task<FraudRuleResult> EvaluateAsync(
        Transaction transaction, Card card)
    {
        var failedCount = await _context.Transactions
            .CountAsync(t => t.CardId == card.Id
                && t.Status == TransactionStatus.Failed);

        if (failedCount >= MaxFailed)
            return new FraudRuleResult(true,
                $"{failedCount} failed transactions detected",
                FraudSeverity.High);

        return new FraudRuleResult(false, string.Empty, FraudSeverity.Low);
    }
}