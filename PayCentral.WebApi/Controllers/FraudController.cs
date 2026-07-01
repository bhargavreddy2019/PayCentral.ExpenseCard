using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayCentral.Application.Fraud;

namespace PayCentral.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class FraudController : ControllerBase
{
    private readonly IMediator _mediator;

    public FraudController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] bool? isResolved,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(
            new GetFraudAlertsQuery(isResolved, page, pageSize));
        return Ok(result);
    }

    [HttpPut("alerts/{alertId}/resolve")]
    public async Task<IActionResult> ResolveAlert(Guid alertId)
    {
        var result = await _mediator.Send(
            new ResolveFraudAlertCommand(alertId));
        return Ok(result);
    }
}