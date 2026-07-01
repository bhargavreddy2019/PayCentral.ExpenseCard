using PayCentral.Application.Fraud;

namespace PayCentral.Application.Common.Interfaces;

public interface IFraudHubService
{
    Task SendFraudAlertAsync(FraudAlertDto alert);
}