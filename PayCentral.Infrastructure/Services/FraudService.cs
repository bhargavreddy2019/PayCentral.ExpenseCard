using PayCentral.Application.Common.Interfaces;
using PayCentral.Application.Fraud;
using PayCentral.Domain.Entities;
using PayCentral.Domain.Enums;

namespace PayCentral.Infrastructure.Services;

public class FraudService : IFraudService
{
    private readonly IAppDbContext _context;
    private readonly IFraudHubService _hubService;
    private readonly IEnumerable<IFraudRule> _rules;

    public FraudService(
        IAppDbContext context,
        IFraudHubService hubService,
        IEnumerable<IFraudRule> rules)
    {
        _context = context;
        _hubService = hubService;
        _rules = rules;
    }

    public async Task EvaluateTransactionAsync(Transaction transaction, Card card)
    {
        foreach (var rule in _rules)
        {
            var result = await rule.EvaluateAsync(transaction, card);

            if (!result.IsViolated) continue;

            var alert = new FraudAlert
            {
                CardId = card.Id,
                TransactionId = transaction.Id,
                AlertType = rule.AlertType,
                Reason = result.Reason,
                Severity = result.Severity,
                IsResolved = false,
                CreatedBy = "FraudEngine"
            };

            await _context.FraudAlerts.AddAsync(alert);
            await _context.SaveChangesAsync(default);

            // Push real-time alert via SignalR
            await _hubService.SendFraudAlertAsync(new FraudAlertDto(
                alert.Id,
                card.MaskedCardNumber,
                card.User?.FullName ?? "Unknown",
                alert.AlertType,
                alert.Reason,
                alert.Severity,
                false,
                null,
                null,
                alert.CreatedAt));
        }
    }
}