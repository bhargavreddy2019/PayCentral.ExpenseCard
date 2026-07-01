using Microsoft.AspNetCore.SignalR;
using PayCentral.Application.Common.Interfaces;
using PayCentral.Application.Fraud;

namespace PayCentral.Infrastructure.Services;

public class FraudHubService : IFraudHubService
{
    private readonly IHubContext<Hub> _hubContext;

    public FraudHubService(IHubContext<Hub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendFraudAlertAsync(FraudAlertDto alert)
    {
        await _hubContext.Clients.Group("Admins")
            .SendAsync("ReceiveFraudAlert", alert);
    }
}