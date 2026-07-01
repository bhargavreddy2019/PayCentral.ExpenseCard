using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayCentral.Application.Transactions;

namespace PayCentral.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] Guid? cardId,
        [FromQuery] string? referenceNumber,
        [FromQuery] string? merchantName,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetTransactionsQuery(
            cardId, referenceNumber, merchantName,
            status, fromDate, toDate, page, pageSize));
        return Ok(result);
    }

    [HttpGet("card/{cardId}")]
    [Authorize(Policy = "AdminOrCardholder")]
    public async Task<IActionResult> GetCardTransactions(
        Guid cardId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetTransactionsQuery(
            cardId, null, null, null, null, null, page, pageSize));
        return Ok(result);
    }

    [HttpGet("card/{cardId}/balance")]
    [Authorize(Policy = "AdminOrCardholder")]
    public async Task<IActionResult> GetBalance(Guid cardId)
    {
        var result = await _mediator.Send(new GetBalanceQuery(cardId));
        return Ok(result);
    }

    [HttpPost("load")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> LoadFunds([FromBody] LoadFundsCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("purchase")]
    [Authorize(Policy = "AdminOrCardholder")]
    public async Task<IActionResult> Purchase([FromBody] PurchaseCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("refund")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Refund([FromBody] RefundCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("reversal")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Reversal([FromBody] ReversalCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}